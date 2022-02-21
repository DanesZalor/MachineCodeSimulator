using System;
using System.Collections.Generic;

using Xunit;
using CPU;

namespace CPUTests{
    public class CPUTests{
        
        /// <summary> Assert the state of CPU c <br>set the optional parameters to make it undergo assertion </summary>
        private void AssertCPUState(
            CPU.CPU c, short ra=-1, short rb=-1, short rc=-1, short rd=-1,
            short re=-1, short rf=-1, short rg=-1, short sp=-1, short ir=-1, short iar=-1
            ){
            
            short[] args = {ra,rb,rc,rd,re,rf,rg,sp,ir,iar};
            string[] keys= {"ra","rb","rc","rd","re","rf","rg","sp","ir","iar"};
            IDictionary<string,byte> state = c.getState();
            for(int i=0; i<10; i++){
                if(args[i]>-1)
                    Assert.Equal(args[i], state[keys[i]]);
            }
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
                AssertCPUState(cpu, ra:10, rb:0, iar:2);
            }
            { // execute mov b,2 
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                AssertCPUState(cpu, ra:10, rb:2, rc:0, rd:0);
            }
            { // execute "mov c,b" and "mov d,a" 
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                AssertCPUState(cpu, ra:10, rb:2, rc:2, rd:10, iar:8);
            }
            { // execute 0x00 0x00 >> mov a,a
                cpu.InstructionCycleTick();
                IDictionary<string,byte> state = cpu.getState();
                AssertCPUState(cpu, ra:10, iar:10);
            }

        }
    }
}

