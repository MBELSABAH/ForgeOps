using System.ComponentModel.DataAnnotations.Schema;

namespace ForgeOps.Web.Models;

public class WorkOrder
{
    public int Id { get; set; }
    [NotMapped] public string ReferenceNumber => $"WO-{Id:D6}";
    public string Title { get; set; } = "";
    public string EquipmentName { get; set; } = "";
    public string Facility { get; set; } = "";
    public string Description { get; set; } = "";
    public WorkOrderPriority Priority { get; set; }
    public WorkOrderStatus Status { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public Technician? AssignedTechnician { get; set; }
    public DateOnly DueDate { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public bool IsArchived { get; set; }
    public List<WorkOrderAudit> History { get; set; } = [];
}

public class Technician
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Specialty { get; set; } = "";
}

public class WorkOrderAudit
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
    public string EventType { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? Notes { get; set; }
    public WorkOrderStatus? FromStatus { get; set; }
    public WorkOrderStatus? ToStatus { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
