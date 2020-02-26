using System;
using System.IO;

namespace Benchmark.App
{
    public class MyBufferedStream
    {
        private readonly Stream _stream;
        private readonly byte[] _buff;
        private readonly int _numBytesToRead;
        private int _numBytesRead;

        public MyBufferedStream(Stream stream)
            :this(stream, 2 * 1024 * 1024, 4096)
        {
        }

        public MyBufferedStream(Stream stream, int numInitBytesToRead, int numBytesToRead)
        {
            _stream = stream;
            _numBytesToRead = numBytesToRead;
            _buff = new byte[stream.Length];

            if (numInitBytesToRead > _buff.Length)
                numInitBytesToRead = _buff.Length;

            _numBytesRead = stream.Read(_buff, 0, numInitBytesToRead);
            if(_numBytesToRead == 0)
                _numBytesToRead = (int) stream.Length;
        }

        public byte ReadByte(int offset)
        {
            Read(offset);
            return _buff[offset];
        }

        public uint ReadUInt(int offset)
        {
            Read(offset + sizeof(int));

            var s = _buff.AsSpan(offset, 4);
            return BitConverter.ToUInt32(s);
        }

        private void Read(int offset)
        {
            while (offset >= _numBytesRead)
            {
                var maxRead = _numBytesRead + _numBytesToRead > _buff.Length
                    ? _buff.Length - _numBytesRead
                    : _numBytesToRead;

                var read = _stream.Read(_buff, _numBytesRead, maxRead);
                _numBytesRead += read;

                if (read == 0)
                    break;
            }
        }
    }
}
