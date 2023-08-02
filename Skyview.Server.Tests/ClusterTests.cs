using Microsoft.AspNetCore.Mvc.Testing;

namespace Skyview.Server.Tests;

[Collection(ClusterCollection.Name)]
public class ClusterTests 
{
    private readonly ClusterFixture _cluster;
    
    public ClusterTests(ClusterFixture cluster)
    {
        _cluster = cluster;
    }
    
    [Fact]
    public void TrackResource()
    {
            
    }
}