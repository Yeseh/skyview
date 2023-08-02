using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Skyview.Lib;
using Azure.ResourceManager;
using Microsoft.AspNetCore.SignalR;

// TODO: Move to config
const string sbns = "sb-resource-monitor.servicebus.windows.net";
const int notifWorkerCount = 1;
    
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Host.UseOrleans(sb =>
{
    sb.UseLocalhostClustering();
    sb.AddMemoryGrainStorage("models");
    sb.UseDashboard();
});

var cred = new DefaultAzureCredential();

builder.Services.AddSingleton(s => new ServiceBusClient(sbns, cred));
builder.Services.AddSingleton(s => new ServiceBusAdministrationClient(sbns, cred));
builder.Services.AddSingleton(s => new ArmClient(cred));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();

public partial class Program {}