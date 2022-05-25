namespace TeleHealth.Common;

public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public record AppointmentRequested(Guid PatientId);
public record AppointmentRouted(Guid BoardId, Guid PatientId, DateTimeOffset EstimatedTime) : IBoardEvent;
public record AppointmentScheduled(Guid BoardId, Guid ProviderId, DateTimeOffset EstimatedTime) : IBoardEvent;
public record AppointmentStarted(Guid BoardId) : IBoardEvent;
public record AppointmentFinished(Guid BoardId) : IBoardEvent;

public enum AppointmentStatus
{
    Requested,
    Scheduled,
    Started,
    Completed
}

public class Appointment
{
    public Guid Id { get; set; }
    public string FirstName { get; }
    public string LastName { get; }

    public AppointmentStatus Status { get; set; }
    public string ProviderName { get; set; }
    public DateTimeOffset? EstimatedTime { get; set; }
    public Guid BoardId { get; set; }

    public Appointment(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}