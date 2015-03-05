using System;
using System.IO;

namespace Hurricane.AppCommunication
{
    public class StreamProvider : IDisposable
    {
        public StreamReader StreamReader { get; set; }
        public StreamWriter StreamWriter { get; set; }

        public BinaryReader BinaryReader { get; set; }
        public BinaryWriter BinaryWriter { get; set; }

        public Stream BaseStream { get; set; }

        public StreamProvider(Stream baseStream)
        {
            StreamReader = new StreamReader(baseStream);
            StreamWriter = new StreamWriter(baseStream);

            BinaryReader = new BinaryReader(baseStream);
            BinaryWriter = new BinaryWriter(baseStream);

            BaseStream = baseStream;
        }

        public void SendLine(string line)
        {
            StreamWriter.WriteLine(line);
            StreamWriter.Flush();
        }

        public void Dispose()
        {
            StreamReader.Close();
            StreamWriter.Close();

            BinaryReader.Close();
            BinaryWriter.Close();

            BaseStream.Close();
        }
    }
}
