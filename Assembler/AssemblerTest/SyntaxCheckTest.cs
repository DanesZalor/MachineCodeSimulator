using Xunit;
using System;
using System.IO;
using Assembler;
namespace AssemblerTest;

public class SyntaxCheckTest
{
    [Theory]
    [InlineData("instructionsTest/all_MOV_R_R")]
    public void InstructionsTest(string filename){
        string actual_res = SyntaxChecker.evaluateProgram(readFile(filename));
        Assert.Equal("", actual_res);
    }

    [Theory]
    [InlineData("freg_1")]
    [InlineData("freg_2")]
    [InlineData("freg_3")]
    [InlineData("dolor_1", false)]
    [InlineData("dolor_2", false)]
    [InlineData("jojo_1")]
    [InlineData("jojo_2")]
    [InlineData("jojo_3")]
    [InlineData("kokong_1")]
    [InlineData("kokong_2")]
    [InlineData("kokong_3")]
    [InlineData("kokong_4", false)]
    /* reads ../../../TestCaes/filename (assuming it exists), 
        if noError==false, look for ../../../TestCases/filename_SyntaxErrors
    */ 
    public void ReadTestCaseFileAndEvaluate(string filename, bool noError = true){
        string actual_res = SyntaxChecker.evaluateProgram(readFile(filename));
        string expected_res = (noError ? "" : readFile(filename+"_SyntaxErrors"));
        //Assert.True(expected_res== actual_res, actual_res);
        Assert.Equal( expected_res , actual_res);
    }

    /// <summary> reads a file assuming ../../../TestCases/filename exists </summary>
    private static string readFile(string filename){
        string path = Path.Combine(
            System.IO.Directory.GetCurrentDirectory(), 
            ".." + Path.DirectorySeparatorChar + 
            ".." + Path.DirectorySeparatorChar + 
            ".." + Path.DirectorySeparatorChar + 
            "TestCases/" + filename
        );
        return new string(System.IO.File.ReadAllText(path));
    }
}