using Xunit;
using System;
using CPU;

namespace CPUTests{
    
    public class RAMTests{
        CPU.RAM ram;
        
        [Theory]
        [InlineData(16,16)]
        [InlineData(69,69)]
        [InlineData(65535, 65535)]
        public void RAM_shouldHaveCorrectSize(ushort size, ushort expected){
            CPU.RAM r = new CPU.RAM(size);
            Assert.Equal(r.getSize(), expected);
        }

        [Fact]
        public void RAM_shouldWriteToTwoAdjacentCells(){
            ram = new CPU.RAM(16);
            ushort data = 17013;
            ram.write(5,data);
            byte b1 = ram.read1(5);
            byte b2 = ram.read1(6);

            Assert.Equal(b1, (byte)(data>>8));
            Assert.Equal(b2, (byte)data);
        }

        [Theory]
        [InlineData(255,255)]
        [InlineData(65432,65432)]
        public void RAM_read_shouldReturnTheCorrectData(ushort data, ushort expected){
            ram = new CPU.RAM(13);
            ram.write(5,data);
            Assert.Equal(ram.read(5),expected);
        }
    }
    public class CPUTests{
        [Fact]
        public void Test1()
        {

        }
    }
}

