using System;
using System.Collections.Generic;

using Xunit;
using CPU;

namespace CPUTests
{

public class CPUTest_Sulay:CPUTest
{
    [Fact]
    public void FMOV_Test()
    {
        byte[] program = 
        {
            0b1000, 2,      // mov a,2
            0b1100, 10,     // mov e, 10
            0b1101, 5,      // mov f, 5
            0b1110, 0b1100,  // mov g, e
            0b10001, 0b1101,// mov b, [f]
            0b11_000, 5,// mov a, [5]
            0b10_0011, 0b1,// mov [b], c
            0b10_1001, 69,// mov [69], b
        };

        CPU.CPU cpu = new CPU.CPU(program);
        
        { // execute mov a,2 
            AssertCPUState(cpu, ra:0, rb:0, rc:0, rd:0, re:0, rf:0, rg:0, iar:0);
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, ra:2, rb:0, rc:0, rd:0, re:0, rf:0, rg:0, iar:2);
        }

        { // execute mov e,10 
            AssertCPUState(cpu, ra:2, rb:0, rc:0, rd:0, re:0, rf:0, rg:0, iar:2);
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, ra:2, rb:0, rc:0, rd:0, re:10, rf:0, rg:0, iar:4);
        }

        { // execute mov f,5 
            AssertCPUState(cpu, ra:2, rb:0, rc:0, rd:0, re:10, rf:0, rg:0, iar:4);
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, ra:2, rb:0, rc:0, rd:0, re:10, rf:5, rg:0, iar:6);
        }

        { // execute mov g,e 
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, ra:2, rb:0, rc:0, rd:0, re:10, rf:5, rg:10, iar:8);
        }

        { // execute mov b,[f]
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, rb:6, iar:10);
        }

        { // execute mov a,[5]
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, ra:170, iar:12);

        }

        { // execute mov [b], c
            Assert.Equal(0b1101,cpu.getRAMState()[13]);
            // 0b1101 : 13

            cpu.InstructionCycleTick();
            Assert.Equal(13,cpu.getRAMState()[13]);
        }

        { // execute mov [69],b
            cpu.InstructionCycleTick();
            Assert.Equal(6, cpu.getRAMState()[69]);   
        } 
    }

    [Fact]
    public void FJMPTest()
    {
        byte[] program = 
        {
            0b0011_0000,        //JMP a
            0b0011_1000, 0b10,  //JMP 2
        };

        CPU.CPU cpu = new CPU.CPU(program);

        
        { // set ra to 3 + execute jmp a 
            cpu.setState(ra: 4, iar:2);
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, iar:4);
        }
    
        { // execute jmp 2
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, iar:2);
        }
    }

    [Fact]
    public void FJCAZTest()
    {
        byte[] program = 
        {
            0b0100_0100, 0b0000_0010, //JC c 
            0b0100_0001, 3,//JZ 3
        };

        CPU.CPU cpu = new CPU.CPU(program);

        { // execute jc c
            cpu.setState(rc:4, aluflags:ALU.FLAG.C);
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, rc:4, iar:4);
        }

        { // execute jz 3
            cpu.setState(aluflags:ALU.FLAG.Z);
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, iar:3);
        }                

    }

    //includes pop
    [Fact]
    public void FPUSHTest()
    {
        byte[] program = 
        {
            0b101_1000, 0b101_001, //PUSH b
            0b110_0000, 0b001,//POP b
            0b101_1001, 0b100,//PUSH 4
            0b110_1001, 0b10,//POP 2
        };

        CPU.CPU cpu = new CPU.CPU(program);

        { // execute "push b"
            cpu.setState(rb:1, iar:2, sp:255);
            AssertCPUState(cpu, sp:255);

            cpu.InstructionCycleTick();
                
            AssertCPUState(cpu, sp:254);
            Assert.Equal(1, cpu.getRAMState()[255]);  
        }

        { // set b to 10 + pop back to b
            cpu.setState(rb:10, iar:4);
            AssertCPUState(cpu, rb:10, iar:4, sp:254);
            
            cpu.InstructionCycleTick();

            AssertCPUState(cpu, rb:1, iar:6, sp:255);

        }

        { // execute push 4
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, sp:254);
            Assert.Equal(4, cpu.getRAMState()[255]);
        }
            
        
        { // execute pop [2]
            cpu.InstructionCycleTick();
            AssertCPUState(cpu, sp:255);
            Assert.Equal(4, cpu.getRAMState()[2]);
            
        }
    }
}
}