using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TeleHealth.Common;

namespace TeleHealth.WebApi;

public record CompleteCharting(Guid ShiftId, Guid AppointmentId, int Version);

public class ProviderShiftController : ControllerBase
{
    [HttpGet("/shift/{shiftId}")]
    public Task GetProviderShift(Guid shiftId, [FromServices] IQuerySession session)
    {
        return session.Json.WriteById<ProviderShift>(shiftId, HttpContext);
    }

    public async Task CompleteCharting(
        [FromBody] CompleteCharting charting, 
        [FromServices] IDocumentSession session)
    {
        /* We've got options for concurrency here! */
        var stream = await session
            .Events.FetchForWriting<ProviderShift>(charting.ShiftId);

        if (stream.Aggregate.Status != ProviderStatus.Charting)
        {
            throw new Exception("The shift is not currently charting");
        }
        
        stream.AppendOne(new ChartingFinished(stream.Aggregate.AppointmentId.Value, stream.Aggregate.BoardId));

        await session.SaveChangesAsync();
    }
}