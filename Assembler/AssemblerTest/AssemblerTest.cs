using Xunit;
using Assembler;

namespace AssemblerTest;

public class AssemblerTest{

    [Theory]
    /*[InlineData("correct0", new byte[4]{
        0b1000, 5,
        0b1001, 10
    })]
    [InlineData("correct8", new byte[38]{
        0b0011_1000, 8,
        97, 110, 110, 97, 72, 0,
        0b0000_1010, 2,
        0b0000_1011, 232,
        0b0000_1000, 0,
        0b0101_0000,
        0b0011_1000, 19,
        0b0011_1000, 30,
        0b0001_0000, 0b0000_0010,
        0b0101_0000,
        0b1000_1010,
        0b1100_0000, 0b0001_1000, 0b0000_0010,
        0b100_1110, 19,
        0b0011_1000, 30,
        0b0110_0000,
        0b0010_0000, 0b0000_00011,
        0b1000_1011,
        0b1100_0000, 0b0000_0001,
        0b0100_1110, 30
    })]*/
    [InlineData("correct9", new byte[74]{
        
    })]
    public void testCompile(string filename, byte[] expected_res){
        
        byte[] actual_res = Assembler.Assembler.compile(Common.readFile(filename));


        foreach(byte b in expected_res)
        {
            System.Console.Write(System.String.Format("{0}, ", b));
        }

        System.Console.WriteLine("bruh");

        foreach(byte b in actual_res)
        {
            System.Console.Write(System.String.Format("{0}, ", b));
        }

        Assert.Equal(expected_res, actual_res);
    }
}