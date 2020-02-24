using System;
using System.IO;
using System.IO.Pipelines;
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
        private readonly int[] rndOffset = new int[10_000];
        private const string dummy = "dummy";

        [Params(1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000)]
        public int MaxOffset { get; set; }

        public BenchmarkStream()
        {
            var random = new Random(243);
            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                rndOffset[i] = random.Next(MaxOffset);
            }

            var data = new byte[MaxOffset+1+4];
            random.NextBytes(data);
            File.WriteAllBytes(dummy, data);
        }

        [Benchmark]
        public void BytesFromArray()
        {
            var array = File.ReadAllBytes(dummy);

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                var b = array[rndOffset[i]];
            }
        }

        [Benchmark]
        public void IntFromArray()
        {
            var array = File.ReadAllBytes(dummy);

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                Span<byte> s = array.AsSpan(rndOffset[i], 4);
                var integer = BitConverter.ToInt32(s);
            }
        }

        [Benchmark]
        public void BytesFromStream()
        {
            using var stream = File.Open(dummy, FileMode.Open);

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                stream.Seek(rndOffset[i], SeekOrigin.Begin);
                var b = stream.ReadByte();
            }
        }

        [Benchmark]
        public void IntFromStream()
        {
            using var stream = File.Open(dummy, FileMode.Open);

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                Span<byte> s = stackalloc byte[4];
                stream.Seek(rndOffset[i], SeekOrigin.Begin);
                stream.Read(s);
                var integer = BitConverter.ToInt32(s);
            }
        }

        [Benchmark]
        public void BytesFromDataReader()
        {
            using var stream = File.Open(dummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                var b = dr.ReadByte(rndOffset[i]);
            }
        }

        [Benchmark]
        public void IntFromDataReader()
        {
            using var stream = File.Open(dummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                var integer = dr.ReadInt(rndOffset[i]);
            }
        }

        //[Benchmark]
        //public void BytesFromPipeline()
        //{
        //    var pipe = new Pipe();
            
        //}
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
