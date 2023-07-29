using Azure.Core;
using Newtonsoft.Json.Linq;
using Orleans.Runtime;
using Skyview.Lib;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

public class ArmModelGrain : Grain, IArmModelGrain
{
	private IPersistentState<ArmModel> _model;
	private ArmClient _arm;
	private ILogger<ArmModelGrain> _log;
	private IGrainFactory _grainFactory;

	public ArmModelGrain(
		[PersistentState(stateName: "model", storageName: "models")]  IPersistentState<ArmModel> model, 
		ArmClient arm, IGrainFactory grainFactory, ILogger<ArmModelGrain> log)
	{
		_model = model;
		_arm = arm;
		_grainFactory = grainFactory;
		_log = log;
		this.RegisterTimer(Compare, _model, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
		_log.LogInformation("Activate grain {GrainId}", this.GetGrainId().ToString());
	}
	
	private async Task<GenericResource> GetOwnResource()
	{
		_log.LogInformation("Fetching remote state...");
		
		var grainId = this.GetPrimaryKeyString();
		var resourceId = ResourceIdentifier.Parse(grainId);
		var resource = _arm.GetGenericResource(resourceId);
		resource = (await resource.GetAsync()).Value;
		
		return resource;
	}

	public async Task Compare(object? state)
	{
		_log.LogInformation("Comparing {GrainId} to remote state", this.GetPrimaryKeyString());
		var remote = await GetOwnResource();

		var remoteTags = remote.Data.Tags;
		var modelTags = _model.State.Tags;

		var valueChecks = remoteTags.Keys.Intersect(modelTags.Keys);
		var addedKeys = remoteTags.Keys.Except(modelTags.Keys);
		var removedKeys = modelTags.Keys.Except(remoteTags.Keys);
		
		var valueChanges = valueChecks.Where(key => remoteTags[key] != modelTags[key]);
		foreach (var key in valueChanges)
		{
			_log.LogInformation("Value for tag {Tag} changed from {OldValue} to {NewValue}", key, modelTags[key], remoteTags[key]);
			modelTags[key] = remoteTags[key];
		}

		foreach (var key in addedKeys)
		{
			_log.LogInformation("Tag {Tag} added with value {Value}", key, remoteTags[key]);
			modelTags[key] = remoteTags[key];		
		}
		
		foreach (var key in removedKeys)
		{
			_log.LogInformation("Tag {Tag} removed", key);
			modelTags.Remove(key);
		}
	}

	public ValueTask SyncToRemote()
	{
		_log.LogWarning("Syncing remote state off {ResourceId} with model", this.GetPrimaryKeyString());
		return ValueTask.CompletedTask;
	}
    
	public async ValueTask SyncFromRemote()
	{
		_log.LogWarning("Syncing model of {ResourceId} with remote state", this.GetPrimaryKeyString());
		var resource = await GetOwnResource();
		
		if (resource is null)
		{
			_log.LogWarning("Resource with id {ResourceId} not found", this.GetPrimaryKeyString());
			return;
		}

		var model = new ArmModel
		{
			Id = resource.Id.ToString(),
			Name = resource.Data.Name,
			Type = resource.Data.Kind,
			Location = resource.Data.Location,
			Kind = resource.Data.Kind,
			Tags = resource.Data.Tags
		};

		_model.State = model;
	}

	public ValueTask Update(JObject newValue)
	{
		throw new NotImplementedException();
	}
}
