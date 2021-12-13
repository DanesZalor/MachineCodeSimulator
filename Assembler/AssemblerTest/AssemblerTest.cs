using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class UnitTest1
{
    [Fact]
    public void Testing_test()
    {
        bool b = Assembler.Assembler.match("[a+12]", Assembler.Assembler.Terms.Address);
        Console.WriteLine(b);
    }

}