using System;
using System.Collections.Generic;

using Xunit;
using CPU;

namespace CPUTests{
    public class CPUTests{
        
        /// <summary> Assert the state of CPU c <br>set the optional parameters to make it undergo assertion </summary>
        private void AssertCPUState(
            CPU.CPU c, short ra=-1, short rb=-1, short rc=-1, short rd=-1,
            short re=-1, short rf=-1, short rg=-1, short sp=-1, short ir=-1, 
            short iar=-1, ALU.FLAG aluflags = ALU.FLAG.NONE
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

            if(aluflags != ALU.FLAG.NONE)
                Assert.Equal(aluflags, c.getALUFlags());
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
                Assert.Equal(0b10010,cpu.readFromRAM(8));
                // ram[18] : 0b10010 -> 49

                cpu.InstructionCycleTick();
                Assert.Equal(49,cpu.readFromRAM(8));
            }
            { // execute mov [a-8],f
                Assert.Equal(8,cpu.readFromRAM(0));
                // ram[0] : 0b1000 -> 20

                cpu.InstructionCycleTick();
                Assert.Equal(20,cpu.readFromRAM(0));
            }
            { // execute mov [e+6],f
                Assert.Equal(10, cpu.readFromRAM(15));
                // ram[15] : 10 -> 20

                cpu.InstructionCycleTick();
                Assert.Equal(20, cpu.readFromRAM(15));
            }
            { // execute mov [240],d
                cpu.InstructionCycleTick();
                Assert.Equal(10, cpu.readFromRAM(240));
            }
            { // execute mov [129],b
                cpu.InstructionCycleTick();
                Assert.Equal(49, cpu.readFromRAM(129));   
            }
            { // execute mov [239],f
                cpu.InstructionCycleTick();
                Assert.Equal(20, cpu.readFromRAM(239));
                Assert.Equal(10, cpu.readFromRAM(240));
            }
        }

        [Fact]
        public void JMPTest(){
            byte[] program = {
                0b1110, 10,         // mov g,10
                0b0011_0110,        // jmp g
                3,4,5,6,7,8,9,      // filler
                0b11_1000, 2,    // jmp 2
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
            { // execute jmp 2
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:2);
            }
        }

