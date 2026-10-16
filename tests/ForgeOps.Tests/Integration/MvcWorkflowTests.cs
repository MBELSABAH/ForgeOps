using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ForgeOps.Web.Contracts;
using ForgeOps.Web.Data;
using ForgeOps.Web.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ForgeOps.Tests.Integration;

public class MvcWorkflowTests
{
    private static async Task<string> Token(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "Expected an antiforgery token in the MVC form.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static Dictionary<string, string> EditFields(WorkOrderDto order, string token) => new() {
        ["__RequestVerificationToken"] = token,
        ["Input.Title"] = order.Title, ["Input.EquipmentName"] = order.EquipmentName,
        ["Input.Facility"] = order.Facility, ["Input.Description"] = order.Description,
        ["Input.Priority"] = order.Priority.ToString(), ["Input.DueDate"] = order.DueDate.ToString("yyyy-MM-dd"),
        ["Input.ResolutionNotes"] = order.ResolutionNotes ?? ""
    };

    [Fact]
    public async Task MvcCreateAndAssign_WritesDatabase_AndAudit()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        int technicianId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ForgeOpsDbContext>();
            var tech = new Technician { Name = "Test Technician", Specialty = "Mechanical" };
            db.Technicians.Add(tech);
            await db.SaveChangesAsync();
            technicianId = tech.Id;
        }
        var token = await Token(client, "/WorkOrders/Create");
        var fields = new Dictionary<string, string> {
            ["__RequestVerificationToken"] = token, ["Input.Title"] = "Inspect motor",
            ["Input.EquipmentName"] = "Motor M-01", ["Input.Facility"] = "Test plant",
            ["Input.Description"] = "Check bearing condition.", ["Input.Priority"] = "High",
            ["Input.DueDate"] = "2026-10-17"
        };
        var response = await client.PostAsync("/WorkOrders/Create", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var orders = (await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders", WorkOrderApiTests.Json))!;
        var order = Assert.Single(orders);
        token = await Token(client, "/WorkOrders/Edit/" + order.Id);
        fields = EditFields(order, token);
        fields["Input.AssignedTechnicianId"] = technicianId.ToString();
        response = await client.PostAsync("/WorkOrders/Edit/" + order.Id, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var assigned = await WorkOrderApiTests.Get(client, order.Id);
        Assert.Equal("Test Technician", assigned.AssignedTechnicianName);
        Assert.Contains(assigned.History, x => x.EventType == "Assigned");
    }

    [Fact]
    public async Task InvalidMvcEdit_PreservesAllStoredFields()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var order = await WorkOrderApiTests.Create(client);
        await WorkOrderApiTests.Status(client, order.Id, WorkOrderStatus.Resolved, "Verified successful repair.");
        var before = await WorkOrderApiTests.Get(client, order.Id);
        var fields = EditFields(before, await Token(client, "/WorkOrders/Edit/" + order.Id));
        fields["Input.Title"] = "THIS MUST NOT BE STORED";
        fields["Input.ResolutionNotes"] = "   ";
        var response = await client.PostAsync("/WorkOrders/Edit/" + order.Id, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Add resolution notes", await response.Content.ReadAsStringAsync());
        var after = await WorkOrderApiTests.Get(client, order.Id);
        Assert.Equal(before.Title, after.Title);
        Assert.Equal(before.ResolutionNotes, after.ResolutionNotes);
        Assert.Equal(before.UpdatedAtUtc, after.UpdatedAtUtc);
        Assert.Equal(before.History.Count, after.History.Count);
    }

    [Fact]
    public async Task ArchiveAndRestore_PreserveHistory_ExcludeMetrics_AndPreventWrites()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var order = await WorkOrderApiTests.Create(client);
        var token = await Token(client, "/WorkOrders/Details/" + order.Id);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/WorkOrders/Archive/" + order.Id,
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }))).StatusCode);
        var archived = await WorkOrderApiTests.Get(client, order.Id);
        Assert.True(archived.IsArchived);
        Assert.Equal(2, archived.History.Count);
        Assert.Empty((await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders", WorkOrderApiTests.Json))!);
        Assert.Single((await client.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders?archived=true", WorkOrderApiTests.Json))!);
        var summary = (await client.GetFromJsonAsync<DashboardSummaryDto>("/api/dashboard/summary", WorkOrderApiTests.Json))!;
        Assert.Equal(0, summary.Total);
        Assert.Equal(0, summary.Overdue);
        Assert.Equal(HttpStatusCode.BadRequest, (await WorkOrderApiTests.Status(client, order.Id, WorkOrderStatus.InProgress)).StatusCode);
        Assert.Equal(2, (await WorkOrderApiTests.Get(client, order.Id)).History.Count);
        var html = await client.GetStringAsync("/WorkOrders/Details/" + order.Id);
        Assert.DoesNotContain("id=\"status-form\"", html);
        token = await Token(client, "/WorkOrders/Details/" + order.Id);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/WorkOrders/Restore/" + order.Id,
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }))).StatusCode);
        var restored = await WorkOrderApiTests.Get(client, order.Id);
        Assert.False(restored.IsArchived);
        Assert.Equal(3, restored.History.Count);
        Assert.Equal(WorkOrderStatus.Open, restored.Status);
    }

    [Fact]
    public async Task MvcWrites_RequireAntiforgery_AndViewsEncodeUserInput()
    {
        using var factory = new ForgeOpsFactory();
        using var client = factory.Client();
        var input = Unit.ValidationAndClockTests.ValidInput("<script>alert('x')</script>");
        var order = await WorkOrderApiTests.Create(client, input);
        var response = await client.PostAsync("/WorkOrders/Archive/" + order.Id, new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False((await WorkOrderApiTests.Get(client, order.Id)).IsArchived);
        var html = await client.GetStringAsync("/WorkOrders/Details/" + order.Id);
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>alert", html);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/WorkOrders/Details/99999")).StatusCode);
    }
}
