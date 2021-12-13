using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{
    [Theory]
    // Register testing
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
    // Offset
    [InlineData("+1", Assembler.Assembler.Terms.Offset)]
    [InlineData("+5", Assembler.Assembler.Terms.Offset)]
    [InlineData("+14", Assembler.Assembler.Terms.Offset)]
    [InlineData("+16", Assembler.Assembler.Terms.Offset, false)]
    [InlineData("+0", Assembler.Assembler.Terms.Offset, false)]
    [InlineData("-1", Assembler.Assembler.Terms.Offset)]
    [InlineData("-5", Assembler.Assembler.Terms.Offset)]
    [InlineData("-13", Assembler.Assembler.Terms.Offset)]
    [InlineData("-16", Assembler.Assembler.Terms.Offset)]
    [InlineData("-17", Assembler.Assembler.Terms.Offset, false)]
    // Address
    [InlineData("[a]", Assembler.Assembler.Terms.Address)]
    [InlineData("[c]", Assembler.Assembler.Terms.Address)]
    [InlineData("[201]", Assembler.Assembler.Terms.Address)]
    [InlineData("[D+12]", Assembler.Assembler.Terms.Address)]
    [InlineData("[D+15]", Assembler.Assembler.Terms.Address)]
    [InlineData("[D+16]", Assembler.Assembler.Terms.Address, false)]
    [InlineData("[D-0]", Assembler.Assembler.Terms.Address, false)]
    [InlineData("[D+0]", Assembler.Assembler.Terms.Address, false)]
    [InlineData("[D-1]", Assembler.Assembler.Terms.Address)]
    [InlineData("[D-9]", Assembler.Assembler.Terms.Address)]
    [InlineData("[D-16]", Assembler.Assembler.Terms.Address)]
    [InlineData("[D-17]", Assembler.Assembler.Terms.Address, false)]
    public void testMatch(string line, string pattern, bool res = true)
    {
        bool actual_res = Assembler.Assembler.match(line, pattern, true);
        Assert.Equal(actual_res, res);
    }

}