using System.Reflection;
using System.Reflection.Emit;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.Architecture.Tests
{

public sealed class ModuleBoundaryTests
{
    private static readonly IReadOnlyDictionary<short, OpCode> OpCodesByValue =
        typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opCode => opCode.Value);

    private static readonly IReadOnlyDictionary<string, string[]> AllowedProjectReferences =
        new Dictionary<string, string[]>
        {
            ["NetWorthTracker.Business"] = [],
            ["NetWorthTracker.Common"] =
            [
                "NetWorthTracker.Business",
                "NetWorthTracker.Data"
            ],
            ["NetWorthTracker.Data"] = ["NetWorthTracker.Business", "NetWorthTracker.Data.Api"],
            ["NetWorthTracker.Data.Api"] = [],
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
    public void RetiredLegacyProjects_AreAbsentFromRegistrationsAndReferences()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var retiredProjects = new[]
        {
            "NetWorthTracker.Models",
            "NetWorthTracker.Services",
            "NetWorthTracker.Services.Api"
        };
        var solutionRegistrations =
            File.ReadAllText(Path.Combine(repositoryRoot, "NetWorthTracker.sln"))
            + File.ReadAllText(Path.Combine(repositoryRoot, "NetWorthTracker.Tests.slnf"));
        var projectReferences = Directory.EnumerateFiles(
                Path.Combine(repositoryRoot, "src"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(
                Path.Combine(repositoryRoot, "tests"),
                "*.csproj",
                SearchOption.AllDirectories))
            .SelectMany(projectFile => System.Xml.Linq.XDocument.Load(projectFile)
                .Descendants()
                .Where(element => element.Name.LocalName == "ProjectReference")
                .Select(element => Path.GetFileNameWithoutExtension(
                    ((string?)element.Attribute("Include") ?? string.Empty)
                    .Replace('\\', Path.DirectorySeparatorChar))))
            .ToHashSet(StringComparer.Ordinal);

        // Act
        var registeredProjects = retiredProjects
            .Where(project => solutionRegistrations.Contains(project, StringComparison.Ordinal))
            .ToArray();
        var referencedProjects = retiredProjects
            .Where(projectReferences.Contains)
            .ToArray();

        // Assert
        Assert.Empty(registeredProjects);
        Assert.Empty(referencedProjects);
    }

    [Fact]
    public void ProjectReferences_FollowThePermittedDependencyGraph()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var sourceProjects = Directory.EnumerateFiles(
                Path.Combine(repositoryRoot, "src"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(projectFile => Path.GetFileNameWithoutExtension(projectFile)!)
            .Order()
            .ToArray();

        // Act
        var mismatches = AllowedProjectReferences
            .Select(entry => new
            {
                Project = entry.Key,
                Expected = entry.Value.Order().ToArray(),
                Actual = ReadProjectReferences(repositoryRoot, entry.Key).Order().ToArray()
            })
            .Where(entry => !entry.Expected.SequenceEqual(entry.Actual))
            .Select(entry =>
                $"{entry.Project}: expected [{string.Join(", ", entry.Expected)}], found [{string.Join(", ", entry.Actual)}]")
            .ToArray();

        // Assert
        Assert.Equal(AllowedProjectReferences.Keys.Order().ToArray(), sourceProjects);
        Assert.Empty(mismatches);
    }

    [Fact]
    public void BusinessProject_HasNoUiMauiEfOrConcretePersistenceDependency()
    {
        // Arrange
        var projectPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "NetWorthTracker.Business",
            "NetWorthTracker.Business.csproj");
        var project = System.Xml.Linq.XDocument.Load(projectPath);

        // Act
        var forbiddenReferences = project.Descendants()
            .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference")
            .Select(element => (string?)element.Attribute("Include") ?? string.Empty)
            .Where(include =>
                include.Contains("NetWorthTracker.UI", StringComparison.OrdinalIgnoreCase)
                || include.Contains("NetWorthTracker.App", StringComparison.OrdinalIgnoreCase)
                || include.Contains("NetWorthTracker.Data", StringComparison.OrdinalIgnoreCase)
                || include.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)
                || include.Contains("Maui", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // Assert
        Assert.Empty(forbiddenReferences);
    }

    [Fact]
    public void BusinessAssembly_HasNoCrossModuleTypeReferencesToInternalOrPersistence()
    {
        // Arrange
        var businessAssembly = typeof(IAccountService).Assembly;

        // Act
        var violations = businessAssembly.GetTypes()
            .SelectMany(source => GetReferencedTypes(source)
                .Where(target => ModuleBoundaryRule.IsForbiddenReference(source, target))
                .Select(target => $"{source.FullName} -> {target.FullName}"))
            .Distinct()
            .ToArray();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public void MethodBodyReferenceScanner_DetectsAnotherModulesInternalImplementation()
    {
        // Arrange
        var consumer = typeof(NetWorthTracker.Business.Features.MonthlyBalances.Internal.AccountInternalConsumer);
        var method = consumer.GetMethod(nameof(
            NetWorthTracker.Business.Features.MonthlyBalances.Internal.AccountInternalConsumer.CreateAccountInternal))!;

        // Act
        var isForbidden = GetMethodBodyReferencedTypes(method)
            .Any(target => ModuleBoundaryRule.IsForbiddenReference(consumer, target));

        // Assert
        Assert.True(isForbidden);
    }

    [Fact]
    public void ModuleBoundaryRule_AllowsAnotherModulesPublicApi()
    {
        // Arrange
        var consumer = typeof(NetWorthTracker.Business.Features.MonthlyBalances.Internal.AccountApiConsumer);

        // Act
        var isForbidden = ModuleBoundaryRule.IsForbiddenReference(consumer, typeof(IAccountService));

        // Assert
        Assert.False(isForbidden);
    }

    [Fact]
    public void ModuleBoundaryRule_RejectsAnotherModulesInternalImplementation()
    {
        // Arrange
        var consumer = typeof(NetWorthTracker.Business.Features.MonthlyBalances.Internal.AccountInternalConsumer);

        // Act
        var isForbidden = ModuleBoundaryRule.IsForbiddenReference(
            consumer,
            typeof(NetWorthTracker.Business.Features.Accounts.Internal.AccountInternalFixture));

        // Assert
        Assert.True(isForbidden);
    }

    [Fact]
    public void ModuleBoundaryRule_RejectsAnotherModulesPersistencePort()
    {
        // Arrange
        var consumer = typeof(NetWorthTracker.Business.Features.MonthlyBalances.Internal.AccountPersistenceConsumer);

        // Act
        var isForbidden = ModuleBoundaryRule.IsForbiddenReference(
            consumer,
            typeof(NetWorthTracker.Business.Features.Accounts.Persistence.AccountPersistenceFixture));

        // Assert
        Assert.True(isForbidden);
    }

    private static IEnumerable<Type> GetReferencedTypes(Type source)
    {
        var types = new List<Type>();

        if (source.BaseType is not null)
        {
            types.Add(source.BaseType);
        }

        types.AddRange(source.GetInterfaces());
        types.AddRange(source.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                        | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Select(field => field.FieldType));
        types.AddRange(source.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                            | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Select(property => property.PropertyType));
        types.AddRange(source.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                        | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(@event => @event.EventHandlerType is not null)
            .Select(@event => @event.EventHandlerType!));

        var methods = source.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                        | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Cast<MethodBase>()
            .Concat(source.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                           | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        foreach (var method in methods)
        {
            if (method is MethodInfo methodInfo)
            {
                types.Add(methodInfo.ReturnType);
            }

            types.AddRange(method.GetParameters().Select(parameter => parameter.ParameterType));
            types.AddRange(method.GetMethodBody()?.LocalVariables.Select(local => local.LocalType) ?? []);
            types.AddRange(GetMethodBodyReferencedTypes(method));
            if (method is MethodInfo genericMethod)
            {
                types.AddRange(genericMethod.GetGenericArguments()
                    .SelectMany(argument => argument.GetGenericParameterConstraints()));
            }
        }

        return types.SelectMany(ExpandType).Where(type => type != source);
    }

    private static IEnumerable<Type> GetMethodBodyReferencedTypes(MethodBase method)
    {
        var il = method.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
        {
            yield break;
        }

        var typeArguments = method.DeclaringType?.GetGenericArguments();
        var methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;
        for (var position = 0; position < il.Length;)
        {
            var firstByte = il[position++];
            var opcodeValue = firstByte == 0xfe
                ? (short)((firstByte << 8) | il[position++])
                : firstByte;
            var opcode = OpCodesByValue[opcodeValue];

            if (opcode.OperandType is OperandType.InlineType
                or OperandType.InlineMethod
                or OperandType.InlineField
                or OperandType.InlineTok)
            {
                var token = BitConverter.ToInt32(il, position);
                var member = method.Module.ResolveMember(token, typeArguments, methodArguments);
                if (member is Type referencedType)
                {
                    yield return referencedType;
                }
                else if (member is MethodBase referencedMethod && referencedMethod.DeclaringType is { } methodType)
                {
                    yield return methodType;
                }
                else if (member is FieldInfo referencedField && referencedField.DeclaringType is { } fieldType)
                {
                    yield return fieldType;
                }
            }

            position += GetOperandSize(opcode.OperandType, il, position);
        }
    }

    private static int GetOperandSize(OperandType operandType, byte[] il, int position)
    {
        return operandType switch
        {
            OperandType.InlineSwitch => sizeof(int) + BitConverter.ToInt32(il, position) * sizeof(int),
            OperandType.InlineI8 or OperandType.InlineR => sizeof(long),
            OperandType.InlineVar => sizeof(short),
            OperandType.InlineI or OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineMethod
                or OperandType.InlineSig or OperandType.InlineString or OperandType.InlineTok or OperandType.InlineType
                or OperandType.ShortInlineR => sizeof(int),
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => sizeof(byte),
            OperandType.InlineNone => 0,
            _ => throw new InvalidOperationException($"Unsupported IL operand type: {operandType}.")
        };
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        yield return type;
        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var element in ExpandType(elementType))
            {
                yield return element;
            }
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var expanded in ExpandType(argument))
                {
                    yield return expanded;
                }
            }
        }
    }

    private static string[] ReadProjectReferences(string repositoryRoot, string projectName)
    {
        var projectFile = Path.Combine(repositoryRoot, "src", projectName, $"{projectName}.csproj");
        return System.Xml.Linq.XDocument.Load(projectFile)
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => Path.GetFileNameWithoutExtension(
                ((string?)element.Attribute("Include") ?? string.Empty)
                .Replace('\\', Path.DirectorySeparatorChar)))
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

internal static class ModuleBoundaryRule
{
    public static bool IsForbiddenReference(Type source, Type target)
    {
        if (!TryGetModule(source.Namespace, out var sourceModule)
            || !TryGetModule(target.Namespace, out var targetModule, out var targetArea))
        {
            return false;
        }

        return !string.Equals(sourceModule, targetModule, StringComparison.Ordinal)
               && targetArea is "Internal" or "Persistence";
    }

    private static bool TryGetModule(string? typeNamespace, out string module)
    {
        return TryGetModule(typeNamespace, out module, out _);
    }

    private static bool TryGetModule(string? typeNamespace, out string module, out string area)
    {
        var segments = typeNamespace?.Split('.') ?? [];
        var featuresIndex = Array.IndexOf(segments, "Features");
        if (featuresIndex < 0 || segments.Length <= featuresIndex + 2)
        {
            module = string.Empty;
            area = string.Empty;
            return false;
        }

        module = segments[featuresIndex + 1];
        area = segments[featuresIndex + 2];
        return true;
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
        public NetWorthTracker.Business.Features.Accounts.Internal.AccountInternalFixture? AccountInternal { get; set; }

        public void CreateAccountInternal()
        {
            _ = new NetWorthTracker.Business.Features.Accounts.Internal.AccountInternalFixture();
        }
    }

    public sealed class AccountPersistenceConsumer
    {
        public NetWorthTracker.Business.Features.Accounts.Persistence.AccountPersistenceFixture? AccountPersistence { get; set; }
    }
}

namespace NetWorthTracker.Business.Features.Accounts.Internal
{
    public sealed class AccountInternalFixture
    {
    }
}

namespace NetWorthTracker.Business.Features.Accounts.Persistence
{
    public sealed class AccountPersistenceFixture
    {
    }
}
