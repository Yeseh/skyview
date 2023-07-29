using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Skyview.Lib;

public class TrackerGrain : Grain, ITrackerGrain
{
	public const int Id = 0;
    
	private readonly HashSet<ResourceIdentifier> _tracked = new();
	private readonly ILogger<TrackerGrain> _log;
	private readonly ArmClient _arm;
	private readonly IGrainFactory _grainFactory;
	
	private readonly List<IArmModelGrain> _createdModels = new();

	public TrackerGrain(ILogger<TrackerGrain> log, ArmClient arm, IGrainFactory grainFactory)
	{
		_log = log;
		_arm = arm;
		_grainFactory = grainFactory;
	}

	public async ValueTask Track(string resourceId)
	{
		var validId = ResourceIdentifier.TryParse(resourceId, out var id);
		if (!validId || id is null)
		{
			_log.LogError("Invalid resourceId: {ResourceId}", resourceId);
			return;
		}

		_log.LogInformation("Found resource type {ResourceType}", id.ResourceType.Type);

		var subscriptionId = SubscriptionResource.CreateResourceIdentifier(id.SubscriptionId);
		switch (id.ResourceType.Type)
		{
			case "subscriptions":
			{
				var sub = _arm.GetSubscriptionResource(subscriptionId);
				await TrackSubscription(sub);
				break;
			}
			case "resourceGroups":
			{
				var sub = _arm.GetSubscriptionResource(subscriptionId);
				var rg = await sub.GetResourceGroupAsync(id.ResourceGroupName);
				await TrackResourceGroup(rg);
				break;
			}
			default:
				TrackResource(id);
				break;
		}
		
		_log.LogInformation("Done tracking, importing state...");
		foreach (var m in _createdModels)
		{
			await m.SyncFromRemote();
		}
	}

	private async Task TrackSubscription(SubscriptionResource sub)
	{
		_log.LogInformation("Tracking subscription {SubscriptionId}", sub.Id);
		
		var grain = _grainFactory.GetGrain<IArmModelGrain>(sub.Id.ToString());
		_createdModels.Add(grain);
		_tracked.Add(sub.Id);
		
		var rgs = sub.GetResourceGroups();
		foreach (var g in rgs)
		{
			await TrackResourceGroup(g);
		}
	}

	private async Task TrackResourceGroup(ResourceGroupResource g)
	{
		_log.LogInformation("Tracking resource group: {ResourceGroupId}", g.Id);
		
		var grain = _grainFactory.GetGrain<IArmModelGrain>(g.Id.ToString());
		_tracked.Add(g.Id); 
		_createdModels.Add(grain);
		
		var groups = g.GetGenericResourcesAsync();
		await foreach (var r in groups)
		{
			TrackResource(r.Id);
		}
	}

	private void TrackResource(ResourceIdentifier id)
	{
		_log.LogInformation("Tracking resource: {ResourceId}", id);
		
		var grain = _grainFactory.GetGrain<IArmModelGrain>(id.ToString());
		_log.LogInformation("Created grain: {GrainId}", grain.GetPrimaryKeyString());
		_tracked.Add(id);
		_createdModels.Add(grain);
	}
    
	public ValueTask Untrack(string resourceId)
	{
		var validId = ResourceIdentifier.TryParse(resourceId, out var id);
		if (!validId || id is null)
		{
			_log.LogError("Invalid resourceId: {ResourceId}", resourceId);
			return ValueTask.CompletedTask;
		}
		
		_tracked.Remove(id);
        
		return ValueTask.CompletedTask;
	}
    
	public ValueTask<bool> IsTracked(string resourceId)
	{
		var validId = ResourceIdentifier.TryParse(resourceId, out var id);
		if (!validId || id is null)
		{
			_log.LogError("Invalid resourceId: {ResourceId}", resourceId);
			return ValueTask.FromResult(false);
		}
		
		return ValueTask.FromResult(_tracked.Contains(id));
	}
}
