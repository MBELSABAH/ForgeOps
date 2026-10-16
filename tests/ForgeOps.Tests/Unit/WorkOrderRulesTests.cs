using ForgeOps.Web.Models;
using ForgeOps.Web.Services;

namespace ForgeOps.Tests.Unit;

public class WorkOrderRulesTests
{
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
