using Xunit;
using System.Collections.Generic;
using CPU;

namespace CPUTests{
    public class CPUTests{
        [Fact]
        public void Test1()
        {
            byte[] program = {
                0b1000, 0b1010, // mov a,10 
                0b1001, 0b10,   // mov b,2
                0b0010, 0b0001, // mov c,b
                0b0011, 0b0000  // mov d,a
            };
            
            CPU.CPU cpu = new CPU.CPU(program);
            
            { // execute mov a,10 
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                Assert.Equal(10, state["ra"]);
                Assert.Equal(0, state["rb"]);
            }
            { // execute mov b,2 
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                Assert.Equal(10, state["ra"]);
                Assert.Equal(2, state["rb"]);
            }
            

        }
    }
}

