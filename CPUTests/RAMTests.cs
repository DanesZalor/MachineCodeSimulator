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
        [InlineData(255, 255)]
        public void RAM_shouldHaveCorrectSize(byte size, byte expected){
            RAM r = new RAM(size);
            Assert.Equal(r.getSize(), expected);
        }

	
        [Theory]
        [InlineData(255,255)]
        [InlineData(2,2)]
        public void RAM_read_shouldReturnTheCorrectData(byte data, byte expected){
            ram.write(5,data);
            Assert.Equal(ram.read(5),expected);
        }

        [Fact]
        public void RAM_correctState(){
            ram.write(0,230);
            ram.write(1,42);
            ram.write(2,12);
            Assert.Equal(230, ram.read(0));
            Assert.Equal(42, ram.read(1));
            Assert.Equal(12, ram.read(2));
        }
    }
}
