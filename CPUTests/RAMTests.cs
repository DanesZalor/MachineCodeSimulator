using Xunit;
using CPU;

namespace CPUTests{
    public class RAMTests{
        RAM ram;
        
        public RAMTests(){
            ram = new RAM(16);
        }

        [Theory]
        [InlineData(16,16)]
        [InlineData(69,69)]
        [InlineData(65535, 65535)]
        public void RAM_shouldHaveCorrectSize(ushort size, ushort expected){
            RAM r = new RAM(size);
            Assert.Equal(r.getSize(), expected);
        }

        [Fact]
        public void RAM_shouldWriteToTwoAdjacentCells(){
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
            ram.write(5,data);
            Assert.Equal(ram.read(5),expected);
        }
    }
}
