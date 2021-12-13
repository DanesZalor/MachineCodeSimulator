using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{
    [Theory]
    // Register
    [InlineData("a", Assembler.Assembler.Terms.Register)]
    [InlineData("b", Assembler.Assembler.Terms.Register)]
    [InlineData("c", Assembler.Assembler.Terms.Register)]
    [InlineData("d", Assembler.Assembler.Terms.Register)]
    [InlineData("e", Assembler.Assembler.Terms.Register)]
    [InlineData("f", Assembler.Assembler.Terms.Register)]
    [InlineData("g", Assembler.Assembler.Terms.Register)]
    [InlineData("h", Assembler.Assembler.Terms.Register, false)]
    [InlineData("sp", Assembler.Assembler.Terms.Register)]
    [InlineData("G", Assembler.Assembler.Terms.Register)]
    // Decimals
    [InlineData("155", Assembler.Assembler.Terms.Decimal)]
    [InlineData("1", Assembler.Assembler.Terms.Decimal)]
    [InlineData("19", Assembler.Assembler.Terms.Decimal)]
    [InlineData("1672", Assembler.Assembler.Terms.Decimal, false)]
    [InlineData("29", Assembler.Assembler.Terms.Decimal)]
    [InlineData("2", Assembler.Assembler.Terms.Decimal)]
    [InlineData("22", Assembler.Assembler.Terms.Decimal)]
    [InlineData("219", Assembler.Assembler.Terms.Decimal)]
    [InlineData("256", Assembler.Assembler.Terms.Decimal, false)]
    [InlineData("279", Assembler.Assembler.Terms.Decimal, false)]
    [InlineData("255", Assembler.Assembler.Terms.Decimal)]
    [InlineData("1100", Assembler.Assembler.Terms.Decimal, false)]
    public void testMatch(string line, string pattern, bool res = true)
    {
        bool actual_res = Assembler.Assembler.match(line, pattern, true);
        Assert.Equal(actual_res, res);
    }

}