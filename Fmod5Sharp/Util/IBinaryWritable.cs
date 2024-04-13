using System.IO;

namespace Fmod5Sharp.Util;

internal interface IBinaryWritable
{
    internal void Write(BinaryWriter writer);
}