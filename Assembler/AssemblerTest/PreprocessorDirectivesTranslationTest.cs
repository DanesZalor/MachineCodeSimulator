using Xunit;
namespace AssemblerTest;
public class PreprocessorDirectivesTest
{   
    [Theory]
    [InlineData("correct1")]
    [InlineData("correct7")]
    [InlineData("correct8")]
    [InlineData("correct10")]
    [InlineData("correct11")]
    [InlineData("correct12")]
    [InlineData("correct13")]
    public void readFileAndDerive(string filename){
        string filecontent = Common.readFile(filename);

        string actual_res = Assembler.PreprocessorDirectives.translateAlias(filecontent);
        string expected_res = Common.readFile(filename+"_Derived");

        Assert.Equal(expected_res, actual_res);
    }
}