        [Fact]
        public void JCAZTest(){
            
            byte[] program = {
                0b100_0100, 0b1,        // jc b
                0,0,0,0,0,0,            // filler
                0b100_1010, 0,          // ja 0
            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute "jc b"
                cpu.setState(rb:8, aluflags:ALU.FLAG.C);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rb:8, iar:8);
            }
            { // execute "ja 0"
                cpu.setState(aluflags:ALU.FLAG.A);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:0);
            }
            { // execute "jc b" again but with wrong flags
                cpu.setState(aluflags:ALU.FLAG.Z);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu,iar:2);
            }
            { // execute fillers "0,0" = "mov a,a" which does nothing
                AssertCPUState(cpu, iar:2);
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:8);
            }
            { // execute "ja 0" again but with wrong flags
                cpu.setState(aluflags:ALU.FLAG.C);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu,iar:10);
            }
        }
        
        [Fact]
        public void PUSHTest(){
            byte[] program = {
                0b101_0110,                 // push g
                0b101_1000, 0b10_110,       // push [g+2]
                0b11_1000,7,                // jmp 7
                69,123,
                0b101_1001, 6,              // push [6] 
                0b101_1010, 169,            // push 169
                0b110_0101,                 // pop f
                0b110_1000, 0b10100_010,    // pop [c-5] // point to 69 or @5
                0b110_1001, 5               // pop [5]
            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute "push g"
                cpu.setState(rg:3);
                AssertCPUState(cpu, sp:255);

                cpu.InstructionCycleTick();
                
                AssertCPUState(cpu, sp:254);
                Assert.Equal(3, cpu.readFromRAM(255));  
            }
            { // execute push "[g+2]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, sp:253);
                Assert.Equal(69, cpu.readFromRAM(254));
            }
            { // execute "jmp 7" and "push [6]"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:9, sp:252);
                Assert.Equal(123, cpu.readFromRAM(253));
            }
            { // execute "push 169"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:11, sp:251);
                Assert.Equal(169, cpu.readFromRAM(252));
            }
            { // execute "pop f"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rf:169, sp:252);
            }{ // execute "pop [c-5]"
                cpu.setState(rc:10);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, sp:253);
                Assert.Equal(123, cpu.readFromRAM(5));
            }{ // execute "pop [5]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, sp:254);
                Assert.Equal(69, cpu.readFromRAM(5));
            }
        }

        [Fact]
        public void CALLandRETtest(){
            byte[] program = {
                0b1000, 7,      // mov a,7
                0b111_0000,     // call a
                0b1000, 0,      // mov a,0
                0b1001, 0,      // mov b,0
                0b1000, 2,      // mov a,2
                0b1001, 3,      // mov b,3
                0b1101_0001     // ret
                

            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute "mov a,7" and "call a"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:7, iar:7);
                Assert.Equal(3, cpu.readFromRAM(255));
            }
            { // execute "mov a,2" & "mov b,3"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:2, rb:3);
            }
            { // execute "ret"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:3);
            }
            { // execute "mov a,0" & "mov b,0"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:7, ra:0, rb:0);
            }
        }

        [Fact]
        public void CALLandRETtest2(){
            byte[] program = {
                0b111_1000, 6,  // call 6
                0b1000, 0,      // mov a,0
                0b1001, 0,      // mov b,0
                0b1000, 2,      // mov a,2
                0b1001, 3,      // mov b,3
                0b1101_0001     // ret
                

            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute "mov a,7" and "call a"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:6);
                Assert.Equal(2, cpu.readFromRAM(255));
            }
            { // execute "mov a,2" & "mov b,3"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:2, rb:3);
            }
            { // execute "ret"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:2);
            }
            { // execute "mov a,0" & "mov b,0"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:6, ra:0, rb:0);
            }
        }
        
        [Fact]
        public void ALU1_Tests(){
            byte[] program = {
                0b1110, 240,    // mov g,240
                0b10000_110,    // not g
                0b10001_110,    // inc g
                0b10100_110,    // shr g
                0b10010_110,    // dec g
                0b10011_110,    // shl g
            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute "mov g,240" and "not g" 
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:3, rg: 15);
            }
            { // execute "inc g"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:4, rg:16);
            }
            { // execute "shr g"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:5, rg:8);
            }
            { // execute "dec g"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:6, rg:7);
            }
            { //execute"shl g"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rg:14);
            }
        }

        [Fact]
        public void ALU2_Tests(){
            byte[] program = {
                0b1010, 10,                             // mov c,10
                0b1011, 20,                             // mov d,20
                0b1100_0000, 0b0011_0010,               // cmp d,c
                0b1100_0000, 0b0010_0011,               // cmp c,d
                0b1100_0000, 0b0011_0011,               // cmp c,d
                0b1100_0000, 0b0011_1001, 1,            // cmp d,[1]
                0b1100_0000, 0b0011_1001, 3,            // cmp d,[3]
                0b1100_0000, 0b0011_1001, 4,            // cmp d,[4]
                0b1100_0000, 0b0010_1010, 10,           // cmp c,10
                0b1100_0000, 0b0010_1010, 9,            // cmp c,9
                0b1100_0000, 0b0010_1010, 11,           // cmp c,11
                0b1100_0000, 0b0010_1000, 0b1_000,      // cmp c,[a+1]
                0b1100_0000, 0b0010_1000, 0b10_000,     // cmp c,[a+2]
                0b1100_0000, 0b0010_1000, 0b1111_000,   // cmp c,[a+15]
            };
            CPU.CPU cpu = new CPU.CPU(program);

            { // execute "mov c,10", "mov d,20" & "cmp d,c"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:6, rc:10, rd:20, aluflags:ALU.FLAG.A);
            }
            { // execte "cmp c,d"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:8, aluflags:ALU.FLAG.C);
            }
            { // execute "cmp d,d"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:10, aluflags:ALU.FLAG.Z);
            }
            { // execute "cmp d,[1]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:13, aluflags:ALU.FLAG.A);
            }
            { // execute "cmp d,[3]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:16, aluflags:ALU.FLAG.Z);
            }
            { // execute "cmp d,[4]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:19, aluflags:ALU.FLAG.C);
            }
            { // execute "cmp c,10"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:22, aluflags:ALU.FLAG.Z);
            }
            { // execute "cmp c,9"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:25, aluflags:ALU.FLAG.A);
            }
            { // execute "cmp c,11"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:28, aluflags:ALU.FLAG.C);
            }
            { // execute "cmp c,[a+1]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:31, aluflags:ALU.FLAG.Z);
            }
            { // execute "cmp c,[a+2]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:34, aluflags:ALU.FLAG.C);
            }
            { // execute "cmp c,[a+15]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:37, aluflags:ALU.FLAG.A);
            }
        }
    }
}

