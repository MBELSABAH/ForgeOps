using ForgeOps.Web.Models;

namespace ForgeOps.Web.Services;

public static class WorkOrderRules
{
    public static Dictionary<string, string[]> ValidateStatusChange(WorkOrderStatus status, string? resolutionNotes) => new();

    public static bool IsOverdue(DateOnly dueDate, WorkOrderStatus status, bool isArchived, DateOnly today) => false;
}
