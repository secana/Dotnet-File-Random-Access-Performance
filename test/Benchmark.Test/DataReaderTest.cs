using System;
using System.IO;
using Benchmark.App;
using Xunit;

namespace Benchmark.Test
{
    public class DataReaderTest
    {
        private readonly Stream _stream
            = new MemoryStream(new byte[]
        {
            0x11, // 0
            0x22, 
            0x33,
            0x44,
            0x55,
            0x66, // 5
            0x77,
            0x88,
            0x99,
            0xaa,
            0xbb, // 10
            0xcc
        });

        [Theory]
        [InlineData(0, 0x11)]
        [InlineData(5, 0x66)]
        [InlineData(11, 0xcc)]
        public void ReadByte_GivenAnOffset_ReturnsCorrectByte(int offset, byte expected)
        {
            var dr = new DataReader(_stream, 1, 1);

            Assert.Equal(expected, dr.ReadByte(offset));
        }

        [Theory]
        [InlineData(0, 0x44332211)]
        [InlineData(5, 0x99887766)]
        public void ReadInt_GivenAnOffset_ReturnsCorrectInt(int offset, uint expected)
        {
            var dr = new DataReader(_stream, 4, 4);

            Assert.Equal(expected, dr.ReadUInt(offset));
        }

        [Theory]
        [InlineData(-1)]
        public void ReadInt_GivenAnTooLargeOffset_ThrowsArgumentOutOfRangeException(int offset)
        {
            var dr = new DataReader(_stream, 1, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => dr.ReadUInt(offset));
        }

        [Theory]
        [InlineData(9)]
        [InlineData(11)]
        [InlineData(12)]
        public void ReadInt_GivenAnTooLargeOffset_ThrowsArgumentException(int offset)
        {
            var dr = new DataReader(_stream, 1, 1);

            Assert.Throws<ArgumentException>(() => dr.ReadUInt(offset));
        }
    }
}
