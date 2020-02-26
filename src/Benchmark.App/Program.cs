using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark.App
{

    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class BenchmarkStream
    {
        private readonly int[] _rndOffset = new int[1_000];
        private const string Dummy = "dummy";

        [Params(10_000, 1_000_000, 10_000_000, 100_000_000)]
        public int MaxOffset { get; set; }

        public BenchmarkStream()
        {
            var random = new Random(234);
            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                _rndOffset[i] = random.Next(MaxOffset);
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random(123);
            var data = new byte[MaxOffset + 1 + 4];
            random.NextBytes(data);
            File.WriteAllBytes(Dummy, data);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            File.Delete(Dummy);
        }

        [Benchmark]
        public void ByteFromArray()
        {
            var array = File.ReadAllBytes(Dummy);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                var b = array[_rndOffset[i]];
                var x = b;
            }
        }

        [Benchmark]
        public void IntFromArray()
        {
            var array = File.ReadAllBytes(Dummy);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                var s = array.AsSpan(_rndOffset[i], 4);
                var integer = BitConverter.ToInt32(s);
                var x = integer;
            }
        }

        [Benchmark]
        public void ByteFromStream()
        {
            using var stream = File.Open(Dummy, FileMode.Open);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                stream.Seek(_rndOffset[i], SeekOrigin.Begin);
                var b = (byte) stream.ReadByte();
                var x = b;
            }
        }

        [Benchmark]
        public void IntFromStream()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            Span<byte> s = stackalloc byte[4];

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                stream.Seek(_rndOffset[i], SeekOrigin.Begin);
                stream.Read(s);
                var integer = BitConverter.ToInt32(s);
                var x = integer;
            }
        }

        [Benchmark]
        public void ByteFromDataReader()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            var dr = new MyBufferedStream(stream);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                var b = dr.ReadByte(_rndOffset[i]);
                var x = b;
            }
        }

        [Benchmark]
        public void IntFromDataReader()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            var dr = new MyBufferedStream(stream);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                var integer = dr.ReadUInt(_rndOffset[i]);
                var x = integer;
            }
        }

        [Benchmark]
        public void ByteFromBufferedStream()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            using var bs = new BufferedStream(stream);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                stream.Seek(_rndOffset[i], SeekOrigin.Begin);
                var b = (byte)stream.ReadByte();
                var x = b;
            }
        }

        [Benchmark]
        public void IntFromBufferedStream()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            var bs = new BufferedStream(stream);
            Span<byte> s = stackalloc byte[4];

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                stream.Seek(_rndOffset[i], SeekOrigin.Begin);
                stream.Read(s);
                var integer = BitConverter.ToInt32(s);
                var x = integer;
            }
        }

        [Benchmark]
        public void ByteFromBufferedStreamBinaryReader()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            using var bs = new BufferedStream(stream);
            using var br = new BinaryReader(bs);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                stream.Seek(_rndOffset[i], SeekOrigin.Begin);
                var b = br.ReadByte();
                var x = b;
            }
        }

        [Benchmark]
        public void IntFromBufferedStreamBinaryReader()
        {
            using var stream = File.Open(Dummy, FileMode.Open);
            using var bs = new BufferedStream(stream);
            using var br = new BinaryReader(bs);

            for (var i = 0; i < _rndOffset.Length - 1; i++)
            {
                stream.Seek(_rndOffset[i], SeekOrigin.Begin);
                var integer = br.ReadUInt32();
                var x = integer;
            }
        }
    }

    class Program
    {
        static void Main(string[] _)
        {
            BenchmarkRunner.Run<BenchmarkStream>();
        }
    }
}
