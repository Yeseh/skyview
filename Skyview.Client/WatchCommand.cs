using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Skyview.Client;

public class WatchCommand : AsyncCommand
{
	private readonly ILogger<WatchCommand> _log;
	private readonly HttpClient _svClient;
	
	public WatchCommand(ILogger<WatchCommand> log, HttpClient svClient)
	{
		_log = log;
		_svClient = svClient;
	}
	
	public override Task<int> ExecuteAsync(CommandContext context)
	{
		var connection = new HubConnectionBuilder()
		                 .WithUrl(_svClient.BaseAddress + "/watch")
		                 .Build();
		
		_log.LogInformation("Watching...");
		connection.On("ResourceChanged", () => _log.LogInformation("CLI received ResourceChanged"));
		connection.StartAsync();
		
		return Task.FromResult(0);
	}
}