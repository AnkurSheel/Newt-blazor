using Microsoft.Extensions.Options;

using NetWorthTracker.Common;

namespace NetWorthTracker.App;

public class MauiAppPaths(IOptions<AppPathOptions> options) : DefaultAppPaths(options, FileSystem.AppDataDirectory);
