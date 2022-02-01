using Xunit;
using System;
using System.IO;
namespace AssemblerTest;

public class SyntaxCheckTest
{
    [Theory]
    [InlineData("instructionsTest/MOV_R_R")]
    [InlineData("instructionsTest/MOV_R_C")]
    [InlineData("instructionsTest/MOV_R_A")]
    [InlineData("instructionsTest/MOV_A_R")]
    public void InstructionsTest(string filename){
        string actual_res = Assembler.SyntaxChecker.evaluateProgram(Common.readFile(filename));
        Assert.Equal("", actual_res);
    }

    [Theory]
    [InlineData("correct1")]
    [InlineData("correct2")]
    [InlineData("correct3")]
    [InlineData("correct4")]
    [InlineData("correct5")]
    [InlineData("correct6")]
    [InlineData("correct7")]
    [InlineData("correct8")]
    [InlineData("correct9")]
    [InlineData("correct10")]
    [InlineData("correct11")]
    [InlineData("correct12")]
    [InlineData("wrongSyntax1", false)]
    [InlineData("wrongSyntax2", false)]
    [InlineData("wrongSyntax3", false)]
    /* reads ../../../TestCaes/filename (assuming it exists), 
        if noError==false, look for ../../../TestCases/filename_SyntaxErrors
    */ 
    public void ReadTestCaseFileAndEvaluate(string filename, bool noError = true){
        string actual_res = Assembler.SyntaxChecker.evaluateProgram(Common.readFile(filename));
        string expected_res = (noError ? "" : Common.readFile(filename+"_SyntaxErrors"));
        Assert.True(expected_res== actual_res, actual_res);
        //Assert.Equal( expected_res , actual_res);
    }

    [Theory]
    [InlineData("mov a, a")]
    [InlineData("mov a, a ; comment")]
    //[InlineData("mov a, a; comment")]
    [InlineData("mov a, [b] ; comment")]
    [InlineData("mov a, [b]; comment")]
    public void SingleLineEval(string line){
        string actual_res = Assembler.SyntaxChecker.evaluateLine(line);
        Assert.True(""==actual_res, actual_res);
    }
}