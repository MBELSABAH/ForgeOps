using System.ComponentModel.DataAnnotations;
using ForgeOps.Web.Models;

namespace ForgeOps.Web.Services;

public static class WorkOrderRules
{
    public static Dictionary<string, string[]> ValidateObject(object input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results.SelectMany(x => x.MemberNames.DefaultIfEmpty("").Select(member => (member, message: x.ErrorMessage!)))
            .GroupBy(x => x.member).ToDictionary(x => x.Key, x => x.Select(y => y.message).ToArray());
    }

    public static Dictionary<string, string[]> ValidateStatusChange(WorkOrderStatus status, string? resolutionNotes)
    {
        var errors = new Dictionary<string, string[]>();
        if (!Enum.IsDefined(status))
            errors["Status"] = ["Choose a valid work-order status."];
        if (resolutionNotes?.Length > 4000)
            errors["ResolutionNotes"] = ["Resolution notes must be 4000 characters or fewer."];
        if (status == WorkOrderStatus.Resolved && string.IsNullOrWhiteSpace(resolutionNotes))
            errors["ResolutionNotes"] = ["Add resolution notes before resolving this work order."];
        return errors;
    }

    public static bool IsOverdue(DateOnly dueDate, WorkOrderStatus status, bool isArchived, DateOnly today) =>
        dueDate < today && status != WorkOrderStatus.Resolved && !isArchived;
}
