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
        public void JMPJCAZTest_Kokong(){

            byte[] program = {

            };

            CPU.CPU cpu = new CPU.CPU(program);
        }

        [Fact]
        public void pushPopTest_kokong(){

        }

        [Fact]
        public void MOVTest_Kokong()
        {
            byte[] program = {
                0b0000_1000, 0b0000_0101,   // mov a, 5
                0b0000_0001, 0b0000_0000,   // mov b,a
                0b0010_0001, 0b0010_0000,   // mov [a+2],b
                0b0000_1010, 0b0000_0001,   // mov c, 1
                0b0000_1001, 0b0000_1000,   // mov b, 8
                0b0001_0000, 0b0010_0000,   // mov a,[a+2]  a = 5
                0b0001_1001, 0b0000_0010,   // mov b,[1+1]  b = 5
                0b0010_1000, 0b0000_0010,   // mov [1+1],a  [2] = 1
            };
            
            CPU.CPU cpu = new CPU.CPU(program);
            
            {   //mov a,5
                AssertCPUState(cpu, ra:0, iar:0);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:5, iar:2);
            }
            {   //mov b,a
                AssertCPUState(cpu, ra:5, rb:0, iar:2);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:5, rb:5, iar:4);
            }
            {   //mov [a+2],b
                AssertCPUState(cpu, ra:5, rb:5, rc:0, iar:4);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:5, rb:5, rc:0, iar:6);
            }
            {   //mov c,1  then mov b,8
                AssertCPUState(cpu, ra:5, rb:5, rc:0, iar:6);
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:5, rb:5, rc:1, iar:10);
            }
            {   // mov a,[a+2]
                AssertCPUState(cpu, ra:5, rc:1, iar:10);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:5, rc:1, iar:12);
            }
            {   // mov b,[1+1]
                AssertCPUState(cpu, rb:5, iar:12);
                Assert.Equal(1, cpu.getRAMState()[2]);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rb:1, iar:14);
                Assert.Equal(1, cpu.getRAMState()[2]);                
            }
            {   // mov [1+1],a 
                AssertCPUState(cpu, ra:5, iar:14);
                Assert.Equal(1, cpu.getRAMState()[2]);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, ra:5, iar:16);
                Assert.Equal(5, cpu.getRAMState()[2]);  
            }
        }
        
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
                Assert.Equal(3, cpu.getRAMState()[255]);  
            }
            { // execute push "[g+2]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, sp:253);
                Assert.Equal(69, cpu.getRAMState()[254]);
            }
            { // execute "jmp 7" and "push [6]"
                cpu.InstructionCycleTick();
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:9, sp:252);
                Assert.Equal(123, cpu.getRAMState()[253]);
            }
            { // execute "push 169"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, iar:11, sp:251);
                Assert.Equal(169, cpu.getRAMState()[252]);
            }
            { // execute "pop f"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, rf:169, sp:252);
            }{ // execute "pop [c-5]"
                cpu.setState(rc:10);
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, sp:253);
                Assert.Equal(123, cpu.getRAMState()[5]);
            }{ // execute "pop [5]"
                cpu.InstructionCycleTick();
                AssertCPUState(cpu, sp:254);
                Assert.Equal(69, cpu.getRAMState()[5]);
            }
        }
    }
}

