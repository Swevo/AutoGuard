// <copyright file="AutoGuardGenerator.cs" company="Canon Europe Limited">
// Copyright (c) Canon Europe Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoGuard;

/// <summary>Generates constructor guard validation for types decorated with <c>[AutoGuard]</c>.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class AutoGuardGenerator : IIncrementalGenerator
{
    private const string AttributeFqn = "AutoGuard.AutoGuardAttribute";
    private const string NotNullAttributeFqn = "AutoGuard.NotNullAttribute";
    private const string NotEmptyAttributeFqn = "AutoGuard.NotEmptyAttribute";
    private const string InRangeAttributeFqn = "AutoGuard.InRangeAttribute";
    private const string MatchesAttributeFqn = "AutoGuard.MatchesAttribute";

    private static readonly DiagnosticDescriptor Gard001 = new(
        id: "GARD001",
        title: "Type must be partial",
        messageFormat: "'{0}' is decorated with [AutoGuard] but is not declared as partial. Declare it as partial to allow source generation.",
        category: "AutoGuard",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource("AutoGuard.Attribute.g.cs", SourceText.From(Emitter.AttributeSource, System.Text.Encoding.UTF8));
            ctx.AddSource("AutoGuard.Validator.g.cs", SourceText.From(Emitter.ValidatorSource, System.Text.Encoding.UTF8));
        });

        var candidates = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => CreateModel(ctx))
            .WithTrackingName("AutoGuardCandidates");

        context.RegisterSourceOutput(candidates, static (spc, model) =>
        {
            if (!model.IsPartial)
            {
                spc.ReportDiagnostic(Diagnostic.Create(Gard001, model.IdentifierLocation, model.TypeName));
                return;
            }

            if (model.Validations.Count == 0)
            {
                return;
            }

            spc.AddSource($"AutoGuard.{model.TypeName}.g.cs", SourceText.From(Emitter.Emit(model), System.Text.Encoding.UTF8));
        });
    }

    private static GuardedTypeModel CreateModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (ClassDeclarationSyntax)context.TargetNode;
        var symbol = (INamedTypeSymbol)context.TargetSymbol;
        var validations = syntax.ParameterList is null
            ? new List<string>()
            : GetValidations(context.SemanticModel, syntax.ParameterList.Parameters);

        return new GuardedTypeModel
        {
            Accessibility = GetAccessibility(symbol.DeclaredAccessibility),
            Constraints = GetConstraints(syntax),
            IdentifierLocation = syntax.Identifier.GetLocation(),
            IsPartial = syntax.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.PartialKeyword)),
            Namespace = symbol.ContainingNamespace?.IsGlobalNamespace == false ? symbol.ContainingNamespace.ToDisplayString() : string.Empty,
            TypeName = symbol.Name,
            TypeParameters = syntax.TypeParameterList?.ToFullString().Trim() ?? string.Empty,
            Validations = validations,
        };
    }

    private static List<string> GetValidations(SemanticModel semanticModel, SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        var validations = new List<string>();

        foreach (var parameterSyntax in parameters)
        {
            if (semanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            {
                continue;
            }

            foreach (var attributeData in parameterSymbol.GetAttributes())
            {
                var expression = CreateValidationExpression(parameterSymbol, attributeData);
                if (!string.IsNullOrEmpty(expression))
                {
                    validations.Add(expression);
                }
            }
        }

        return validations;
    }

    private static string CreateValidationExpression(IParameterSymbol parameterSymbol, AttributeData attributeData)
    {
        var attributeName = attributeData.AttributeClass?.ToDisplayString();
        var parameterName = parameterSymbol.Name;
        var parameterNameLiteral = SymbolDisplay.FormatLiteral(parameterName, true);

        return attributeName switch
        {
            NotNullAttributeFqn => $"global::AutoGuard.AutoGuardValidator.NotNull({parameterName}, {parameterNameLiteral})",
            NotEmptyAttributeFqn => CreateNotEmptyExpression(parameterSymbol, parameterName, parameterNameLiteral),
            InRangeAttributeFqn => $"global::AutoGuard.AutoGuardValidator.InRange((double){parameterName}, {FormatDouble(attributeData.ConstructorArguments[0].Value)}, {FormatDouble(attributeData.ConstructorArguments[1].Value)}, {parameterNameLiteral})",
            MatchesAttributeFqn => $"global::AutoGuard.AutoGuardValidator.Matches({parameterName}, {SymbolDisplay.FormatLiteral((string)attributeData.ConstructorArguments[0].Value!, true)}, {parameterNameLiteral})",
            _ => string.Empty,
        };
    }

    private static string CreateNotEmptyExpression(IParameterSymbol parameterSymbol, string parameterName, string parameterNameLiteral)
    {
        if (parameterSymbol.Type.SpecialType == SpecialType.System_String)
        {
            return $"global::AutoGuard.AutoGuardValidator.NotEmpty({parameterName}, {parameterNameLiteral})";
        }

        if (!TryGetEnumerableElementType(parameterSymbol.Type, out var elementType))
        {
            return string.Empty;
        }

        return $"global::AutoGuard.AutoGuardValidator.NotEmpty<{elementType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>({parameterName}, {parameterNameLiteral})";
    }

    private static bool TryGetEnumerableElementType(ITypeSymbol typeSymbol, out ITypeSymbol? elementType)
    {
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            elementType = arrayType.ElementType;
            return true;
        }

        var enumerableInterface = typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                ? namedType
                : typeSymbol.AllInterfaces.FirstOrDefault(static candidate => candidate.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

        elementType = enumerableInterface?.TypeArguments[0];
        return elementType is not null;
    }

    private static string GetAccessibility(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Internal => "internal",
        Accessibility.Private => "private",
        Accessibility.Protected => "protected",
        Accessibility.ProtectedAndInternal => "private protected",
        Accessibility.ProtectedOrInternal => "protected internal",
        _ => "internal",
    };

    private static string GetConstraints(ClassDeclarationSyntax syntax)
    {
        if (syntax.ConstraintClauses.Count == 0)
        {
            return string.Empty;
        }

        return "\n" + string.Join("\n", syntax.ConstraintClauses.Select(static clause => clause.ToFullString().Trim()));
    }

    private static string FormatDouble(object? value)
    {
        var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        return number.ToString("R", CultureInfo.InvariantCulture);
    }
}

internal sealed class GuardedTypeModel
{
    public string Accessibility { get; set; } = "internal";

    public string Constraints { get; set; } = string.Empty;

    public Location IdentifierLocation { get; set; } = Location.None;

    public bool IsPartial { get; set; }

    public string Namespace { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public string TypeParameters { get; set; } = string.Empty;

    public IReadOnlyList<string> Validations { get; set; } = Array.Empty<string>();
}





