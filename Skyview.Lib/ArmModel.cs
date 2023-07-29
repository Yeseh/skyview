using Newtonsoft.Json.Linq;

namespace Skyview.Lib;

[GenerateSerializer]
public class ArmModel
{
	[Id(0)]
	public string Id { get; set; } = string.Empty;

	[Id(1)]
	public string Name { get; set; } = string.Empty;

	[Id(2)]
	public string Type { get; set; } = string.Empty;
	[Id(3)]
	public string Location { get; set; } = string.Empty;

	[Id(4)]
	public string? Kind { get; set; } = string.Empty;

	[Id(5)]
	public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

	// [Id(6)]
	// public JObject? Sku { get; set; }
	//
	// [Id(7)]
	// public JObject Properties { get; set; } = new();
}
