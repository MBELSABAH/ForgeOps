namespace ForgeOps.Web.Services;

public class OperationsClock(TimeProvider timeProvider)
{
    public static readonly TimeZoneInfo FacilityTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Halifax");
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), FacilityTimeZone).DateTime);
    public static string LocalTime(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), FacilityTimeZone).ToString("MMM d, h:mm tt");
}
