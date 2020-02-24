using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark.App
{

    public class DataReader
    {
        private Stream _stream;
        private byte[] _buff;
        private int _maxRead;
        private const int _initialSize = 2_000_000;

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
                    throw new IndexOutOfRangeException("Shit hit the fan.");
                }
                _maxRead = position;
            }


            return _buff[position];
        }
    }

    [MemoryDiagnoser]
    public class BenchmarkStream
    {
        private int[] randPos = new int[100_000];
        private int[] randPosLarge = new int[100_000];
        private const string smallDummy = "smalldummy";
        private const string largeDummy = "largedummy";

        public BenchmarkStream()
        {
            var random = new Random(243);
            for (var i = 0; i < randPos.Length - 1; i++)
            {
                randPos[i] = random.Next(1_000_000);
            }
            for (var i = 0; i < randPosLarge.Length - 1; i++)
            {
                randPosLarge[i] = random.Next(10_000_000);
            }
        }

        [Benchmark]
        public void GetLargeBytesFromArray()
        {
            var array = File.ReadAllBytes(largeDummy);

            for (var i = 0; i < randPosLarge.Length - 1; i++)
            {
                var b = array[randPosLarge[i]];
            }
        }

        [Benchmark]
        public void GetBytesFromSmallArray()
        {
            var array = File.ReadAllBytes(smallDummy);

            for (var i = 0; i < randPos.Length - 1; i++)
            {
                var b = array[randPos[i]];
            }
        }

        [Benchmark]
        public void GetBytesFromSmallStream()
        {
            using var stream = File.Open(smallDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < randPos.Length - 1; i++)
            {
                var b = dr.ReadByte(randPos[i]);
            }
        }

        [Benchmark]
        public void GetLargeBytesFromStream()
        {
            using var stream = File.Open(largeDummy, FileMode.Open);
            var dr = new DataReader(stream);

            for (var i = 0; i < randPosLarge.Length - 1; i++)
            {
                var b = dr.ReadByte(randPosLarge[i]);
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
