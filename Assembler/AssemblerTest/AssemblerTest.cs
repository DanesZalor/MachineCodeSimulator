using Xunit;
using Assembler;

namespace AssemblerTest;

public class AssemblerTest{

    [Theory]
    [InlineData("correct0", new byte[4]{
        0b1000, 5,
        0b1001, 10
    })]
    public void testCompile(string filename, byte[] expected_res){
        
        byte[] actual_res = Assembler.Assembler.compile(Common.readFile(filename));
        Assert.Equal(expected_res, actual_res);
    }

}