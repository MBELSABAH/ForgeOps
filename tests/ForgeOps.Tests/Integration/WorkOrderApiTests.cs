using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ForgeOps.Tests.Unit;
using ForgeOps.Web.Contracts;
using ForgeOps.Web.Data;
using ForgeOps.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ForgeOps.Tests.Integration;

public class WorkOrderApiTests
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) {
        Converters = { new JsonStringEnumConverter() }
    };

    internal static async Task<WorkOrderDto> Create(HttpClient client, WorkOrderInput? input = null)
    {
        var response = await client.PostAsJsonAsync("/api/workorders", input ?? ValidationAndClockTests.ValidInput(), Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<WorkOrderDto>(Json))!;
        Assert.EndsWith("/api/workorders/" + result.Id, response.Headers.Location!.ToString());
        return result;
    }

    internal static async Task<WorkOrderDto> Get(HttpClient client, int id) =>
        (await client.GetFromJsonAsync<WorkOrderDto>("/api/workorders/" + id, Json))!;

    internal static Task<HttpResponseMessage> Status(HttpClient client, int id, WorkOrderStatus status, string? notes = null) =>
        client.PatchAsJsonAsync("/api/workorders/" + id + "/status", new StatusChangeRequest { Status = status, ResolutionNotes = notes }, Json);

    [Fact]
    public async Task Create_Get_PersistsAcrossApplicationHosts()
    {
        var directory = Path.Combine(Path.GetTempPath(), "forgeops-restart-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "persist.db");
            int id;
            using (var first = new ForgeOpsFactory(path))
            using (var client = first.Client())
            {
                var created = await Create(client);
                id = created.Id;
                Assert.Equal(WorkOrderStatus.Open, created.Status);
                Assert.Single(created.History);
                using var scope = first.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ForgeOpsDbContext>();
                Assert.Equal(created.Title, (await db.WorkOrders.AsNoTracking().SingleAsync()).Title);
            }
            using (var second = new ForgeOpsFactory(path))
            using (var client = second.Client())
            {
                var retrieved = await Get(client, id);
                Assert.Equal("Inspect hydraulic press", retrieved.Title);
                Assert.Equal($"WO-{id:D6}", retrieved.ReferenceNumber);
                Assert.Equal(DateTimeKind.Utc, retrieved.CreatedAtUtc.Kind);
                Assert.Single(await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders", Json) ?? []);
            }
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData("Title")]
    [InlineData("EquipmentName")]
    [InlineData("Facility")]
    [InlineData("Description")]
    public async Task InvalidCreate_Returns400_AndStoresNothing(string field)
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var input = ValidationAndClockTests.ValidInput();
        typeof(WorkOrderInput).GetProperty(field)!.SetValue(input, "  ");
        var response = await client.PostAsJsonAsync("/api/workorders", input, Json);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(field, await response.Content.ReadAsStringAsync());
        using var scope = factory.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ForgeOpsDbContext>().WorkOrders.ToListAsync());
    }

    [Fact]
    public async Task ResolutionWithoutNotes_DoesNotChangeOrderOrAudit()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var before = await Create(client);
        var response = await Status(client, before.Id, WorkOrderStatus.Resolved, "   ");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Add resolution notes", await response.Content.ReadAsStringAsync());
        var after = await Get(client, before.Id);
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.UpdatedAtUtc, after.UpdatedAtUtc);
        Assert.Equal(before.History.Count, after.History.Count);
        Assert.Null(after.ResolutionNotes);
    }

    [Fact]
    public async Task StatusFlow_UpdatesSummary_AuditsChanges_AndPreservesReopenedResolution()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var order = await Create(client);
        Assert.True(order.IsOverdue);
        Assert.Equal(HttpStatusCode.OK, (await Status(client, order.Id, WorkOrderStatus.InProgress)).StatusCode);
        var progress = await Get(client, order.Id);
        Assert.Equal(2, progress.History.Count);
        await Status(client, order.Id, WorkOrderStatus.InProgress);
        Assert.Equal(2, (await Get(client, order.Id)).History.Count);
        const string notes = "Replaced coupling, verified alignment and completed operating check.";
        Assert.Equal(HttpStatusCode.OK, (await Status(client, order.Id, WorkOrderStatus.Resolved, notes)).StatusCode);
        var resolved = await Get(client, order.Id);
        Assert.False(resolved.IsOverdue);
        Assert.Equal(3, resolved.History.Count);
        var summary = (await client.GetFromJsonAsync<DashboardSummaryDto>("/api/dashboard/summary", Json))!;
        Assert.Equal(1, summary.Total);
        Assert.Equal(1, summary.Resolved);
        Assert.Equal(0, summary.Overdue);
        Assert.Equal(summary.Total, summary.ByPriority.Values.Sum());
        Assert.Equal(summary.Total, summary.ByStatus.Values.Sum());
        await Status(client, order.Id, WorkOrderStatus.Open);
        var reopened = await Get(client, order.Id);
        Assert.Null(reopened.ResolutionNotes);
        Assert.True(reopened.IsOverdue);
        Assert.Equal(4, reopened.History.Count);
        Assert.Contains(reopened.History, x => x.EventType == "StatusChanged" && x.Notes == notes);
        Assert.Equal(2, reopened.History.Count(x => x.Notes == notes));
    }

    [Theory]
    [InlineData("\"Broken\"")]
    [InlineData("999")]
    [InlineData("\"999\"")]
    [InlineData("null")]
    public async Task InvalidStatus_Is400_AndDoesNotMutate(string jsonStatus)
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var order = await Create(client);
        var response = await client.PatchAsync("/api/workorders/" + order.Id + "/status",
            new StringContent("{\"status\":" + jsonStatus + "}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Single((await Get(client, order.Id)).History);
    }

    [Fact]
    public async Task MissingRecords_Return404()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/workorders/999")).StatusCode);
        var response = await Status(client, 999, WorkOrderStatus.InProgress);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("not found", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Search_Filters_AndOverdue_AreDatabaseDerived()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var first = await Create(client);
        var secondInput = ValidationAndClockTests.ValidInput("Check coolant pump");
        secondInput.Priority = WorkOrderPriority.Low;
        secondInput.DueDate = new DateOnly(2026, 10, 15); // due today, not overdue
        await Create(client, secondInput);
        await Status(client, first.Id, WorkOrderStatus.InProgress);
        var filtered = await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders?status=InProgress&priority=High&search=PRESS", Json);
        Assert.Equal(first.Id, Assert.Single(filtered!).Id);
        var reference = await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders?search=" + first.ReferenceNumber, Json);
        Assert.Single(reference!);
        Assert.Single((await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders?overdue=true", Json))!);
        var summary = (await client.GetFromJsonAsync<DashboardSummaryDto>("/api/dashboard/summary", Json))!;
        Assert.Equal(2, summary.Total);
        Assert.Equal(1, summary.Overdue);
        Assert.Equal(1, summary.Open);
        Assert.Equal(1, summary.InProgress);
        Assert.Equal(2, summary.RecentlyUpdated.Count);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/workorders?status=Unknown")).StatusCode);
    }

    [Fact]
    public async Task InvalidTechnician_IsRejected_AndApiRequiresJson()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var input = ValidationAndClockTests.ValidInput();
        input.AssignedTechnicianId = 98765;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/workorders", input, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType,
            (await client.PostAsync("/api/workorders", new StringContent("{}", Encoding.UTF8, "text/plain"))).StatusCode);
    }
}
