namespace ForgeOps.Web.Services;

public class WorkOrderValidationException(Dictionary<string, string[]> errors) : Exception("The work order could not be saved.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}

public class WorkOrderNotFoundException(int id) : Exception($"Work order {id} was not found.");
