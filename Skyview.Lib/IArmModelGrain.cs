using Newtonsoft.Json.Linq;

namespace Skyview.Lib;

public interface IArmModelGrain : IGrainWithStringKey
{
	ValueTask SyncToRemote();
	ValueTask SyncFromRemote();
	// ValueTask Update(JObject newValue);
}
