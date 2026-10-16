using System.ComponentModel.DataAnnotations;
using ForgeOps.Web.Models;

namespace ForgeOps.Web.Contracts;

public class WorkOrderInput
{
    [Required, StringLength(120), Display(Name = "Title")]
    public string Title { get; set; } = "";
    [Required, StringLength(120), Display(Name = "Equipment")]
    public string EquipmentName { get; set; } = "";
    [Required, StringLength(100)]
    public string Facility { get; set; } = "";
    [Required, StringLength(2000)]
    public string Description { get; set; } = "";
    [Required, EnumDataType(typeof(WorkOrderPriority))]
    public WorkOrderPriority? Priority { get; set; }
    [Display(Name = "Assigned technician")]
    public int? AssignedTechnicianId { get; set; }
    [Required, DataType(DataType.Date), Display(Name = "Due date")]
    public DateOnly? DueDate { get; set; }
    [StringLength(4000), Display(Name = "Resolution notes")]
    public string? ResolutionNotes { get; set; }
}

public class StatusChangeRequest
{
    [Required, EnumDataType(typeof(WorkOrderStatus))]
    public WorkOrderStatus? Status { get; set; }
    [StringLength(4000)]
    public string? ResolutionNotes { get; set; }
}

public class WorkOrderQuery
{
    [EnumDataType(typeof(WorkOrderStatus))]
    public WorkOrderStatus? Status { get; set; }
    [EnumDataType(typeof(WorkOrderPriority))]
    public WorkOrderPriority? Priority { get; set; }
    [StringLength(120)]
    public string? Search { get; set; }
    public bool Archived { get; set; }
    public bool Overdue { get; set; }
}

public record TechnicianDto(int Id, string Name, string Specialty);
public record AuditDto(int Id, string EventType, string Summary, string? Notes,
    WorkOrderStatus? FromStatus, WorkOrderStatus? ToStatus, DateTime OccurredAtUtc);
public record WorkOrderDto(int Id, string ReferenceNumber, string Title, string EquipmentName,
    string Facility, string Description, WorkOrderPriority Priority, WorkOrderStatus Status,
    int? AssignedTechnicianId, string? AssignedTechnicianName, DateOnly DueDate, string? ResolutionNotes,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc, bool IsArchived, bool IsOverdue, IReadOnlyList<AuditDto> History);
public record DashboardSummaryDto(int Total, int Open, int InProgress, int Resolved, int Overdue,
    DateOnly Today, IReadOnlyDictionary<string, int> ByStatus, IReadOnlyDictionary<string, int> ByPriority,
    IReadOnlyList<WorkOrderDto> RecentlyUpdated);
