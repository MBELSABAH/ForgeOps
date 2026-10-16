using ForgeOps.Web.Contracts;
using ForgeOps.Web.Models;

namespace ForgeOps.Web.ViewModels;

public record WorkOrderListViewModel(IReadOnlyList<WorkOrderDto> Orders, WorkOrderQuery Query);

public class WorkOrderFormViewModel
{
    public int? Id { get; set; }
    public WorkOrderStatus Status { get; set; }
    public WorkOrderInput Input { get; set; } = new();
    public IReadOnlyList<TechnicianDto> Technicians { get; set; } = [];
}
