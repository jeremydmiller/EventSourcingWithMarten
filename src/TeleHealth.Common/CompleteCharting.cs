namespace TeleHealth.Common;

public record CompleteCharting(
    Guid ProviderShiftId, 
    Guid AppointmentId, 
    int Version);