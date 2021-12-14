using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{

    [Theory]
    // Test MOV Instruction
    [InlineData("mov a, b", new byte[2] { 0b0000_0000, 0b0000_0001 })]
    [InlineData("mov g,c", new byte[2] { 0b0000_0110, 0b0000_0010 })]
    [InlineData("mov d, SP", new byte[2] { 0b0000_0011, 0b0000_0111 })]
    [InlineData("mov a, bcd", new byte[0] { })]
    // Test DATA Instruction
    [InlineData("mov D,254", new byte[2] { 0b000_1011, 0b1111_1110 })]
    [InlineData("mov D   ,  31", new byte[2] { 0b000_1011, 0b0001_1111 })]
    // Test LOAD Reg, [Reg+Offset] Instruction
    [InlineData("mov a, [a+12]", new byte[2] { 0b001_0000, 0b0110_0000 })]
    [InlineData("mov a, [ a - 12 ]", new byte[2] { 0b001_0000, 0b1101_1000 })]
    [InlineData("mov a, [ a - 1 2 ]", new byte[0] { })]
    [InlineData("mov e, [g+1 ]", new byte[2] { 0b001_0100, 0b0000_1110 })]
    [InlineData("mov e, [ g-1]", new byte[2] { 0b001_0100, 0b1000_0110 })]
    // Test LOAD Reg, [Reg] Instruction
    [InlineData("mov e, [g]", new byte[2] { 0b0001_0100, 0b0000_0110 })]
    [InlineData("mov e , [ g ]", new byte[2] { 0b0001_0100, 0b0000_0110 })]
    [InlineData("mov a , [ sp ]", new byte[2] { 0b0001_0000, 0b0000_0111 })]
    // Test LOAD Reg, [Const] Instruction
    [InlineData("mov e, [255]", new byte[2] { 0b0001_1100, 0b1111_1111 })]
    [InlineData("mov f, [63]", new byte[2] { 0b0001_1101, 0b0011_1111 })]
    public void test_evaluateMov(string line, byte[] expected_res)
    {
        byte[] actual_res = Assembler.Assembler.translateLine(line);
        Assert.Equal(expected_res, actual_res);
    }

}