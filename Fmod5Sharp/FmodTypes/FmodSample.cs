using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Fmod5Sharp.CodecRebuilders;
using Fmod5Sharp.Util;

namespace Fmod5Sharp.FmodTypes
{
    public class FmodSample : IBinaryWritable
    {
        public FmodSampleMetadata Metadata;
        public Memory<byte> SampleBytes;
        public string? Name;
        internal FmodSoundBank? MyBank;

        public FmodSample(FmodSampleMetadata metadata, Memory<byte> sampleBytes)
        {
            Metadata = metadata;
            SampleBytes = sampleBytes;
        }

#if NET6_0_OR_GREATER
		public bool RebuildAsStandardFileFormat([NotNullWhen(true)] out byte[]? data, [NotNullWhen(true)] out string? fileExtension)
#else
        public bool RebuildAsStandardFileFormat(out byte[]? data, out string? fileExtension)
#endif
        {
            switch (MyBank!.Header.AudioType)
            {
                case FmodAudioType.VORBIS:
                    data = FmodVorbisRebuilder.RebuildOggFile(this);
                    fileExtension = "ogg";
                    return data.Length > 0;
                case FmodAudioType.PCM8:
                case FmodAudioType.PCM16:
                case FmodAudioType.PCM32:
                    data = FmodPcmRebuilder.Rebuild(this, MyBank.Header.AudioType);
                    fileExtension = "wav";
                    return data.Length > 0;
                case FmodAudioType.GCADPCM:
                    data = FmodGcadPcmRebuilder.Rebuild(this);
                    fileExtension = "wav";
                    return data.Length > 0;
                case FmodAudioType.IMAADPCM:
                    data = FmodImaAdPcmRebuilder.Rebuild(this);
                    fileExtension = "wav";
                    return data.Length > 0;
                default:
                    data = null;
                    fileExtension = null;
                    return false;
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(SampleBytes.Span);
        }
    }
}