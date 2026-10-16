using ForgeOps.Web.Models;
using ForgeOps.Web.Services;

namespace ForgeOps.Tests.Unit;

public class WorkOrderRulesTests
{
    [Fact]
    public void PastDue_Unresolved_Unarchived_Order_IsOverdue()
    {
        var today = new DateOnly(2026, 10, 15);
        Assert.True(WorkOrderRules.IsOverdue(today.AddDays(-1), WorkOrderStatus.Open, false, today));
    }

    [Theory]
    [InlineData(-1, WorkOrderStatus.Resolved, false, false)]
    [InlineData(-1, WorkOrderStatus.Open, true, false)]
    [InlineData(0, WorkOrderStatus.Open, false, false)]
    [InlineData(1, WorkOrderStatus.InProgress, false, false)]
    [InlineData(-1, WorkOrderStatus.InProgress, false, true)]
    public void Overdue_RespectsDateStatusAndArchive(int days, WorkOrderStatus status, bool archived, bool expected)
    {
        var today = new DateOnly(2026, 10, 15);
        Assert.Equal(expected, WorkOrderRules.IsOverdue(today.AddDays(days), status, archived, today));
    }

    [Theory]
    [InlineData(WorkOrderStatus.Open, null)]
    [InlineData(WorkOrderStatus.InProgress, null)]
    [InlineData(WorkOrderStatus.Resolved, "Replaced bearing and verified operation.")]
    public void ValidStatusChange_IsAccepted(WorkOrderStatus status, string? notes) =>
        Assert.Empty(WorkOrderRules.ValidateStatusChange(status, notes));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithoutNotes_IsRejected(string? notes)
    {
        var errors = WorkOrderRules.ValidateStatusChange(WorkOrderStatus.Resolved, notes);
        Assert.Contains("ResolutionNotes", errors.Keys);
    }
}
