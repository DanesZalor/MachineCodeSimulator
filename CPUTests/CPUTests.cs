using Xunit;
using CPU;

namespace CPUTests{
    public class CPUTests{
        [Fact]
        public void Test1()
        {
            byte[] program = {
                0b1000, 0b1010, // mov a,10 
                0b1001, 0b10,   // mov b,2
                //0b0000, 0b0001  // mov a,b
            };
            
            CPU.CPU cpu = new CPU.CPU(program);
            cpu.InstructionCycleTick();
            cpu.InstructionCycleTick();
            System.Console.WriteLine(cpu.getState());
        }
    }
}

