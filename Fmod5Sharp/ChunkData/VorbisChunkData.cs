using System.IO;

namespace Fmod5Sharp.ChunkData
{
	internal class VorbisChunkData : IChunkData
	{
		public uint Crc32;
		public byte[] Unknown = new byte[0];

		public void Read(BinaryReader reader, uint expectedSize)
		{
			Crc32 = reader.ReadUInt32();
			Unknown = reader.ReadBytes((int)(expectedSize - 4));
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Crc32);
			writer.Write(Unknown);
		}
	}
}