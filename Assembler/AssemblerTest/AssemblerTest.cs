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
    [InlineData("mov a, bcd", new byte[0] { })]
    public void test_evaluateMov(string line, byte[] expected_res)
    {
        byte[] actual_res = Assembler.Assembler.evaluateMOV(line);
        Assert.Equal(expected_res, actual_res);

    }

}