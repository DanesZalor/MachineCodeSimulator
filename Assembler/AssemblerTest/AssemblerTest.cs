using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{

    [Theory]
    [InlineData("mov a, b", new byte[2] { 0b0000_0000, 0b0000_0001 })]
    [InlineData("mov g,c", new byte[2] { 0b0000_0110, 0b0000_0010 })]
    [InlineData("mov d, SP", new byte[2] { 0b0000_0011, 0b0000_0111 })]
    [InlineData("mov a, bcd", new byte[0] { })]
    [InlineData("mov D,254", new byte[2] { 0b000_1011, 0b1111_1110 })]
    [InlineData("mov D   ,  31", new byte[2] { 0b000_1011, 0b0001_1111 })]
    [InlineData("mov a, [a+12]", new byte[2] { 0b001_0000, 0b0110_0000 })]
    [InlineData("mov a, [ a - 12 ]", new byte[2] { 0b001_0000, 0b1101_1000 })]
    [InlineData("mov a, [ a - 1 2 ]", new byte[0] { })]
    [InlineData("mov e, [g+1 ]", new byte[2] { 0b001_0100, 0b0000_1110 })]
    [InlineData("mov e, [ g-1]", new byte[2] { 0b001_0100, 0b1000_0110 })] //refer to the diagrams
    public void test_evaluateMov(string line, byte[] expected_res)
    {
        byte[] actual_res = Assembler.Assembler.translateMOV(line);
        Assert.Equal(expected_res, actual_res);
    }

}