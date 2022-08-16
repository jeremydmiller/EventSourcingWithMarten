using System;
using System.Threading.Tasks;
using Marten;
using Shouldly;
using TeleHealth.Common;
using Xunit;
using Xunit.Abstractions;

namespace TeleHealth.Tests;

public class GettingStarted : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private DocumentStore theStore;
    private Provider theProvider;

    private Guid theBoardId = Guid.NewGuid();
    private Guid theShiftId = Guid.NewGuid();

    public GettingStarted(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        // Initialize Marten the simplest, possible way
        theStore = DocumentStore.For(opts =>
        {
            opts.Connection("Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres");
            opts.Logger(new TestOutputMartenLogger(_output));
        });

        // Rewinding any existing data first
        await theStore.Advanced.ResetAllData();
        
        await using var session = theStore.LightweightSession();

        // Just a little reference data
        theProvider = new Provider
        {
            FirstName = "Larry",
            LastName = "Bird"
        };
        
        session.Store(theProvider);
        await session.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task start_a_new_shift()
    {
        await using var session = theStore.LightweightSession();

        session.Events.StartStream<ProviderShift>
        (
            theShiftId,
            new ProviderJoined(theProvider.Id, theBoardId),
            new ProviderReady()
        );

        await session.SaveChangesAsync();
    }

    [Fact]
    public async Task live_project_the_provider_shift()
    {
        await using var session = theStore.LightweightSession();
        
        // Sneak peek at projections
        var shift = await session.Events
            .AggregateStreamAsync<ProviderShift>(theShiftId);

        shift.Name.ShouldBe("Larry Bird");
    }

}