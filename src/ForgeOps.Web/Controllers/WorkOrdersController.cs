using ForgeOps.Web.Contracts;
using ForgeOps.Web.Models;
using ForgeOps.Web.Services;
using ForgeOps.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ForgeOps.Web.Controllers;

[AutoValidateAntiforgeryToken]
public class WorkOrdersController(IWorkOrderService service, OperationsClock clock) : Controller
{
    public async Task<IActionResult> Index([FromQuery] WorkOrderQuery query, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid work-order filters.");
        return View(new WorkOrderListViewModel(await service.ListAsync(query, ct), query));
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await service.GetAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }

    public async Task<IActionResult> Create(CancellationToken ct) => View("Form", new WorkOrderFormViewModel {
        Input = new() { Priority = WorkOrderPriority.Medium, DueDate = clock.Today.AddDays(2) },
        Technicians = await service.GetTechniciansAsync(ct)
    });

    [HttpPost]
    public async Task<IActionResult> Create(WorkOrderFormViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var order = await service.CreateAsync(model.Input, ct);
                TempData["Success"] = $"{order.ReferenceNumber} created.";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (WorkOrderValidationException ex) { AddErrors(ex); }
        }
        model.Id = null;
        model.Technicians = await service.GetTechniciansAsync(ct);
        return View("Form", model);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var order = await service.GetAsync(id, ct);
        if (order is null) return NotFound();
        if (order.IsArchived) { TempData["Notice"] = "Restore this order before editing."; return RedirectToAction(nameof(Details), new { id }); }
        return View("Form", new WorkOrderFormViewModel { Id = id, Status = order.Status,
            Input = new() { Title = order.Title, EquipmentName = order.EquipmentName, Facility = order.Facility,
                Description = order.Description, Priority = order.Priority, AssignedTechnicianId = order.AssignedTechnicianId,
                DueDate = order.DueDate, ResolutionNotes = order.ResolutionNotes },
            Technicians = await service.GetTechniciansAsync(ct) });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, WorkOrderFormViewModel model, CancellationToken ct)
    {
        var current = await service.GetAsync(id, ct);
        if (current is null) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                await service.UpdateAsync(id, model.Input, ct);
                TempData["Success"] = "Work order updated.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (WorkOrderValidationException ex) { AddErrors(ex); }
        }
        model.Id = id;
        model.Status = current.Status;
        model.Technicians = await service.GetTechniciansAsync(ct);
        return View("Form", model);
    }

    [HttpPost]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        await service.SetArchivedAsync(id, true, ct);
        TempData["Success"] = "Order archived. Its history has been preserved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
    {
        await service.SetArchivedAsync(id, false, ct);
        TempData["Success"] = "Order restored to the operational queue.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private void AddErrors(WorkOrderValidationException ex)
    {
        foreach (var (field, messages) in ex.Errors)
            foreach (var message in messages) ModelState.AddModelError($"Input.{field}", message);
    }
}
