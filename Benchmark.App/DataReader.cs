using System;
using System.IO;

namespace Benchmark.App
{
    public class DataReader
    {
        private readonly Stream _stream;
        private byte[] _buff;
        private int _maxRead;
        private const int _initialSize = 2 * 1024 * 1024; // Read 2MB initially from disk

        public DataReader(Stream stream)
        {
            _stream = stream;

            if (stream.Length > _initialSize)
            {
                _buff = new byte[_initialSize];
            }
            else
            {
                _buff = new byte[stream.Length];

                stream.Read(_buff, 0, (int)stream.Length - 1);
            }

        }

        public byte ReadByte(int position)
        {
            if (position > _maxRead)
            {
                if (position > _buff.Length - 1)
                {
                    Array.Resize(ref _buff, position + 1);
                }
                var bytesToRead = position - _maxRead + 1;
                _stream.Seek(position, SeekOrigin.Begin);
                if (_stream.Read(_buff, _maxRead, bytesToRead) == -1)
                {
                    throw new IndexOutOfRangeException();
                }
                _maxRead = position;
            }


            return _buff[position];
        }

        public int ReadInt(int position)
        {
            if (position + sizeof(int) > _maxRead)
            {
                if (position + sizeof(int) > _buff.Length - 1)
                {
                    Array.Resize(ref _buff, position + sizeof(int) + 1);
                }
                var bytesToRead = position + sizeof(int) - _maxRead + 1;
                _stream.Seek(position, SeekOrigin.Begin);
                if (_stream.Read(_buff, _maxRead, bytesToRead) == -1)
                {
                    throw new IndexOutOfRangeException();
                }
                _maxRead = position + sizeof(int);
            }

            Span<byte> s = _buff.AsSpan(position, 4);
            return BitConverter.ToInt32(s);
        }
    }
}
