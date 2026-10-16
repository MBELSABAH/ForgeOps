using ForgeOps.Tests.Unit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ForgeOps.Tests.Integration;

public class ForgeOpsFactory : WebApplicationFactory<Program>
{
    private readonly string? ownedDirectory;
    public string DatabasePath { get; }

    public ForgeOpsFactory(string? databasePath = null)
    {
        if (databasePath is null)
        {
            ownedDirectory = Path.Combine(Path.GetTempPath(), "forgeops-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(ownedDirectory);
            databasePath = Path.Combine(ownedDirectory, "integration.db");
        }
        DatabasePath = databasePath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> {
            ["ConnectionStrings:ForgeOps"] = "Data Source=" + DatabasePath,
            ["Database:Provider"] = "Sqlite",
            ["Database:SeedDemoData"] = "false"
        }));
        builder.ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton<TimeProvider>(
            new FixedTimeProvider(DateTimeOffset.Parse("2026-10-16T01:00:00Z")))));
    }

    public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && ownedDirectory is not null && Directory.Exists(ownedDirectory))
            Directory.Delete(ownedDirectory, recursive: true);
    }
}
