using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var headerStart = stream.Position;
            using var writer = new BinaryWriter(stream, Encoding.ASCII, true);
            Header.Write(writer);

            var nameTableStart = stream.Position;

            if (Samples.All(s => s.Name != null))
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

            var dataStart = stream.Position;
            Header.SizeOfNameTable = (uint)(dataStart - nameTableStart);

            foreach (var sample in Samples)
            {
                sample.Metadata.DataOffset = (uint)(stream.Position - dataStart);
                sample.Write(writer);

                writer.Align(32);
            }

            var dataEnd = stream.Position;
            Header.SizeOfData = (uint)(dataEnd - dataStart);

            stream.Position = headerStart;
            Header.Write(writer);

            stream.Position = dataEnd;
        }
    }
}