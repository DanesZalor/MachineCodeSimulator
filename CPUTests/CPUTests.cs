using System;
using System.Collections.Generic;

using Xunit;
using CPU;

namespace CPUTests{
    public class CPUTests{
        
        private void AssertCPUState(short ra=-1, short rb=-1, short rc=-1, short rd=-1,
                                    short re=-1, short rf=-1, short rg=-1, short sp=-1,
                                    short ir=-1, short iar=-1 
                                    ){
            
        }

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
                Assert.Equal(2, state["iar"]);
            }
            { // execute mov b,2 
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                Assert.Equal(10, state["ra"]);
                Assert.Equal(2, state["rb"]);
                Assert.Equal(0, state["rc"]);
                Assert.Equal(0, state["rd"]);
            }
            { // execute "mov c,b" and "mov d,a" 
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                Assert.Equal(10, state["ra"]);
                Assert.Equal(2, state["rb"]);
                Assert.Equal(2, state["rc"]);
                Assert.Equal(10, state["rd"]);
                Assert.Equal(8, state["iar"]);
            }
            { // execute 0x00 0x00 >> mov a,a
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                Assert.Equal(10, state["ra"]);
                Assert.Equal(10, state["iar"]);
            }

            Console.WriteLine(cpu.getState_inString());

        }
    }
}

