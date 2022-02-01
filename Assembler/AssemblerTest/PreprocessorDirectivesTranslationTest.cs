using Xunit;
namespace AssemblerTest;
public class PreprocessorDirectivesTest
{   
    [Theory]
    [InlineData("correct10")]
    [InlineData("correct11")]
    [InlineData("correct12")]
    public void readFileAndDerive(string filename){
        string actual_res = Assembler.PreprocessorDirectives.translateAlias(Common.readFile(filename));
        string expected_res = Common.readFile(filename+"_Derived");
        Assert.Equal(expected_res, actual_res);
    }
}