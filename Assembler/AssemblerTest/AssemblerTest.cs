using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{
    [Theory]
    [InlineData("mov a, sp", Assembler.Assembler.LEXICON.SYNTAX.MOV)]
    [InlineData("mov G,  B", Assembler.Assembler.LEXICON.SYNTAX.MOV)]
    [InlineData("mov C,D", Assembler.Assembler.LEXICON.SYNTAX.MOV)]
    [InlineData("mov     E     ,      F     ", Assembler.Assembler.LEXICON.SYNTAX.MOV)]
    [InlineData("movE,F", Assembler.Assembler.LEXICON.SYNTAX.MOV, false)]
    public void testMatch(string line, string pattern, bool res = true)
    {
        bool actual_res = Assembler.Assembler.match(line, pattern, true);
        Assert.Equal(actual_res, res);
    }

    [Fact]
    public void testMov()
    {
        string s = "";
        s = Assembler.Assembler.evaluate_MOV("mov a, b"); Console.WriteLine(s);
        s = Assembler.Assembler.evaluate_MOV("mov a, asasf"); Console.WriteLine(s);

    }

}