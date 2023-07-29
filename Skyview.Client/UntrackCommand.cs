using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Skyview.Lib;
using Spectre.Console.Cli;

namespace Skyview.Client;

public class UntrackCommand : AsyncCommand<UntrackCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<id>")]
		public string ResourceId { get; set; }
	}
	
	private readonly ILogger<UntrackCommand> _log;
	private readonly HttpClient _svClient;
	
	public UntrackCommand(ILogger<UntrackCommand> log, HttpClient svClient)
	{
		_log = log;
		_svClient = svClient;
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var req = new TrackRequest();
		req.ResourceId = settings.ResourceId;
		
		var response = await _svClient.PostAsJsonAsync("/untrack", req);
		_log.LogInformation("Response: {ResponseMessage}", response.ToString());
		
		return 0;
	}
}