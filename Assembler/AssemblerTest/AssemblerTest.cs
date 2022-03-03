using Xunit;
using Assembler;

namespace AssemblerTest;

public class AssemblerTest{

    [Theory]
    [InlineData("correct0", new byte[4]{
        0b1000, 5,
        0b1001, 10
    })]

    [Theory]
    [InlineData("correct1", new byte[22]{
        0b1000, 10,//mov a,10
        0b1001, 2, //mov b,2
        0b0111_1000, 7,//call 7
        1101_0010,//hlt
        0b0111_1000, 16,//call 16
        0b0111_1000, 20,//call 20
        0b1100_0000, 0b0000_1001,///cmp a,b
        0b0100_1110, 7,//jca 7
        0b1101_0001,//ret
        0b1100_1000, 0b0001_1010, 2,//add b,2
        0b1101_0001,//ret
        0b1100_1001, 0b0000_1010, 2,//sub a,2
        0b1101_0001//ret
    })]

    [Theory]
    [InlineData("correct2", new byte[10]{
        0b1000, 3,//mov a,3
        0b1001, 10,//mov b,10
        0b0111_1000, 7,//call 7
        0b1101_0010,//hlt
        0b10001000,//inc a
        0b1100_0000, 0b0000_1001,//cmp a,b
        0b0100_1110, 7//jca 7
    })]

    [Theory]
    [InlineData("correct3", new byte[23]{
        0b1000, 8,//mov a,8
        0b1011, 0b1000,//mov d,a
        0b1001, 32,//mov b,32
        0b0111_1000, 15,//call 15
        0b1000, 0b1011,//mov a,d
        0b1010, 1,//mov c,1
        0b0111_1000, 20,//call 20
        0b1101_0010,//hlt
        0b10011000,//shl a
        0b1100_0000, 0b0000_1001,//cmp a,b
        0b0100_1110, 15,//jca 15
        0b10100000,//shr a
        0b1100_0000, 0b0000_1010,//cmp a,c
        0b0100_1110, 20//jca 20
    })]

    [Theory]
    [InlineData("correct5", new byte[20]{
        0b1000, 1,//mov a,1
        0b1001, 2,//mov b,2
        0b0011_1000, 9,//jmp 9
        0b10001000,//inc a
        0b0011_1000, 9,//jmp 9
        0b1100_0000, 0b0000_1001,//cmp a,b
        0b0100_1101, 6,//jcz 6
        0b1000, 253,//mov a,253
        0b1001, 1,//mov b,1
        0b1100_1001, 0b0000_0001,//add a,b
        0b0100_1011, 24,//jaz 24
        0b1101_0010//hlt
    })]
    
    [Theory]
    [InlineData("correct6", new byte[16]{
        0b1000, 0,//mov a,0
        0b1001, 0,//mov b,0
        0b1100_0010, 0b0000_0001,//and a,b
        0b0100_1110, 14,//jca 14
        0b10001000,//inc a
        0b1100_1001, 0b0000_1010, 3,//add b,3
        0b0011_1000, 6,//jmp 6
        0b1000, 1,//mov a,1
        0b1001, 0,//mov b,1
        0b1100_0001, 0b0000_0001,//xor a,b
        0b0100_1110, 27,//jca 27
        0b1100_0110, 0b0000_1010, 2,//mul a,2
        0b0011_1000, 22,//jmp 22
        0b1000, 14,//mov a,14
        0b1001, 20,//mov b,20
        0b1100_0011, 0b0000_0001,//or a,b
        0b1101_0010//hlt
    })]
    
    public void testCompile(string filename, byte[] expected_res){
        
        byte[] actual_res = Assembler.Assembler.compile(Common.readFile(filename));
        Assert.Equal(expected_res, actual_res);
    }


}