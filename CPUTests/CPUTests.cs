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
                if(args[i]>-1){
                    Assert.True(
                        args[i]==state[keys[i]],
                        String.Format(
                            "'{0}' Expected: {1}\t Actual: {2}", 
                            keys[i],args[i], state[keys[i]]
                        )
                    );
                }
            }
        }

        [Fact]
        public void MOVTests(){
            byte[] program = {
                0b1000, 0b1010,         // mov a,10 
                0b1001, 0b10,           // mov b,2
                0b0010, 0b0001,         // mov c,b
                0b0011, 0b0000,         // mov d,a
                0b10010, 0b110001,      // mov c,[b+6]
                0b10100, 0b001,         // mov e,[b]
                0b10000, 0b11001_011,   // mov a,[d-10]
                0b11_101, 10,           // mov f,[10]
                0b11_001, 9,            // mov b,[9]
                0b10_0001, 0b0,         // mov [a],b
                0b10_0101, 0b10111_000, // mov [a-8],f
                0b10_0101, 0b00110_100, // mov [e+6],f
                0b10_1011, 240,         // mov [240],d
                0b10_1001, 129,         // mov [129],b
                0b10_1101, 239,         // mov [239],f
            };
            
            CPU.CPU cpu = new CPU.CPU(program);
            
            { // execute mov a,10 
                AssertCPUState(cpu, ra:0, rb:0, iar:0);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:10, rb:0, iar:2);
            }
            { // execute mov b,2 
                AssertCPUState(cpu, ra:10, rb:0, rc:0, rd:0, iar:2);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:10, rb:2, rc:0, rd:0, iar:4);
            }
            { // execute "mov c,b" and "mov d,a" 
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:10, rb:2, rc:2, rd:10, iar:8);
            }
            { // execute mov c,[b+6]
                AssertCPUState(cpu, rc:2); 
                // rc : 2 -> 10

                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rc:18, iar:10);
            }
            { // execute mov e,[b]
                cpu.InstructionCycleTick();
                AssertCPUState(cpu,re:9,iar:12);
            }
            { // execute mov a,[d-16]
                AssertCPUState(cpu,ra:10); 
                // ra : 10 -> 8

                cpu.InstructionCycleTick();
                AssertCPUState(cpu,ra:8, iar:14);
            }
            { // execute mov f,[10]
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rf:20, iar:16);
            }
            { // execute mov b,[9]
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rb:49, iar:18);
            }
            { // execute mov [a],b
                Assert.Equal(0b10010,cpu.getRAMState()[8]);
                // ram[18] : 0b10010 -> 49

                cpu.InstructionCycleTick();
                Assert.Equal(49,cpu.getRAMState()[8]);
            }
            { // execute mov [a-8],f
                Assert.Equal(8,cpu.getRAMState()[0]);
                // ram[0] : 0b1000 -> 20

                cpu.InstructionCycleTick();
                Assert.Equal(20,cpu.getRAMState()[0]);
            }
            { // execute mov [e+6],f
                Assert.Equal(10, cpu.getRAMState()[15]);
                // ram[15] : 10 -> 20

                cpu.InstructionCycleTick();
                Assert.Equal(20, cpu.getRAMState()[15]);
            }
            { // execute mov [240],d
                cpu.InstructionCycleTick();
                Assert.Equal(10, cpu.getRAMState()[240]);
            }
            { // execute mov [129],b
                cpu.InstructionCycleTick();
                Assert.Equal(49, cpu.getRAMState()[129]);   
            }
            { // execute mov [239],f
                cpu.InstructionCycleTick();
                Assert.Equal(20, cpu.getRAMState()[239]);
                Assert.Equal(10, cpu.getRAMState()[240]);
            }
        }

        [Fact]
        public void JMPTest(){
            byte[] program = {
                0b1110, 10,         // mov g,10
                0b0011_0110,        // jmp g
                3,4,5,6,7,8,9,      // filler
                0b0011_1000, 2,    // jmp 2
            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute mov g,10
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rg:10, iar:2);
            }
            { // execute jmp g
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:10);
            }
        }
        
    }
}

