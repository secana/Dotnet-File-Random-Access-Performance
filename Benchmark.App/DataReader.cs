using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Benchmark.App
{
    public class DataReader
    {
        private readonly Stream _stream;
        private byte[] _buff;
        private const int _initialSize = 2 * 1024 * 1024; // Read 2MB initially from disk
        private const int _maxStepSize = 1 * 1024 * 1024; // Read at most 1MB each time the buffer runs out of space

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Resize(int offset, int readSize)
        {
            if(offset + readSize > _buff.Length - 1)
            {
                var resizeBy = offset - _buff.Length + 1;

                resizeBy = resizeBy < _maxStepSize 
                    ? _maxStepSize 
                    : resizeBy;

                resizeBy = resizeBy + _buff.Length > _stream.Length
                    ? (int) _stream.Length - _buff.Length
                    : resizeBy;


                Array.Resize(ref _buff, _buff.Length + resizeBy);

                return resizeBy;
            }
            else
            {
                return 0;
            }
        }

        public byte ReadByte(int offset)
        {
            if (offset > _buff.Length)
            {
                var resizedBy = Resize(offset, sizeof(byte));
                _stream.Seek(offset, SeekOrigin.Begin);
                if (_stream.Read(_buff, _buff.Length, resizedBy) == -1)
                {
                    throw new IndexOutOfRangeException();
                }
            }


            return _buff[offset];
        }

        public int ReadInt(int offset)
        {
            if (offset + sizeof(int) > _buff.Length)
            {
                var resized = Resize(offset, sizeof(int));
                _stream.Seek(offset, SeekOrigin.Begin);
                if (_stream.Read(_buff, _buff.Length, resized) == -1)
                {
                    throw new IndexOutOfRangeException();
                }
            }

            Span<byte> s = _buff.AsSpan(offset, 4);
            return BitConverter.ToInt32(s);
        }
    }
}
