using System.Text.Json.Serialization;
using ForgeOps.Web.Data;
using ForgeOps.Web.Infrastructure;
using ForgeOps.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var exportSqlServer = args.Contains("--export-sqlserver-schema");
var builder = WebApplication.CreateBuilder(args.Where(x => x != "--export-sqlserver-schema").ToArray());
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<OperationsClock>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddDbContext<ForgeOpsDbContext>((services, options) =>
{
    var config = services.GetRequiredService<IConfiguration>();
    var provider = exportSqlServer ? "SqlServer" : config["Database:Provider"] ?? "Sqlite";
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(config.GetConnectionString("SqlServer") ??
            "Server=localhost;Database=ForgeOps;Integrated Security=true;TrustServerCertificate=true");
    else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var connection = new SqliteConnectionStringBuilder(config.GetConnectionString("ForgeOps") ?? "Data Source=App_Data/forgeops.db");
        if (connection.DataSource != ":memory:" && !Path.IsPathRooted(connection.DataSource))
            connection.DataSource = Path.GetFullPath(connection.DataSource, builder.Environment.ContentRootPath);
        if (connection.DataSource != ":memory:") Directory.CreateDirectory(Path.GetDirectoryName(connection.DataSource)!);
        options.UseSqlite(connection.ToString());
    }
    else throw new InvalidOperationException($"Unknown database provider '{provider}'. Use Sqlite or SqlServer.");
});
var app = builder.Build();
if (exportSqlServer)
{
    using var scope = app.Services.CreateScope();
    Console.WriteLine(scope.ServiceProvider.GetRequiredService<ForgeOpsDbContext>().Database.GenerateCreateScript());
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePagesWithReExecute("/Home/HttpError", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
if (app.Environment.IsDevelopment()) app.MapOpenApi();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ForgeOpsDbContext>();
    // SQLite migrations must never be applied to a SQL Server database.
    if (db.Database.IsSqlite()) await db.Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("Database:SeedDemoData"))
        await DemoSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<OperationsClock>());
}
await app.RunAsync();

public partial class Program;
