using Marten;
using Marten.AspNetCore;
using Marten.Exceptions;
using Marten.Schema;
using Microsoft.AspNetCore.Mvc;
using TeleHealth.Common;

namespace TeleHealth.WebApi;

public class ProviderShiftController : ControllerBase
{
    [HttpGet("/shift/{shiftId}")]
    public Task GetProviderShift(Guid shiftId, [FromServices] IQuerySession session)
    {
        return session.Json.WriteById<ProviderShift>(shiftId, HttpContext);
    }
    
    [HttpPost("/shift/charting/complete")]
    public async Task CompleteCharting(
        [FromBody] CompleteCharting charting, 
        [FromServices] IDocumentSession session)
    {
        var stream = await session
            .Events
            .FetchForWriting<ProviderShift>(charting.ProviderShiftId, charting.Version);
        
        if (stream.Aggregate.Status != ProviderStatus.Charting)
        {
            throw new Exception("The shift is not currently charting");
        }
        
        stream.AppendOne(new ChartingFinished());

        await session.SaveChangesAsync();
    }

    // [HttpPost("/shift/charting/complete")]
    // public Task CompleteCharting(
    //     [FromBody] CompleteCharting charting, 
    //     [FromServices] IDocumentSession session)
    // {
    //     return session
    //         .Events
    //         .WriteToAggregate<ProviderShift>(charting.ProviderShiftId, charting.Version, stream =>
    //         {
    //             if (stream.Aggregate.Status != ProviderStatus.Charting)
    //             {
    //                 throw new Exception("The shift is not currently charting");
    //             }
    //
    //             var finished = new ChartingFinished();
    //             stream.AppendOne(finished);
    //         });
    // }
}

