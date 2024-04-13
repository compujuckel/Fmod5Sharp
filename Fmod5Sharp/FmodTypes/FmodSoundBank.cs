using System.Collections.Generic;
using System.IO;
using System.Text;
using Fmod5Sharp.Util;

namespace Fmod5Sharp.FmodTypes
{
    public class FmodSoundBank
    {
        public FmodAudioHeader Header;
        public List<FmodSample> Samples;

        internal FmodSoundBank(FmodAudioHeader header, List<FmodSample> samples)
        {
            Header = header;
            Samples = samples;
            Samples.ForEach(s => s.MyBank = this);
        }

        public void ToFile(string path)
        {
            using var file = File.Create(path);
            ToStream(file);
        }

        public void ToStream(Stream stream)
        {
            using var writer = new BinaryWriter(stream);
            Header.Write(writer);

            if (Header.SizeOfNameTable > 0)
            {
                var nameOffset = Samples.Count * 4;
                foreach (var sample in Samples)
                {
                    writer.Write(nameOffset);
                    nameOffset += Encoding.UTF8.GetByteCount(sample.Name!) + 1;
                }

                foreach (var sample in Samples)
                {
                    writer.Write(Encoding.UTF8.GetBytes(sample.Name!));
                    writer.Write((byte)0);
                }
            }

            writer.Align(32);
            
            foreach (var sample in Samples)
            {
                sample.Write(writer);
            }
        }
    }
}