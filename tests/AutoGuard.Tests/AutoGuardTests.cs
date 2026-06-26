// <copyright file="AutoGuardTests.cs" company="Canon Europe Limited">
// Copyright (c) Canon Europe Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoGuard;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AutoGuard.Tests;

/// <summary>Sample command used to verify generated guard execution.</summary>
[AutoGuard]
public partial class SampleCommand([NotNull] string id, [InRange(1, 50)] int qty)
{
}

/// <summary>Sample command used to verify generated NotEmpty behavior.</summary>
[AutoGuard]
public partial class NameCommand([NotEmpty] string name)
{
}

/// <summary>Tests emitted generator source.</summary>
public class GeneratorOutputTests
{
    /// <summary>Ensures the attribute source is always emitted.</summary>
    [Fact]
    public void AlwaysEmits_AttributeFile()
        => RunGenerator(string.Empty).Should().ContainKey("AutoGuard.Attribute.g.cs");

    /// <summary>Ensures the validator source is always emitted.</summary>
    [Fact]
    public void AlwaysEmits_ValidatorFile()
        => RunGenerator(string.Empty).Should().ContainKey("AutoGuard.Validator.g.cs");

    /// <summary>Ensures the validator source contains NotNull.</summary>
    [Fact]
    public void ValidatorFile_ContainsNotNull()
        => RunGenerator(string.Empty)["AutoGuard.Validator.g.cs"].Should().Contain("NotNull");

    /// <summary>Ensures the validator source contains InRange.</summary>
    [Fact]
    public void ValidatorFile_ContainsInRange()
        => RunGenerator(string.Empty)["AutoGuard.Validator.g.cs"].Should().Contain("InRange");

    /// <summary>Ensures the validator source contains Matches.</summary>
    [Fact]
    public void ValidatorFile_ContainsMatches()
        => RunGenerator(string.Empty)["AutoGuard.Validator.g.cs"].Should().Contain("Matches");

    /// <summary>Ensures a guarded parameter emits the backing field.</summary>
    [Fact]
    public void NotNullParam_GeneratesField()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([NotNull] string name) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("_autoGuard");
    }

    /// <summary>Ensures NotNull emits the expected validator call.</summary>
    [Fact]
    public void NotNullParam_GeneratesNotNullCall()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([NotNull] string name) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("NotNull");
    }

    /// <summary>Ensures InRange emits the expected validator call.</summary>
    [Fact]
    public void InRangeParam_GeneratesInRangeCall()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([InRange(1, 100)] int count) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("InRange");
    }

    /// <summary>Ensures Matches emits the expected validator call.</summary>
    [Fact]
    public void MatchesParam_GeneratesMatchesCall()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([Matches(@"\d+")] string code) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("Matches");
    }

    /// <summary>Ensures NotEmpty emits the expected validator call.</summary>
    [Fact]
    public void NotEmptyParam_GeneratesNotEmptyCall()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([NotEmpty] string name) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("NotEmpty");
    }

    /// <summary>Ensures multiple guard expressions are chained with &&.</summary>
    [Fact]
    public void MultipleGuards_ChainsWithAnd()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([NotNull] string id, [InRange(1, 100)] int count) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("&&");
    }

    /// <summary>Ensures namespaced types are emitted inside their namespace.</summary>
    [Fact]
    public void NamespacedClass_WrapsInNamespace()
    {
        var source = """
            using AutoGuard;
            namespace MyApp;

            [AutoGuard]
            public partial class Order([NotNull] string id) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("namespace MyApp");
    }

    /// <summary>Ensures non-partial types report GARD001.</summary>
    [Fact]
    public void NonPartialClass_ReportsGARD001()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public class Order([NotNull] string id) { }
            """;

        GetDiagnostics(source).Should().ContainSingle(d => d.Id == "GARD001");
    }

    /// <summary>Ensures non-partial types do not emit a generated file.</summary>
    [Fact]
    public void NonPartialClass_DoesNotGenerateFile()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public class Order([NotNull] string id) { }
            """;

        RunGenerator(source).Should().NotContainKey("AutoGuard.Order.g.cs");
    }

    /// <summary>Ensures valid partial classes do not report GARD001.</summary>
    [Fact]
    public void ValidClass_NoGARD001()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([NotNull] string id) { }
            """;

        GetDiagnostics(source).Should().NotContain(d => d.Id == "GARD001");
    }

    /// <summary>Ensures classes without guarded parameters do not emit a per-type file.</summary>
    [Fact]
    public void ClassWithNoGuardedParams_DoesNotGenerateFile()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order(string id) { }
            """;

        RunGenerator(source).Should().NotContainKey("AutoGuard.Order.g.cs");
    }

    /// <summary>Ensures emitted files contain the auto-generated comment.</summary>
    [Fact]
    public void HasAutoGeneratedComment()
    {
        var source = """
            using AutoGuard;
            [AutoGuard]
            public partial class Order([NotNull] string id) { }
            """;

        RunGenerator(source)["AutoGuard.Order.g.cs"].Should().Contain("// <auto-generated by Swevo.AutoGuard/>");
    }

    private static IReadOnlyList<Diagnostic> GetDiagnostics(string source)
    {
        var compilation = CreateCompilation(source);
        var generator = new AutoGuardGenerator();

        CSharpGeneratorDriver.Create(generator).RunGeneratorsAndUpdateCompilation(
            compilation,
            out _,
            out var diagnostics);

        return diagnostics;
    }

    private static Dictionary<string, string> RunGenerator(string source)
    {
        var compilation = CreateCompilation(source);
        var driver = CSharpGeneratorDriver.Create(new AutoGuardGenerator())
            .RunGenerators(compilation);

        return driver.GetRunResult().GeneratedTrees
            .ToDictionary(
                static tree => Path.GetFileName(tree.FilePath),
                static tree => tree.GetText().ToString());
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        };

        try
        {
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location));
        }
        catch
        {
            // Best effort.
        }

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

/// <summary>Tests the generated runtime behavior.</summary>
public class GeneratedTypeTests
{
    /// <summary>Ensures null values throw <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public void SampleCommand_ThrowsOnNullId()
    {
        var act = () => new SampleCommand(null!, 1);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>Ensures out-of-range values throw <see cref="ArgumentOutOfRangeException"/>.</summary>
    [Fact]
    public void SampleCommand_ThrowsOnOutOfRange()
    {
        var act = () => new SampleCommand("x", 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>Ensures valid values do not throw.</summary>
    [Fact]
    public void SampleCommand_Succeeds_WithValidArgs()
    {
        var act = () => new SampleCommand("x", 25);
        act.Should().NotThrow();
    }

    /// <summary>Ensures empty values trigger the generated NotEmpty validator.</summary>
    [Fact]
    public void NotEmptyCommand_ThrowsOnEmpty()
    {
        var emptyAct = () => new NameCommand(string.Empty);
        var whitespaceAct = () => new NameCommand("   ");

        emptyAct.Should().Throw<ArgumentException>();
        whitespaceAct.Should().Throw<ArgumentException>();
    }
}



