using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Extensions;

public static class AvroTypesExtensions
{
    public static bool IsCompsite(this AvroTypes avroTypes)
    {
        double _n = (double)avroTypes;
        int maxTraverse = 32;
        int currentTraverse = 1;
        while (currentTraverse++ < maxTraverse)
        {
            _n = _n / 2;
            if (_n % 1 != 0)
            {
                return true;
            }
            else if (_n == 1)
            {
                return false;
            }
        }
        throw new Exception("Maximum traverse limit reached");
    }
    public static AvroTypes ExcludeFlag(this AvroTypes avroTypes, AvroTypes flag) {
        return avroTypes ^ flag;
    }
    public static AvroTypes AddFlag(this AvroTypes avroTypes, AvroTypes flag) {
        return avroTypes | flag;
    }
    public static bool IsNullable(this AvroTypes avroTypes) {
        return (avroTypes & AvroTypes.NULL) == AvroTypes.NULL;
    }
}