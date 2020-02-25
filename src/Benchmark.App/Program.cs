using System;
using System.Buffers.Binary;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark.App
{

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
            var random = new Random(DateTime.Now.Millisecond);
            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                rndOffset[i] = random.Next(MaxOffset);
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random(123);
            var data = new byte[MaxOffset + 1 + 4];
            random.NextBytes(data);
            File.WriteAllBytes(dummy, data);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            File.Delete(dummy);
        }

        [Benchmark]
        public void ByteFromArray()
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
        public void ByteFromStream()
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
            Span<byte> s = stackalloc byte[4];

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                stream.Seek(rndOffset[i], SeekOrigin.Begin);
                stream.Read(s);
                var integer = BitConverter.ToInt32(s);
            }
        }

        [Benchmark]
        public void IntFromStreamPrimitive()
        {
            using var stream = File.Open(dummy, FileMode.Open);
            Span<byte> s = stackalloc byte[4];

            for (var i = 0; i < rndOffset.Length - 1; i++)
            {
                stream.Seek(rndOffset[i], SeekOrigin.Begin);
                stream.Read(s);
                var integer = BinaryPrimitives.ReadInt32LittleEndian(s);
            }
        }

        [Benchmark]
        public void ByteFromDataReader()
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
    }

    class Program
    {
        static void Main(string[] _)
        {
            BenchmarkRunner.Run<BenchmarkStream>();
        }
    }
}
