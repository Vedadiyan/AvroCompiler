using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Storage;

public enum HighOrderType
{
    NONE = 0,
    RECORD = 1,
    ENUM = 2,
    FIXED = 3,
    MESSAGE = 4
}
public class Types
{
    public static Lazy<Types> Current { get; } = new Lazy<Types>(() => new Types(), true);
    private readonly Dictionary<string, HighOrderType> types;
    private readonly object locker;
    private Types()
    {
        types = new Dictionary<string, HighOrderType>();
        locker = new object();
    }
    public bool TryGetType(string name, out HighOrderType highOrderType)
    {
        lock (locker)
        {
            return types.TryGetValue(name, out highOrderType);
        }
    }
    public void RegisterType(string name, HighOrderType type)
    {
        lock (locker)
        {
            types.Add(name, type);
        }
    }
}