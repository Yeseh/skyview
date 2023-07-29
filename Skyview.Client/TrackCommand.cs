using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Skyview.Lib;
using Spectre.Console.Cli;

namespace Skyview.Client;

public class TrackCommand : AsyncCommand<TrackCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<id>")]
		public string ResourceId { get; set; }
	}

	private readonly ILogger<TrackCommand> _log;
	private readonly HttpClient _svClient;
	
	public TrackCommand(ILogger<TrackCommand> log, HttpClient svClient)
	{
		_log = log;
		_svClient = svClient;
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var req = new TrackRequest();
		req.ResourceId = settings.ResourceId;
			
		var response = await _svClient.PostAsJsonAsync("/track", req);
		_log.LogInformation("Response: {ResponseMessage}", response.ToString());
		
		return 0;
	}
}