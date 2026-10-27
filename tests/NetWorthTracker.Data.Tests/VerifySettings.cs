using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DiffEngine;

namespace NetWorthTracker.Data.Tests;

public static partial class VerifySettings
{
    [GeneratedRegex(@"@__\w*locals\d+_")]
    private static partial Regex ClosureParameterRegex();

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DerivePathInfo((
            sourceFile,
            projectDirectory,
            type,
            method
        ) => new PathInfo(Path.Combine(projectDirectory, "Snapshots"), type.Name, method.Name));

        VerifierSettings.ScrubLinesWithReplace(line => ClosureParameterRegex()
            .Replace(line, "@__"));

        DiffRunner.Disabled = true;
        VerifierSettings.OnFirstVerify(filePair =>
        {
            AutoApproveFile(filePair);

            return Task.CompletedTask;
        });
        VerifierSettings.OnVerifyMismatch((filePair, message) =>
        {
            AutoApproveFile(filePair);

            return Task.CompletedTask;
        });
    }

    private static void AutoApproveFile(FilePair filePair)
    {
        if (File.Exists(filePair.VerifiedPath))
        {
            File.Delete(filePair.VerifiedPath);
        }
        File.Copy(filePair.ReceivedPath, filePair.VerifiedPath);
    }
}
