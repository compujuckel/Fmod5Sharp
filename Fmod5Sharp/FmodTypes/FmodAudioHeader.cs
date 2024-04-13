using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fmod5Sharp.ChunkData;
using Fmod5Sharp.Util;

namespace Fmod5Sharp.FmodTypes
{
	public class FmodAudioHeader : IBinaryWritable
	{
		private static readonly object ChunkReadingLock = new();

		internal readonly bool IsValid;
		
		public readonly FmodAudioType AudioType;
		public readonly uint Version;
		public readonly uint NumSamples;

		internal readonly uint SizeOfThisHeader;
		internal readonly uint SizeOfSampleHeaders;
		internal readonly uint SizeOfNameTable;
		internal readonly uint SizeOfData;

		internal uint Unknown1;
		internal uint Unknown2;
		internal uint Flags;
		internal ulong HashLower;
		internal ulong HashUpper;
		internal ulong Unknown3;
		
		internal readonly List<FmodSampleMetadata> Samples = new();

		public FmodAudioHeader(BinaryReader reader)
		{
			string magic = reader.ReadString(4);

			if (magic != "FSB5")
			{
				IsValid = false;
				return;
			}

			Version = reader.ReadUInt32(); //0x04
			NumSamples = reader.ReadUInt32(); //0x08
			SizeOfSampleHeaders = reader.ReadUInt32();
			SizeOfNameTable = reader.ReadUInt32();
			SizeOfData = reader.ReadUInt32(); //0x14
			AudioType = (FmodAudioType) reader.ReadUInt32(); //0x18

			Unknown1 = reader.ReadUInt32(); //Skip 0x1C which is always 0
			
			if (Version == 0)
			{
				SizeOfThisHeader = 0x40;
				Unknown2 = reader.ReadUInt32(); //Version 0 has an extra field at 0x20 before flags
			}
			else
			{
				SizeOfThisHeader = 0x3C;
			}
			
			Flags = reader.ReadUInt32(); //Skip 0x20 (flags)

			//128-bit hash
			HashLower = reader.ReadUInt64(); //0x24
			HashUpper = reader.ReadUInt64(); //0x30

			Unknown3 = reader.ReadUInt64(); //Skip unknown value at 0x34

			var sampleHeadersStart = reader.Position();
			for (var i = 0; i < NumSamples; i++)
			{
				var sampleMetadata = reader.ReadEndian<FmodSampleMetadata>();

				if (!sampleMetadata.HasAnyChunks)
				{
					Samples.Add(sampleMetadata);
					continue;
				}

				lock (ChunkReadingLock)
				{
					List<FmodSampleChunk> chunks = new();
					FmodSampleChunk.CurrentSample = sampleMetadata;
					
					FmodSampleChunk nextChunk;
					do
					{
						nextChunk = reader.ReadEndian<FmodSampleChunk>();
						chunks.Add(nextChunk);
					} while (nextChunk.MoreChunks);

					FmodSampleChunk.CurrentSample = null;
					
					if (chunks.FirstOrDefault(c => c.ChunkType == FmodSampleChunkType.FREQUENCY) is { ChunkData: FrequencyChunkData fcd })
					{
						sampleMetadata.FrequencyId = fcd.ActualFrequencyId;
					}

					sampleMetadata.Chunks = chunks;
				
					Samples.Add(sampleMetadata);
				}
			}
			
			IsValid = true;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write("FSB5"u8);
			writer.Write(Version);
			writer.Write(NumSamples);
			writer.Write(SizeOfSampleHeaders);
			writer.Write(SizeOfNameTable);
			writer.Write(SizeOfData);
			writer.Write((uint)AudioType);
			writer.Write(Unknown1);

			if (Version == 0)
			{
				writer.Write(Unknown2);
			}
			
			writer.Write(Flags);
			writer.Write(HashLower);
			writer.Write(HashUpper);
			writer.Write(Unknown3);

			foreach (var sample in Samples)
			{
				sample.Write(writer);
				
				if (!sample.HasAnyChunks) continue;

				foreach (var chunk in sample.Chunks)
				{
					chunk.Write(writer);
				}
			}
		}
	}
}