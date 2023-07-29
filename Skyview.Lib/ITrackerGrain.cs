namespace Skyview.Lib;

public interface ITrackerGrain : IGrainWithIntegerKey
{
	ValueTask Track(string resourceId);
	ValueTask Untrack(string resourceId);
	ValueTask<bool> IsTracked(string resourceId);
}
