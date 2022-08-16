using Marten;
using Marten.Events.Projections;
using Oakton;
using TeleHealth.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(opts =>
{
    // This would typically come from configuration
    opts.Connection("Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres");

    opts.Projections.Add<AppointmentProjection>(ProjectionLifecycle.Inline);
    opts.Projections.SelfAggregate<ProviderShift>(ProjectionLifecycle.Inline);
});

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