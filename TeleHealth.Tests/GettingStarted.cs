using System;
using System.Threading.Tasks;
using Marten;
using Marten.Events.Projections;
using Shouldly;
using TeleHealth.Common;
using Xunit;
using Xunit.Abstractions;

namespace TeleHealth.Tests;

public class GettingStarted
{
    private readonly ITestOutputHelper _output;

    public GettingStarted(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task start_a_new_shift()
    {
        // Initialize Marten the simplest, possible way
        var store = DocumentStore.For(opts =>
        {
            opts.Connection(ConnectionSource.ConnectionString);
        });

        var provider = new Provider
        {
            FirstName = "Larry",
            LastName = "Bird"
        };

        // Just a little reference data
        await using var session = store.LightweightSession();
        session.Store(provider);
        await session.SaveChangesAsync();

        var boardId = Guid.NewGuid();

        // Just to capture the SQL being executed to the test output
        session.Logger = new TestOutputMartenLogger(_output);

        var shiftId = session.Events.StartStream<ProviderShift>
        (
            new ProviderJoined(provider.Id, boardId),
            new ProviderReady(boardId)
        ).Id;

        await session.SaveChangesAsync();

        // More on this later!
        var shift = await session.Events
            .AggregateStreamAsync<ProviderShift>(shiftId);

        shift.Name.ShouldBe("Larry Bird");
    }
}