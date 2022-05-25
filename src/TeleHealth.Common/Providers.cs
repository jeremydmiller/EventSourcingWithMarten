namespace TeleHealth.Common;

public interface IBoardEvent
{
    Guid BoardId { get; }
}

public record ProviderAssigned(Guid AppointmentId, Guid BoardId) : IBoardEvent;
public record ProviderJoined(Guid ProviderId, Guid BoardId) : IBoardEvent;
public record ProviderReady (Guid BoardId) : IBoardEvent;
public record ProviderPaused(Guid BoardId) : IBoardEvent;
public record ProviderSignedOff(Guid BoardId) : IBoardEvent;

public record ChartingFinished(Guid AppointmentId, Guid BoardId) : IBoardEvent;
public record ChartingStarted(Guid AppointmentId, Guid BoardId) : IBoardEvent;


public enum ProviderStatus
{
    Ready,
    Assigned,
    Charting,
    Paused
}

public class Provider
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}