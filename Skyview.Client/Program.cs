using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Skyview.Client;
using Spectre.Console.Cli;

var svClient = new HttpClient();
svClient.BaseAddress = new Uri("http://localhost:5000");

var services = new ServiceCollection();
services.AddSingleton(svClient);
services.AddLogging();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);
app.Configure(c =>
{
	c.PropagateExceptions();
	c.AddCommand<TrackCommand>("track");
	c.AddCommand<UntrackCommand>("untrack");
	c.AddCommand<WatchCommand>("watch");
});

app.Run(args);