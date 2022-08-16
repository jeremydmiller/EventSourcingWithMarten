using Jasper.Persistence.Marten;
using TeleHealth.Common;

namespace TeleHealth.ApiWithJasper;

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

        return new ChartingFinished();
    }
}