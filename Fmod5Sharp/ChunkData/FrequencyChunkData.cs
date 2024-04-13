using System.IO;

namespace Fmod5Sharp.ChunkData
{
	internal class FrequencyChunkData : IChunkData
	{
		public uint ActualFrequencyId;

		public void Read(BinaryReader reader, uint expectedSize)
		{
			ActualFrequencyId = reader.ReadUInt32();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(ActualFrequencyId);
		}
	}
}