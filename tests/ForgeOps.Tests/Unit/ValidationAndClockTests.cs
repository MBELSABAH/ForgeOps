using ForgeOps.Web.Contracts;
using ForgeOps.Web.Models;
using ForgeOps.Web.Services;

namespace ForgeOps.Tests.Unit;

public class ValidationAndClockTests
{
    [Theory]
    [InlineData(nameof(WorkOrderInput.Title))]
    [InlineData(nameof(WorkOrderInput.EquipmentName))]
    [InlineData(nameof(WorkOrderInput.Facility))]
    [InlineData(nameof(WorkOrderInput.Description))]
    public void RequiredText_RejectsWhitespace(string property)
    {
        var input = ValidInput();
        typeof(WorkOrderInput).GetProperty(property)!.SetValue(input, "   ");
        Assert.Contains(property, WorkOrderRules.ValidateObject(input).Keys);
    }

    [Theory]
    [InlineData(nameof(WorkOrderInput.DueDate))]
    [InlineData(nameof(WorkOrderInput.Priority))]
    public void RequiredValues_RejectNull(string property)
    {
        var input = ValidInput();
        typeof(WorkOrderInput).GetProperty(property)!.SetValue(input, null);
        Assert.Contains(property, WorkOrderRules.ValidateObject(input).Keys);
    }

    [Fact]
    public void RequiredText_EnforcesMaximumLength()
    {
        var input = ValidInput();
        input.Title = new string('x', 121);
        Assert.Contains("Title", WorkOrderRules.ValidateObject(input).Keys);
    }

    [Fact]
    public void UndefinedPriority_IsRejected()
    {
        var input = ValidInput();
        input.Priority = (WorkOrderPriority)999;
        Assert.Contains("Priority", WorkOrderRules.ValidateObject(input).Keys);
    }

    [Fact]
    public void UndefinedStatus_IsRejected() =>
        Assert.Contains("Status", WorkOrderRules.ValidateStatusChange((WorkOrderStatus)999, null).Keys);

    [Fact]
    public void OverlongResolutionNotes_AreRejected() =>
        Assert.Contains("ResolutionNotes", WorkOrderRules.ValidateStatusChange(WorkOrderStatus.Resolved, new string('n', 4001)).Keys);

    [Fact]
    public void ValidDetails_AreAccepted() => Assert.Empty(WorkOrderRules.ValidateObject(ValidInput()));

    [Theory]
    [InlineData("2026-10-16T02:59:59Z", "2026-10-15")]
    [InlineData("2026-10-16T03:00:00Z", "2026-10-16")]
    [InlineData("2026-12-16T03:59:59Z", "2026-12-15")]
    [InlineData("2026-12-16T04:00:00Z", "2026-12-16")]
    public void Today_UsesHalifaxCalendarIncludingDaylightSaving(string utc, string expected)
    {
        var clock = new OperationsClock(new FixedTimeProvider(DateTimeOffset.Parse(utc)));
        Assert.Equal(DateOnly.Parse(expected), clock.Today);
    }

    public static WorkOrderInput ValidInput(string title = "Inspect hydraulic press") => new() {
        Title = title, EquipmentName = "Press HP-01", Facility = "Fictional test plant",
        Description = "Inspect drive coupling and verify alignment.", Priority = WorkOrderPriority.High,
        DueDate = new DateOnly(2026, 10, 14)
    };
}

public class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
