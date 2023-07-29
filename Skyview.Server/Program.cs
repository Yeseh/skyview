using Azure.Identity;
using Skyview.Lib;
using Azure.ResourceManager;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Host.UseOrleans(sb =>
{
    sb.UseLocalhostClustering();
    sb.AddMemoryGrainStorage("models");
    sb.UseDashboard();
});

builder.Services.AddSingleton(s => new ArmClient(new DefaultAzureCredential()));
builder.Services.AddSignalR().AddAzureSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapPost("/track", async (IGrainFactory grains, TrackRequest request) =>
{
    var tracker = grains.GetGrain<ITrackerGrain>(TrackerGrain.Id);
    await tracker.Track(request.ResourceId);
});

app.MapPost("/untrack", async (IGrainFactory grains, TrackRequest request) =>
{
    var tracker = grains.GetGrain<ITrackerGrain>(TrackerGrain.Id);
    await tracker.Untrack(request.ResourceId);
});

app.MapHub<SkyViewHub>("watch");

app.Run();

public class SkyViewHub : Hub
{
    private readonly IGrainFactory _grains;
    private readonly  ILogger<SkyViewHub> _log;
        
    public SkyViewHub(IGrainFactory grains, ILogger<SkyViewHub> log)
    {
        _grains = grains;
        _log = log;
    }

    public Task ResourceChanged()
    {
        return Clients.All.SendAsync("ResourceChanged", "Detected a resource change");
    }
}