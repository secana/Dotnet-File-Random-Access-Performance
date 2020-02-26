using System;
using System.IO;

namespace Benchmark.App
{
    public class DataReader
    {
        private readonly Stream _stream;
        private readonly byte[] _buff;
        private readonly int _numInitBytesToRead;
        private readonly int _numBytesToRead;
        private int _numBytesRead;

        public DataReader(Stream stream)
            :this(stream, 2 * 1024 * 1024, 4096)
        {
        }

        public DataReader(Stream stream, int numInitBytesToRead, int numBytesToRead)
        {
            _stream = stream;
            _numInitBytesToRead = numInitBytesToRead;
            _numBytesToRead = numBytesToRead;
            _buff = new byte[stream.Length];

            _numBytesRead = stream.Read(_buff, 0, _numInitBytesToRead);
        }

        public byte ReadByte(int offset)
        {
            while(offset >= _numBytesRead)
            {
                var read = _stream.Read(_buff, _numBytesRead, _numBytesToRead);
                _numBytesRead += read;

                if (read == 0)
                    break;
            }

            return _buff[offset];
        }

        public uint ReadUInt(int offset)
        {
            while(offset + sizeof(int) >= _numBytesRead)
            {
                var read = _stream.Read(_buff, _numBytesRead, _numBytesToRead);
                _numBytesRead += read;

                if (read == 0)
                    break;
            }

            var s = _buff.AsSpan(offset, 4);
            return BitConverter.ToUInt32(s);
        }
    }
}
