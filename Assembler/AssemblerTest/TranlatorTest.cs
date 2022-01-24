using Xunit;
using Assembler;
using System;

namespace AssemblerTest;

/** Contains no failing test cases as by the time the assembly process 
* reaches translation phase, it is assumed that the program is already
* syntactically correct.
*/
public class TranslationCheck
{
    public class TranslationTest
    {
        [Theory]
        // MOV Instruction
        [InlineData("mov a, b", new byte[2] { 0b0000_0000, 0b0000_0001 })]
        [InlineData("mov g,c", new byte[2] { 0b0000_0110, 0b0000_0010 })]
        [InlineData("mov d, SP", new byte[2] { 0b0000_0011, 0b0000_0111 })]
        // DATA Instruction
        [InlineData("mov D,254", new byte[2] { 0b000_1011, 254 })]
        [InlineData("mov D   ,  31", new byte[2] { 0b000_1011, 31 })]
        [InlineData("mov D   , 031", new byte[2] { 0b000_1011, 31 })]
        [InlineData("mov D   ,  0000000001", new byte[2] { 0b000_1011, 1 })]
        [InlineData("mov D   ,  00", new byte[2] { 0b000_1011, 0 })]
        // LOAD Reg, [Reg+Offset] Instruction
        [InlineData("mov a, [a+12]", new byte[2] { 0b001_0000, 0b0110_0000 })]
        [InlineData("mov a, [ a - 12 ]", new byte[2] { 0b001_0000, 0b1101_1000 })]
        [InlineData("mov e, [g+1 ]", new byte[2] { 0b001_0100, 0b0000_1110 })]
        [InlineData("mov e, [ g-1]", new byte[2] { 0b001_0100, 0b1000_0110 })]
        // LOAD Reg, [Reg] Instruction
        [InlineData("mov e, [g]", new byte[2] { 0b0001_0100, 0b0000_0110 })]
        [InlineData("mov e , [ g ]", new byte[2] { 0b0001_0100, 0b0000_0110 })]
        [InlineData("mov a , [ sp ]", new byte[2] { 0b0001_0000, 0b0000_0111 })]
        // LOAD Reg, [Const] Instruction
        [InlineData("mov e, [255]", new byte[2] { 0b0001_1100, 0b1111_1111 })]
        [InlineData("mov f, [63]", new byte[2] { 0b0001_1101, 0b0011_1111 })]
        // STORE [Reg+Offset], Reg
        [InlineData("mov [b+7], c", new byte[2] { 0b0010_0001, 0b0011_1010 })]
        [InlineData("mov [ b + 7 ], c", new byte[2] { 0b0010_0001, 0b0011_1010 })]
        [InlineData("mov [b-7], c", new byte[2] { 0b0010_0001, 0b1011_0010 })]
        [InlineData("mov [      b - 7 ], c", new byte[2] { 0b0010_0001, 0b1011_0010 })]
        // STORE [Reg], Reg
        [InlineData("mov [b], c", new byte[2] { 0b0010_0001, 0b0000_0010 })]
        [InlineData("mov [ b ], c", new byte[2] { 0b0010_0001, 0b0000_0010 })]
        [InlineData("mov [f], sp", new byte[2] { 0b0010_0101, 0b0000_0111 })]
        // STORE [Const], Reg
        [InlineData("mov [255], c", new byte[2] { 0b0010_1010, 255 })]
        [InlineData("mov [128], sp", new byte[2] { 0b0010_1111, 128 })]
        // JMP Reg
        [InlineData("jmp b", new byte[1] { 0b0011_0001 })]
        [InlineData("jmp g", new byte[1] { 0b0011_0110 })]
        [InlineData("jmp   g  ", new byte[1] { 0b0011_0110 })]
        // JMP Const
        [InlineData("   jmp 127", new byte[2] { 0b0011_1000, 127 })]
        [InlineData("   jmp   127  ", new byte[2] { 0b0011_1000, 127 })]
        // JCAZ Reg
        [InlineData("jc a", new byte[2] { 0b0100_0100, 0b0000_0000 })]
        [InlineData("ja b", new byte[2] { 0b0100_0010, 0b0000_0001 })]
        [InlineData("jz c", new byte[2] { 0b0100_0001, 0b0000_0010 })]
        [InlineData("jca sp", new byte[2] { 0b0100_0110, 0b0000_0111 })]
        [InlineData("jc   c", new byte[2] { 0b0100_0100, 0b0000_0010 })]
        // JCAZ Const
        [InlineData("jaz 254", new byte[2] { 0b0100_1011, 254 })]
        [InlineData("jcz   254", new byte[2] { 0b0100_1101, 254 })]
        public void translatesAssemblyInstructionsToMachineCodeCorrectly(string line, byte[] expected_res)
        {
            byte[] actual_res = Assembler.Translator.translateLine(line);
            Assert.Equal(expected_res, actual_res);
        }
    }
}