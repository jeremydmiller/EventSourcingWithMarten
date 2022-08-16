using Baseline.Dates;
using Jasper;
using Jasper.ErrorHandling;
using Jasper.Persistence.Marten;
using Marten;
using Marten.Events.Projections;
using Marten.Exceptions;
using Npgsql;
using Oakton;
using TeleHealth.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

// Add services to the container.
builder.Host.UseJasper(opts =>
{
    // I'm choosing to process any ChartingFinished event messages
    // in a separate, local queue with persistent messages for the inbox/outbox
    opts.PublishMessage<ChartingFinished>()
        .ToLocalQueue("charting")
        .UsePersistentInbox();
    
    // If we encounter a concurrency exception, just try it immediately 
    // up to 3 times total
    opts.Handlers.OnException<ConcurrencyException>()
        .RetryOnce()
        .Then.RetryWithCooldown(50.Milliseconds(), 250.Milliseconds())
        .Then.MoveToErrorQueue(); 
    
    // It's an imperfect world, and sometimes transient connectivity errors
    // to the database happen
    opts.Handlers.OnException<NpgsqlException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(opts =>
{
    // This would typically come from configuration
    opts.Connection(ConnectionSource.ConnectionString);

    opts.Projections.Add<AppointmentProjection>(ProjectionLifecycle.Inline);
    opts.Projections.SelfAggregate<ProviderShift>(ProjectionLifecycle.Inline);
})
    // I added this to enroll Marten in the Jasper outbox
    .IntegrateWithJasper()
    
    // I also added this to opt into events being forward to
    // the Jasper outbox during SaveChangesAsync()
    .EventForwardingToJasper();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers(); // I'm sticking with controllers for now

// This is using Oakton for command running
await app.RunOaktonCommands(args);