using ForgeOps.Web.Models;
using ForgeOps.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace ForgeOps.Web.Data;

public static class DemoSeeder
{
    public static async Task SeedAsync(ForgeOpsDbContext db, OperationsClock clock)
    {
        if (await db.WorkOrders.AnyAsync()) return;
        var technicians = await db.Technicians.OrderBy(x => x.Id).ToListAsync();
        if (technicians.Count == 0)
        {
            technicians = [
                new() { Name = "Avery Chen", Specialty = "Mechanical systems" },
                new() { Name = "Jordan Ellis", Specialty = "Electrical & controls" },
                new() { Name = "Morgan Patel", Specialty = "Hydraulics & pneumatics" },
                new() { Name = "Riley Brooks", Specialty = "Preventive maintenance" }
            ];
            db.Technicians.AddRange(technicians);
        }
        var samples = new (string Title, string Equipment, string Facility, string Description, WorkOrderPriority Priority, WorkOrderStatus Status, int Due)[]
        {
            ("Investigate vibration on press drive", "Hydraulic press HP-04", "North plant · Forming", "Operator reported increased vibration during the return stroke. Inspect the drive coupling and bearing alignment before the next production run.", WorkOrderPriority.Critical, WorkOrderStatus.Open, -2),
            ("Replace worn conveyor guide rollers", "Assembly conveyor CV-12", "North plant · Assembly", "Guide rollers show uneven wear at station 3. Replace affected rollers and check belt tracking.", WorkOrderPriority.High, WorkOrderStatus.InProgress, -1),
            ("Restore coolant flow to CNC spindle", "CNC machining centre MC-08", "South plant · Machining", "Coolant pressure falls below the normal operating range after warm-up. Inspect filter, pump and supply lines.", WorkOrderPriority.High, WorkOrderStatus.Open, 0),
            ("Calibrate furnace temperature sensor", "Heat-treatment furnace HT-02", "South plant · Heat treatment", "Scheduled verification found a sensor reading offset. Calibrate against a reference probe and document the readings.", WorkOrderPriority.Medium, WorkOrderStatus.InProgress, 1),
            ("Repair air leak at packaging manifold", "Pneumatic manifold PM-06", "North plant · Packaging", "Audible air leak near valve bank B. Isolate the supply, replace the seal and perform a leak check.", WorkOrderPriority.Medium, WorkOrderStatus.InProgress, -3),
            ("Inspect overhead crane limit switch", "Bridge crane BC-03", "North plant · Material handling", "The upper travel limit switch operates intermittently. Keep the equipment isolated until inspection is complete.", WorkOrderPriority.Critical, WorkOrderStatus.InProgress, 0),
            ("Replace task lighting at inspection bench", "Inspection bench IB-05", "South plant · Quality", "Two task lights flicker during visual inspection. Replace fixtures and verify illumination.", WorkOrderPriority.Low, WorkOrderStatus.Open, 5),
            ("Service hydraulic power-unit filter", "Hydraulic power unit PU-11", "North plant · Forming", "Differential-pressure indicator is approaching its service threshold. Change the filter and check fluid condition.", WorkOrderPriority.Medium, WorkOrderStatus.Open, 3),
            ("Align laser sensor on transfer line", "Transfer line TL-09", "North plant · Assembly", "Parts are intermittently missed by the position sensor. Realign the bracket and confirm repeatability.", WorkOrderPriority.High, WorkOrderStatus.InProgress, -1),
            ("Replace motor contactor on extraction fan", "Extraction fan EF-07", "South plant · Fabrication", "Contactor showed heat discoloration during planned inspection. Replace and verify current draw.", WorkOrderPriority.High, WorkOrderStatus.Resolved, -4),
            ("Complete lubrication route for roller table", "Roller table RT-02", "North plant · Material handling", "Complete the weekly bearing lubrication route and inspect grease lines for blockage.", WorkOrderPriority.Low, WorkOrderStatus.Resolved, -2),
            ("Repair interlock on wash station", "Parts washer PW-03", "South plant · Finishing", "Lid interlock did not consistently register closed. Replace the switch and verify the safety circuit.", WorkOrderPriority.High, WorkOrderStatus.Resolved, -1)
        };
        for (var i = 0; i < samples.Length; i++)
        {
            var sample = samples[i];
            var created = clock.UtcNow.AddDays(-10).AddHours(i);
            var updated = clock.UtcNow.AddHours(-i * 3);
            var order = new WorkOrder {
                Title = sample.Title, EquipmentName = sample.Equipment, Facility = sample.Facility,
                Description = sample.Description, Priority = sample.Priority, Status = sample.Status,
                DueDate = clock.Today.AddDays(sample.Due), CreatedAtUtc = created, UpdatedAtUtc = updated,
                AssignedTechnician = i % 4 == 0 ? null : technicians[i % technicians.Count],
                ResolutionNotes = sample.Status == WorkOrderStatus.Resolved ? "Fictional demo: component serviced, operating checks completed and equipment returned to service." : null
            };
            order.History.Add(new() { EventType = "Created", Summary = "Fictional demo order created.", OccurredAtUtc = created });
            if (sample.Status != WorkOrderStatus.Open)
                order.History.Add(new() { EventType = "StatusChanged", Summary = $"Fictional demo: status changed to {WorkOrderService.Label(sample.Status)}.",
                    FromStatus = WorkOrderStatus.Open, ToStatus = sample.Status, Notes = order.ResolutionNotes, OccurredAtUtc = updated });
            db.WorkOrders.Add(order);
        }
        await db.SaveChangesAsync();
    }
}
