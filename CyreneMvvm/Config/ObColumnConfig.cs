namespace CyreneMvvm.Config;

public interface IDatabaseParser
{
    public string Serialize<T>(T value);
    public T? Deserialize<T>(string value);
}

public static class ObColumnConfig
{
    public static IDatabaseParser? Parser { get; set; }
}
