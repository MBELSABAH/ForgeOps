using ForgeOps.Web.Contracts;
using ForgeOps.Web.Data;
using ForgeOps.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ForgeOps.Web.Services;

public interface IWorkOrderService
{
    Task<IReadOnlyList<WorkOrderDto>> ListAsync(WorkOrderQuery query, CancellationToken ct = default);
    Task<WorkOrderDto?> GetAsync(int id, CancellationToken ct = default);
    Task<WorkOrderDto> CreateAsync(WorkOrderInput input, CancellationToken ct = default);
    Task<WorkOrderDto> UpdateAsync(int id, WorkOrderInput input, CancellationToken ct = default);
    Task<WorkOrderDto> ChangeStatusAsync(int id, StatusChangeRequest input, CancellationToken ct = default);
    Task SetArchivedAsync(int id, bool archived, CancellationToken ct = default);
    Task<IReadOnlyList<TechnicianDto>> GetTechniciansAsync(CancellationToken ct = default);
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}

public class WorkOrderService(ForgeOpsDbContext db, OperationsClock clock, ILogger<WorkOrderService> logger) : IWorkOrderService
{
    public async Task<IReadOnlyList<WorkOrderDto>> ListAsync(WorkOrderQuery query, CancellationToken ct = default)
    {
        ThrowIfInvalid(WorkOrderRules.ValidateObject(query));
        var orders = db.WorkOrders.AsNoTracking().Include(x => x.AssignedTechnician)
            .Where(x => x.IsArchived == query.Archived);
        if (query.Status.HasValue) orders = orders.Where(x => x.Status == query.Status);
        if (query.Priority.HasValue) orders = orders.Where(x => x.Priority == query.Priority);
        if (query.Overdue)
        {
            var today = clock.Today;
            orders = orders.Where(x => !x.IsArchived && x.Status != WorkOrderStatus.Resolved && x.DueDate < today);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            var reference = search.StartsWith("wo-") ? search[3..] : search;
            var id = int.TryParse(reference, out var parsed) ? parsed : -1;
            orders = orders.Where(x => x.Title.ToLower().Contains(search) ||
                x.EquipmentName.ToLower().Contains(search) || x.Id == id);
        }
        var rows = await orders.OrderByDescending(x => x.UpdatedAtUtc).ThenByDescending(x => x.Id).ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<WorkOrderDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var order = await db.WorkOrders.AsNoTracking().Include(x => x.AssignedTechnician)
            .Include(x => x.History).SingleOrDefaultAsync(x => x.Id == id, ct);
        return order is null ? null : ToDto(order);
    }

    public async Task<WorkOrderDto> CreateAsync(WorkOrderInput input, CancellationToken ct = default)
    {
        await ValidateDetailsAsync(input, ct);
        var order = new WorkOrder { Status = WorkOrderStatus.Open, CreatedAtUtc = clock.UtcNow, UpdatedAtUtc = clock.UtcNow };
        ApplyDetails(order, input);
        AddAudit(order, "Created", "Work order created.", notes: order.ResolutionNotes);
        db.WorkOrders.Add(order);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created work order {WorkOrderId}", order.Id);
        return (await GetAsync(order.Id, ct))!;
    }

