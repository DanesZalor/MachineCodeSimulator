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
    [InlineData("mov a, [a-12]", new byte[2] { 0b001_0000, 0 })]
    public void test_evaluateMov(string line, byte[] expected_res)
    {
        byte[] actual_res = Assembler.Assembler.evaluateMOV(line);
        Assert.Equal(expected_res, actual_res);
    }

}