using System.Linq;
using Newtonsoft.Json;

public class DataLoaderJob : ThreadedJob
{
    public string text;
    public DataHolder dataHolder;
    
    protected override void ThreadFunction()
    {
        RawDataHolder holder = JsonConvert.DeserializeObject<RawDataHolder>(text);
        dataHolder = new DataHolder();
        dataHolder.edgeData = holder.edges.Select((edge) => new EdgeData(edge)).ToDictionary(i=>i.id);
        dataHolder.verticeData = holder.vertices.Select((vertice) => new VerticeData(vertice)).ToDictionary(i=>i.id);
        
        // Make other computations here!
    }

    protected override void OnFinished()
    {
        
    }
}