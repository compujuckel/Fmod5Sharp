using System;
using System.Collections.Generic;
using System.IO;
using Fmod5Sharp.Util;

namespace Fmod5Sharp.FmodTypes
{
	public class FmodSampleMetadata : IBinaryReadable, IBinaryWritable
	{
		internal bool HasAnyChunks;
		internal uint FrequencyId;
		internal ulong DataOffset;
		internal List<FmodSampleChunk> Chunks = new();
		internal int NumChannels;

		public bool IsStereo;
		public ulong SampleCount;

		public int Frequency => FsbLoader.Frequencies.TryGetValue(FrequencyId, out var actualFrequency) ? actualFrequency : (int)FrequencyId; //If set by FREQUENCY chunk, id is actual frequency
		public uint Channels => (uint)NumChannels;

		private ulong _encoded;

		void IBinaryReadable.Read(BinaryReader reader)
		{
			_encoded = reader.ReadUInt64();
			
			HasAnyChunks = (_encoded & 1) == 1; //Bit 0
			FrequencyId = (uint) _encoded.Bits( 1, 4); //Bits 1-4
			var pow2 = (int) _encoded.Bits(5, 2); //Bits 5-6
			NumChannels = 1 << pow2;
			if (NumChannels > 2)
				throw new("> 2 channels not supported");
			
			IsStereo = NumChannels == 2;
			
			DataOffset = _encoded.Bits(7, 27) * 32;
			SampleCount = _encoded.Bits(34, 30);
		}

		public void Write(BinaryWriter writer)
		{
			ulong encoded = 0;
			encoded.SetBits(0, 1, HasAnyChunks ? 1U : 0U);
			encoded.SetBits(1, 4, FrequencyId);
			encoded.SetBits(5, 2, (ulong)Math.Log2(NumChannels));
			encoded.SetBits(7, 27, DataOffset / 32);
			encoded.SetBits(34, 30, SampleCount);
			writer.Write(encoded);
		}
	}
}