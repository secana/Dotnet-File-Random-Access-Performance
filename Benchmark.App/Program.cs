using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

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

    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class BenchmarkStream
    {
        private readonly int[] smallRnd = new int[10_000];
        private readonly int[] largeRnd = new int[10_000];
        private const string smallDummy = "smalldummy";
        private const string largeDummy = "largedummy";

        public BenchmarkStream()
        {
            var random = new Random(243);
            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                smallRnd[i] = random.Next(1_000_000);
            }
            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                largeRnd[i] = random.Next(10_000_000);
            }
        }

        [Benchmark]
        public void LowBytesFromSmallArray()
        {
            var array = File.ReadAllBytes(smallDummy);

            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                var b = array[smallRnd[i]];
            }
        }

        [Benchmark]
        public void LowBytesFromLargeArray()
        {
            var array = File.ReadAllBytes(largeDummy);

            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                var b = array[largeRnd[i]];
            }
        }

        [Benchmark]
        public void HighBytesFromLargeArray()
        {
            var array = File.ReadAllBytes(largeDummy);

            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                var b = array[largeRnd[i]];
            }
        }

        [Benchmark]
        public void LowBytesFromSmallStream()
        {
            using var stream = File.Open(smallDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                var b = dr.ReadByte(smallRnd[i]);
            }
        }

        [Benchmark]
        public void LowBytesFromLargeStream()
        {
            using var stream = File.Open(smallDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                var b = dr.ReadByte(smallRnd[i]);
            }
        }

        [Benchmark]
        public void HighBytesFromLargeStream()
        {
            using var stream = File.Open(largeDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                var b = dr.ReadByte(largeRnd[i]);
            }
        }

        [Benchmark]
        public void LowIntFromSmallArray()
        {
            var array = File.ReadAllBytes(smallDummy);

            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                Span<byte> s = array.AsSpan(smallRnd[i], 4);
                var integer = BitConverter.ToInt32(s);
            }
        }

        [Benchmark]
        public void LowIntFromLargeArray()
        {
            var array = File.ReadAllBytes(largeDummy);

            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                Span<byte> s = array.AsSpan(largeRnd[i], 4);
                var integer = BitConverter.ToInt32(s);
            }
        }

        [Benchmark]
        public void HighIntFromLargeArray()
        {
            var array = File.ReadAllBytes(largeDummy);

            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                Span<byte> s = array.AsSpan(largeRnd[i], 4);
                var integer = BitConverter.ToInt32(s);
            }
        }

        [Benchmark]
        public void LowIntFromSmallStream()
        {
            using var stream = File.Open(smallDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                var integer = dr.ReadInt(smallRnd[i]);
            }
        }

        [Benchmark]
        public void LowIntFromLargeStream()
        {
            using var stream = File.Open(smallDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < smallRnd.Length - 1; i++)
            {
                var integer = dr.ReadInt(smallRnd[i]);
            }
        }

        [Benchmark]
        public void HighIntFromLargeStream()
        {
            using var stream = File.Open(largeDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < largeRnd.Length - 1; i++)
            {
                var integer = dr.ReadInt(largeRnd[i]);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkStream>();
            // var b = new BenchmarkStream();
            // b.GetBytesFromStream();
        }
    }
}
