using System.Xml.Linq;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Internal;
using NetWorthTracker.Business.Features.Accounts.Persistence;
using NetWorthTracker.Business.Features.MonthlyBalances.Internal;

using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;
using ReflectionAssembly = System.Reflection.Assembly;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace NetWorthTracker.Architecture.Tests
{
    public sealed class ModuleBoundaryTests
    {
        private const string FeaturesNamespace = "NetWorthTracker.Business.Features";
        private static readonly ReflectionAssembly BusinessAssembly = typeof(IAccountService).Assembly;
        private static readonly ReflectionAssembly TestAssembly = typeof(ModuleBoundaryTests).Assembly;

        private static readonly ArchUnitArchitecture Architecture = new ArchLoader()
            .LoadAssemblies(BusinessAssembly, TestAssembly)
            .Build();

        private static readonly IReadOnlyDictionary<string, string[]> AllowedProjectReferences =
            new Dictionary<string, string[]>
            {
                ["NetWorthTracker.Business"] = [],
                ["NetWorthTracker.Common"] =
                [
                    "NetWorthTracker.Business",
                    "NetWorthTracker.Data"
                ],
                ["NetWorthTracker.Data"] = ["NetWorthTracker.Business"],
                ["NetWorthTracker.Seeder"] =
                [
                    "NetWorthTracker.Business",
                    "NetWorthTracker.Common",
                    "NetWorthTracker.Data"
                ],
                ["NetWorthTracker.UI"] = ["NetWorthTracker.Business"],
                ["NetWorthTracker.App"] = ["NetWorthTracker.Common", "NetWorthTracker.UI"]
            };

        [Fact]
        public void Projects_follow_the_permitted_dependency_graph()
        {
            var repositoryRoot = FindRepositoryRoot();
            var sourceProjects = Directory.EnumerateFiles(
                    Path.Combine(repositoryRoot, "src"),
                    "*.csproj",
                    SearchOption.AllDirectories)
                .Select(projectFile => Path.GetFileNameWithoutExtension(projectFile)!)
                .Order()
                .ToArray();

            var mismatches = AllowedProjectReferences.Select(entry => new
                {
                    Project = entry.Key,
                    Expected = entry.Value.Order()
                        .ToArray(),
                    Actual = ReadProjectReferences(repositoryRoot, entry.Key)
                        .Order()
                        .ToArray()
                })
                .Where(entry => !entry.Expected.SequenceEqual(entry.Actual))
                .Select(entry
                    => $"{entry.Project}: expected [{string.Join(", ", entry.Expected)}], found [{string.Join(", ", entry.Actual)}]")
                .ToArray();

            Assert.Multiple(
                () => Assert.Equal(
                    AllowedProjectReferences.Keys.Order()
                        .ToArray(),
                    sourceProjects),
                () => Assert.Empty(mismatches));
        }

        [Fact]
        public void Business_project_has_no_ui_maui_ef_or_concrete_persistence_dependencies()
        {
            var projectPath = Path.Combine(
                FindRepositoryRoot(),
                "src",
                "NetWorthTracker.Business",
                "NetWorthTracker.Business.csproj");
            var project = XDocument.Load(projectPath);

            var forbiddenReferences = project.Descendants()
                .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference")
                .Select(element => (string?)element.Attribute("Include") ?? string.Empty)
                .Where(include => include.Contains("NetWorthTracker.UI", StringComparison.OrdinalIgnoreCase)
                                  || include.Contains("NetWorthTracker.App", StringComparison.OrdinalIgnoreCase)
                                  || include.Contains("NetWorthTracker.Data", StringComparison.OrdinalIgnoreCase)
                                  || include.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)
                                  || include.Contains("Maui", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.Empty(forbiddenReferences);
        }

        [Theory]
        [MemberData(nameof(FeatureModules))]
        public void Business_modules_cannot_reference_other_modules_internals_or_persistence(string module)
        {
            CreateModuleBoundaryRule(
                    module,
                    Types()
                        .That()
                        .ResideInAssembly(BusinessAssembly))
                .Check(Architecture);
        }

        [Fact]
        public void Another_modules_public_api_can_be_referenced()
        {
            var rule = CreateFixtureBoundaryRule(nameof(AccountApiConsumer));

            Assert.True(rule.HasNoViolations(Architecture));
        }

        [Fact]
        public void Same_module_dependencies_are_allowed()
        {
            var sourceTypes = Types()
                .That()
                .ResideInAssembly(TestAssembly)
                .And()
                .HaveNameMatching($"^({nameof(AccountSameModuleConsumer)}|{nameof(AccountApiConsumer)})$");
            var rule = CreateModuleBoundaryRule("Accounts", sourceTypes);

            Assert.Multiple(
                () => Assert.Equal(
                    2,
                    sourceTypes.GetObjects(Architecture)
                        .Count()),
                () => Assert.True(rule.HasNoViolations(Architecture)));
        }

        [Fact]
        public void Another_modules_internal_implementation_cannot_be_referenced()
        {
            var rule = CreateFixtureBoundaryRule(nameof(AccountInternalConsumer));

            Assert.False(rule.HasNoViolations(Architecture));
        }

        [Fact]
        public void Another_modules_persistence_port_cannot_be_referenced()
        {
            var rule = CreateFixtureBoundaryRule(nameof(AccountPersistenceConsumer));

            Assert.False(rule.HasNoViolations(Architecture));
        }

        [Fact]
        public void Method_body_references_to_another_modules_internal_implementation_are_rejected()
        {
            var rule = CreateFixtureBoundaryRule(nameof(MethodBodyAccountInternalConsumer));

            Assert.False(rule.HasNoViolations(Architecture));
        }

        private static IArchRule CreateModuleBoundaryRule(string targetModule, IObjectProvider<IType> sourceTypes)
        {
            var moduleNamespace = $"{FeaturesNamespace}.{targetModule}";

            return Types()
                .That()
                .Are(sourceTypes)
                .And()
                .ResideInNamespaceOrBelow(FeaturesNamespace)
                .And()
                .DoNotResideInNamespaceOrBelow(moduleNamespace)
                .Should()
                .NotDependOnAny(
                    Types()
                        .That()
                        .ResideInNamespaceOrBelow($"{moduleNamespace}.Internal")
                        .Or()
                        .ResideInNamespaceOrBelow($"{moduleNamespace}.Persistence"))
                .Because(
                    $"feature modules must not depend on another module's internal or persistence types ({targetModule})");
        }

        private static IArchRule CreateFixtureBoundaryRule(string sourceTypeName)
        {
            var sourceTypes = Types()
                .That()
                .ResideInAssembly(TestAssembly)
                .And()
                .HaveName(sourceTypeName);
            Assert.Single(sourceTypes.GetObjects(Architecture));

            return CreateModuleBoundaryRule("Accounts", sourceTypes);
        }

        private static string[] GetFeatureModules()
        {
            return BusinessAssembly.GetTypes()
                .Select(type => type.Namespace)
                .Where(typeNamespace
                    => typeNamespace?.StartsWith($"{FeaturesNamespace}.", StringComparison.Ordinal) == true)
                .Select(typeNamespace => typeNamespace!.Split('.')[3])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
        }

        public static IEnumerable<object[]> FeatureModules => GetFeatureModules()
            .Select(module => new object[] { module });

        private static string[] ReadProjectReferences(string repositoryRoot, string projectName)
        {
            var projectFile = Path.Combine(repositoryRoot, "src", projectName, $"{projectName}.csproj");

            return XDocument.Load(projectFile)
                .Descendants()
                .Where(element => element.Name.LocalName == "ProjectReference")
                .Select(element => Path.GetFileNameWithoutExtension(
                    ((string?)element.Attribute("Include") ?? string.Empty).Replace('\\', Path.DirectorySeparatorChar)))
                .Order()
                .ToArray();
        }

        private static string FindRepositoryRoot()
        {
            var startingDirectories = new[]
            {
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                new DirectoryInfo(AppContext.BaseDirectory)
            };

            foreach (var startingDirectory in startingDirectories)
            {
                for (var directory = startingDirectory; directory is not null; directory = directory.Parent)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "NetWorthTracker.sln")))
                    {
                        return directory.FullName;
                    }
                }
            }

            throw new DirectoryNotFoundException("Unable to locate NetWorthTracker.sln.");
        }
    }
}

namespace NetWorthTracker.Business.Features.MonthlyBalances.Internal
{
    public sealed class AccountApiConsumer
    {
        public IAccountService? AccountService { get; set; }
    }

    public sealed class AccountInternalConsumer
    {
        public AccountInternalFixture? AccountInternal { get; set; }
    }

    public sealed class MethodBodyAccountInternalConsumer
    {
        public void CreateAccountInternal()
        {
            _ = new AccountInternalFixture();
        }
    }

    public sealed class AccountPersistenceConsumer
    {
        public AccountPersistenceFixture? AccountPersistence { get; set; }
    }
}

namespace NetWorthTracker.Business.Features.Accounts.Internal
{
    public sealed class AccountInternalFixture
    {
    }

    public sealed class AccountSameModuleConsumer
    {
        public AccountInternalFixture? AccountInternal { get; set; }
    }
}

namespace NetWorthTracker.Business.Features.Accounts.Persistence
{
    public sealed class AccountPersistenceFixture
    {
    }
}