    public async Task<WorkOrderDto> UpdateAsync(int id, WorkOrderInput input, CancellationToken ct = default)
    {
        var order = await FindForWriteAsync(id, ct);
        EnsureEditable(order);
        await ValidateDetailsAsync(input, ct);
        if (order.Status == WorkOrderStatus.Resolved)
            ThrowIfInvalid(WorkOrderRules.ValidateStatusChange(order.Status, input.ResolutionNotes));
        var changed = new List<string>();
        if (order.Title != input.Title.Trim()) changed.Add("title");
        if (order.EquipmentName != input.EquipmentName.Trim()) changed.Add("equipment");
        if (order.Facility != input.Facility.Trim()) changed.Add("facility");
        if (order.Description != input.Description.Trim()) changed.Add("description");
        if (order.Priority != input.Priority) changed.Add("priority");
        if (order.DueDate != input.DueDate) changed.Add("due date");
        if (order.ResolutionNotes != NormalizeNotes(input.ResolutionNotes)) changed.Add("resolution notes");
        if (order.AssignedTechnicianId != input.AssignedTechnicianId)
        {
            var technician = input.AssignedTechnicianId.HasValue
                ? await db.Technicians.FindAsync([input.AssignedTechnicianId.Value], ct) : null;
            AddAudit(order, "Assigned", $"Assignment changed from {order.AssignedTechnician?.Name ?? "Unassigned"} to {technician?.Name ?? "Unassigned"}.");
            changed.Add("technician");
        }
        if (changed.Count == 0) return ToDto(order);
        ApplyDetails(order, input);
        order.UpdatedAtUtc = clock.UtcNow;
        AddAudit(order, "Updated", $"Updated {string.Join(", ", changed)}.", notes: order.ResolutionNotes);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(id, ct))!;
    }

    public async Task<WorkOrderDto> ChangeStatusAsync(int id, StatusChangeRequest input, CancellationToken ct = default)
    {
        var order = await FindForWriteAsync(id, ct);
        EnsureEditable(order);
        ThrowIfInvalid(WorkOrderRules.ValidateObject(input));
        ThrowIfInvalid(WorkOrderRules.ValidateStatusChange(input.Status!.Value, input.ResolutionNotes));
        var previous = order.Status;
        var notes = input.Status == WorkOrderStatus.Resolved
            ? NormalizeNotes(input.ResolutionNotes)
            : previous == WorkOrderStatus.Resolved ? null : order.ResolutionNotes;
        if (previous == input.Status && order.ResolutionNotes == notes) return ToDto(order);
        AddAudit(order, previous == input.Status ? "Updated" : "StatusChanged",
            previous == input.Status ? "Updated resolution notes." : $"Status changed from {Label(previous)} to {Label(input.Status.Value)}.",
            previous, input.Status.Value, input.Status == WorkOrderStatus.Resolved ? notes : order.ResolutionNotes);
        order.Status = input.Status.Value;
        order.ResolutionNotes = notes;
        order.UpdatedAtUtc = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Work order {WorkOrderId} changed from {PreviousStatus} to {Status}", id, previous, order.Status);
        return (await GetAsync(id, ct))!;
    }

    public async Task SetArchivedAsync(int id, bool archived, CancellationToken ct = default)
    {
        var order = await FindForWriteAsync(id, ct);
        if (order.IsArchived == archived) return;
        order.IsArchived = archived;
        order.UpdatedAtUtc = clock.UtcNow;
        AddAudit(order, archived ? "Archived" : "Restored", archived ? "Archived; excluded from operational metrics." : "Restored to the operational queue.");
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TechnicianDto>> GetTechniciansAsync(CancellationToken ct = default) =>
        await db.Technicians.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new TechnicianDto(x.Id, x.Name, x.Specialty)).ToListAsync(ct);

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var orders = db.WorkOrders.AsNoTracking().Where(x => !x.IsArchived);
        var statusCounts = await orders.GroupBy(x => x.Status).Select(x => new { Key = x.Key, Count = x.Count() }).ToListAsync(ct);
        var priorityCounts = await orders.GroupBy(x => x.Priority).Select(x => new { Key = x.Key, Count = x.Count() }).ToListAsync(ct);
        var byStatus = Enum.GetValues<WorkOrderStatus>().ToDictionary(x => x.ToString(), x => statusCounts.SingleOrDefault(y => y.Key == x)?.Count ?? 0);
        var byPriority = Enum.GetValues<WorkOrderPriority>().ToDictionary(x => x.ToString(), x => priorityCounts.SingleOrDefault(y => y.Key == x)?.Count ?? 0);
        var today = clock.Today;
        var overdue = await orders.CountAsync(x => x.Status != WorkOrderStatus.Resolved && x.DueDate < today, ct);
        var recent = await orders.Include(x => x.AssignedTechnician).OrderByDescending(x => x.UpdatedAtUtc)
            .ThenByDescending(x => x.Id).Take(5).ToListAsync(ct);
        return new(byStatus.Values.Sum(), byStatus["Open"], byStatus["InProgress"], byStatus["Resolved"], overdue,
            today, byStatus, byPriority, recent.Select(ToDto).ToList());
    }

    private async Task<WorkOrder> FindForWriteAsync(int id, CancellationToken ct) =>
        await db.WorkOrders.Include(x => x.AssignedTechnician).Include(x => x.History)
            .SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new WorkOrderNotFoundException(id);

    private async Task ValidateDetailsAsync(WorkOrderInput input, CancellationToken ct)
    {
        var errors = WorkOrderRules.ValidateObject(input);
        if (input.AssignedTechnicianId.HasValue && !await db.Technicians.AnyAsync(x => x.Id == input.AssignedTechnicianId, ct))
            errors["AssignedTechnicianId"] = ["Choose a technician from the roster."];
        ThrowIfInvalid(errors);
    }

    private static void EnsureEditable(WorkOrder order)
    {
        if (order.IsArchived)
            throw new WorkOrderValidationException(new() { ["WorkOrder"] = ["Restore this archived order before making changes."] });
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0) throw new WorkOrderValidationException(errors);
    }

    private static string? NormalizeNotes(string? notes) => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private static void ApplyDetails(WorkOrder order, WorkOrderInput input)
    {
        order.Title = input.Title.Trim();
        order.EquipmentName = input.EquipmentName.Trim();
        order.Facility = input.Facility.Trim();
        order.Description = input.Description.Trim();
        order.Priority = input.Priority!.Value;
        order.AssignedTechnicianId = input.AssignedTechnicianId;
        order.DueDate = input.DueDate!.Value;
        order.ResolutionNotes = NormalizeNotes(input.ResolutionNotes);
    }

    private void AddAudit(WorkOrder order, string type, string summary, WorkOrderStatus? from = null,
        WorkOrderStatus? to = null, string? notes = null) =>
        order.History.Add(new() { EventType = type, Summary = summary, FromStatus = from,
            ToStatus = to, Notes = notes, OccurredAtUtc = clock.UtcNow });

    private WorkOrderDto ToDto(WorkOrder order) =>
        new(order.Id, order.ReferenceNumber, order.Title, order.EquipmentName, order.Facility, order.Description,
            order.Priority, order.Status, order.AssignedTechnicianId, order.AssignedTechnician?.Name, order.DueDate,
            order.ResolutionNotes, order.CreatedAtUtc, order.UpdatedAtUtc, order.IsArchived,
            WorkOrderRules.IsOverdue(order.DueDate, order.Status, order.IsArchived, clock.Today),
            order.History.OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id)
                .Select(x => new AuditDto(x.Id, x.EventType, x.Summary, x.Notes, x.FromStatus, x.ToStatus, x.OccurredAtUtc)).ToList());

    public static string Label(WorkOrderStatus status) => status == WorkOrderStatus.InProgress ? "In progress" : status.ToString();
}
