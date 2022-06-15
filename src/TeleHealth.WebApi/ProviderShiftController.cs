using Jasper;
using Jasper.Attributes;
using Jasper.Persistence.Marten;
using Marten;
using Marten.AspNetCore;
using Marten.Exceptions;
using Marten.Schema;
using Microsoft.AspNetCore.Mvc;
using TeleHealth.Common;

namespace TeleHealth.WebApi;

public record CompleteCharting(
    Guid ProviderShiftId, 
    Guid AppointmentId, 
    int Version);

public class ProviderShiftController : ControllerBase
{
    [HttpGet("/shift/{shiftId}")]
    public Task GetProviderShift(Guid shiftId, [FromServices] IQuerySession session)
    {
        return session.Json.WriteById<ProviderShift>(shiftId, HttpContext);
    }

    public Task CompleteCharting(
        [FromBody] CompleteCharting charting, 
        [FromServices] ICommandBus bus)
    {
        // Just delegating to Jasper here
        return bus.InvokeAsync(charting, HttpContext.RequestAborted);
    }
}

// This is auto-discovered by Jasper
public class CompleteChartingHandler
{
    [MartenCommandWorkflow] // this opts into some Jasper middlware 
    public ChartingFinished Handle(CompleteCharting charting, ProviderShift shift)
    {
        if (shift.Status != ProviderStatus.Charting)
        {
            throw new Exception("The shift is not currently charting");
        }

        return new ChartingFinished(charting.AppointmentId, shift.BoardId);
    }
}