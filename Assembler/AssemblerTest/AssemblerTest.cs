using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{
    [Theory]
    // REGISTER testing
    [InlineData("a", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("b", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("c", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("d", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("e", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("f", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("g", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("h", Assembler.Assembler.TERMS.REGISTER, false)]
    [InlineData("sp", Assembler.Assembler.TERMS.REGISTER)]
    [InlineData("G", Assembler.Assembler.TERMS.REGISTER)]
    // DECIMALs
    [InlineData("155", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("1", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("19", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("1672", Assembler.Assembler.TERMS.DECIMAL, false)]
    [InlineData("29", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("2", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("22", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("219", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("256", Assembler.Assembler.TERMS.DECIMAL, false)]
    [InlineData("279", Assembler.Assembler.TERMS.DECIMAL, false)]
    [InlineData("255", Assembler.Assembler.TERMS.DECIMAL)]
    [InlineData("1100", Assembler.Assembler.TERMS.DECIMAL, false)]
    // OFFSET
    [InlineData("+1", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("+5", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("+14", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("+16", Assembler.Assembler.TERMS.OFFSET, false)]
    [InlineData("+0", Assembler.Assembler.TERMS.OFFSET, false)]
    [InlineData("-1", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("-5", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("-13", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("-16", Assembler.Assembler.TERMS.OFFSET)]
    [InlineData("-17", Assembler.Assembler.TERMS.OFFSET, false)]
    // ADDRESS
    [InlineData("[a]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[c]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[201]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[D+12]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[D+15]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[D+16]", Assembler.Assembler.TERMS.ADDRESS, false)]
    [InlineData("[D-0]", Assembler.Assembler.TERMS.ADDRESS, false)]
    [InlineData("[D+0]", Assembler.Assembler.TERMS.ADDRESS, false)]
    [InlineData("[D-1]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[D-9]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[D-16]", Assembler.Assembler.TERMS.ADDRESS)]
    [InlineData("[D-17]", Assembler.Assembler.TERMS.ADDRESS, false)]
    // Instructions
    [InlineData("mov a,a", Assembler.Assembler.INSTRUCTIONS.MOV)]
    public void testMatch(string line, string pattern, bool res = true)
    {
        bool actual_res = Assembler.Assembler.match(line, pattern, true);
        Assert.Equal(actual_res, res);
    }

}