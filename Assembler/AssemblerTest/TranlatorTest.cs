using Xunit;
using Assembler;
using System;

namespace AssemblerTest;

/** Contains no failing test cases as by the time the assembly process 
* reaches translation phase,it is assumed that the program is already
* syntactically correct.
*/
public class TranslationCheck
{
    /*
    public class SingleLineEval{
        [Theory]
        // MOV Instruction
        [InlineData("mov a,b",new byte[2] { 0b0000_0000,0b0000_0001 })]
        [InlineData("mov g,c",new byte[2] { 0b0000_0110,0b0000_0010 })]
        [InlineData("mov d,sp",new byte[2] { 0b0000_0011,0b0000_0111 })]
        // DATA Instruction
        [InlineData("mov d,254",new byte[2] { 0b000_1011,254 })]
        [InlineData("mov d,31",new byte[2] { 0b000_1011,31 })]
        [InlineData("mov d,1",new byte[2] { 0b000_1011,1 })]
        [InlineData("mov d,0",new byte[2] { 0b000_1011,0 })]
        // LOAD Reg,[Reg+Offset] Instruction
        [InlineData("mov a,[a+12]",new byte[2] { 0b001_0000,0b0110_0000 })]
        [InlineData("mov a,[sp-12]",new byte[2] { 0b001_0000,0b1101_1111 })]
        [InlineData("mov e,[g+1]",new byte[2] { 0b001_0100,0b0000_1110 })]
        [InlineData("mov e,[g-1]",new byte[2] { 0b001_0100,0b1000_0110 })]
        // LOAD Reg,[Reg] Instruction
        
        [InlineData("mov e,[g]",new byte[2] { 0b0001_0100,0b0000_0110 })]
        [InlineData("mov e,[a]",new byte[2] { 0b0001_0100,0b0000_0000 })]
        [InlineData("mov a,[sp]",new byte[2] { 0b0001_0000,0b0000_0111 })]
        // LOAD Reg,[Const] Instruction
        [InlineData("mov e,[255]",new byte[2] { 0b0001_1100,0b1111_1111 })]
        [InlineData("mov f,[63]",new byte[2] { 0b0001_1101,0b0011_1111 })]
        // STORE [Reg+Offset],Reg
        [InlineData("mov [b+7],c",new byte[2] { 0b0010_0010,0b0011_1001 })]
        [InlineData("mov [g-4],c",new byte[2] { 0b0010_0010,0b1001_1110 })]
        [InlineData("mov [g],c",new byte[2] { 0b0010_0010,0b0000_0110 })]
        [InlineData("mov [sp-16],f",new byte[2] { 0b0010_0101,0b1111_1111 })]
        // STORE [Reg],Reg
        [InlineData("mov [b],c",new byte[2] { 0b0010_0010,0b0000_0001 })]
        [InlineData("mov [f],sp",new byte[2] { 0b0010_0111,0b0000_0101 })]
        [InlineData("mov [c+2],sp",new byte[2] { 0b0010_0111,0b0001_0010 })]
        // STORE [Const],Reg
        [InlineData("mov [255],c",new byte[2] { 0b0010_1010,255 })]
        [InlineData("mov [128],sp",new byte[2] { 0b0010_1111,128 })]
        
        
        // JMP Reg
        [InlineData("jmp b",new byte[1] { 0b0011_0001 })]
        [InlineData("jmp g",new byte[1] { 0b0011_0110 })]
        // JMP Const
        [InlineData("jmp 127",new byte[2] { 0b0011_1000,127 })]
        [InlineData("jmp 52",new byte[2] { 0b0011_1000,52 })]
        // JCAZ Reg
        [InlineData("jc a",new byte[2] { 0b0100_0100,0b0000_0000 })]
        [InlineData("ja b",new byte[2] { 0b0100_0010,0b0000_0001 })]
        [InlineData("jz c",new byte[2] { 0b0100_0001,0b0000_0010 })]
        [InlineData("jca sp",new byte[2] { 0b0100_0110,0b0000_0111 })]
        // JCAZ Const
        [InlineData("jaz 254",new byte[2] { 0b0100_1011,254 })]
        [InlineData("jaz 101",new byte[2] { 0b0100_1011,101 })]
        // PUSH 
        [InlineData("push b",new byte[1] { 0b0101_0001 })]
        [InlineData("push g",new byte[1] { 0b0101_0110 })]
        [InlineData("push 125",new byte[2] { 0b0101_1010,125 })]
        [InlineData("push 205",new byte[2] { 0b0101_1010,205 })]
        [InlineData("push [g+12]",new byte[2] { 0b0101_1000,0b0110_0110 })]
        [InlineData("push [c-16]",new byte[2] { 0b0101_1000,0b1111_1010 })]
        [InlineData("push [sp]",new byte[2] { 0b0101_1000,0b0000_0111 })]
        [InlineData("push [200]",new byte[2] { 0b0101_1001, 200 })]
        // POP
        [InlineData("pop c", new byte[1]{ 0b0110_0010 })]
        [InlineData("pop d", new byte[1]{ 0b0110_0011 })]
        [InlineData("pop [b]", new byte[2]{ 0b0110_1000, 0b0000_0001 })]
        [InlineData("pop [b-12]", new byte[2]{ 0b0110_1000, 0b1101_1001 })]
        [InlineData("pop [f+12]", new byte[2]{ 0b0110_1000, 0b0110_0101 })]
        // CALL
        [InlineData("call e", new byte[1]{ 0b0111_0100})]
        [InlineData("call f", new byte[1]{ 0b0111_0101})]
        [InlineData("call 21", new byte[2]{ 0b0111_1000, 21})]
        //ALU1
        [InlineData("not c", new byte[1]{ 0b1000_0010 })]
        [InlineData("inc d", new byte[1]{ 0b1000_1011 })]
        [InlineData("dec e", new byte[1]{ 0b1001_0100 })]
        [InlineData("shl f", new byte[1]{ 0b1001_1101 })]
        [InlineData("shr g", new byte[1]{ 0b1010_0110 })]
        //ALU2
        [InlineData("add a,c", new byte[2]{ 0b1100_1000, 0b0000_0010 })]
        [InlineData("sub d,sp", new byte[2]{ 0b1100_1001, 0b0011_0111 })]
        [InlineData("xor b,[f+1]", new byte[3]{ 0b1100_0001, 0b0001_1000, 0b0000_1101 })]
        [InlineData("and g,[a+15]", new byte[3]{ 0b1100_0010, 0b0110_1000, 0b0111_1000 })]
        [InlineData("or g,[a-15]", new byte[3]{ 0b1100_0011, 0b0110_1000, 0b1111_0000 })]
        [InlineData("shl g,[a]", new byte[3]{ 0b1100_0100, 0b0110_1000, 0b0000_0000 })]
        [InlineData("shr b,[151]", new byte[3]{ 0b1100_0101, 0b0001_1001, 151 })]
        [InlineData("mul c,[12]", new byte[3]{ 0b1100_0110, 0b0010_1001, 12 })]
        [InlineData("div d,123", new byte[3]{ 0b1100_0111, 0b0011_1010, 123 })]
        [InlineData("add e,0", new byte[3]{ 0b1100_1000, 0b0100_1010, 0 })]
        //etc
        [InlineData("clf", new byte[1]{0b1101_0000} )]
        [InlineData("ret", new byte[1]{0b1101_0001} )]
        [InlineData("hlt", new byte[1]{0b1101_0010} )]
        public void translatesAssemblyInstructionsToMachineCodeCorrectly(string line,byte[] expected_res)
        {
            byte[] actual_res = Assembler.Translator.translateLine(line);
            Assert.Equal(expected_res,actual_res);
            
        }
    }*/


    [Theory]
    [InlineData("BinTest/test1", new byte[5]{
        0b1000, 0b1010, 
        0b1001, 0b10, 
        0b1101_0000
    })]
    [InlineData("BinTest/jo1", new byte[12]{
        0b1000, 1, 
        0b1001, 2, 
        0b1100_0000, 0b0000_0001,
        0b0100_1101, 9, 
        0b1101_0010, 
        0b10001_000,
        0b0011_1000, 4
    })]
    [InlineData("BinTest/jo2", new byte[22]{
        0b1000, 1, 
        0b1001, 2, 
        0b0011_1000, 9, 
        0b10001_000, 
        0b0011_1000, 9, 
        0b1100_0000, 0b0000_0001, 
        0b0100_1101, 6, 
        0b1000, 253, 
        0b1001, 1, 
        0b1100_1000, 0b0000_0001, 
        0b0100_1011, 24, 
        0b1101_0010
    })]
    [InlineData("BinTest/jo3", new byte[27]{
        0b1000, 0, 
        0b1001, 0, 
        0b1100_0010, 0b0000_0001, 
        0b0100_1110, 14, 
        0b10001_000, 
        0b1100_1000, 0b0001_1010, 3, 
        0b0011_1000, 4, 
        0b1000, 1, 
        0b1001, 1, 
        0b1100_0001, 0b0000_0001, 
        0b1000, 14, 
        0b1001, 20, 
        0b1100_0011, 0b0000_0001, 
        0b1101_0010
    })]
  
    [InlineData("BinTest/frc1", new byte[14]{
        0b1000, 2,
        0b1001, 7,
        0b0010, 0b0000,
        0b1001_0001,
        0b1100_0110, 0b0000_0010,
        0b1001_0001,
        0b1100_0000, 0b0001_0011,
        0b0100_1110, 7
    })]
    [InlineData("BinTest/frc2", new byte[38]{
        0b0011_1000, 8,
        97,110,110,97,72,0,
        0b1010, 2,
        0b1011, 232,
        0b1000, 0,
        0b0101_0000,
        0b0011_1000, 19,
        0b0011_1000, 30,
        0b1_0000, 0b0000_0010, 
        0b0101_0000, 
        0b1000_1010, 
        0b1100_0000, 0b0001_1000, 0b0000_0010,
        0b0100_1110, 19,
        0b0011_1000, 30,
        0b0110_0000,
        0b0010_0000, 0b0000_0011,
        0b1000_1011, 
        0b1100_0000, 0b0000_0001,
        0b0100_1110, 30
    })]
    [InlineData("BinTest/frc3", new byte[]{
        0b0000_1001, 5,
        0b0000_1011, 232,
        0b1001_0001,
        0b1000_1010,
        0b1100_1000, 0b0000_1010, 2,
        0b0010_0000, 0b0000_0011,
        0b1000_1011,
        0b1100_0000, 0b0001_0010,
        0b0100_1110, 7
    })]
    
    [InlineData("BinTest/sulay1", new byte[24]{
        0b1000, 10,
        0b1001, 2,
        0b111_1000, 7, 
        0b1101_0010, 
        0b111_1000, 16,
        0b111_1000, 20, 
        0b1100_0000, 0b0000_0001,
        0b100_1110, 7, 
        0b1101_0001, 
        0b1100_1000, 0b0001_1010, 2, 
        0b1101_0001, 
        0b1100_1001, 0b0000_1010, 2, 
        0b1101_0001
    })]
    [InlineData("BinTest/sulay2", new byte[25]{
        0b1000, 8,
        0b0011, 0b0000, 
        0b1001, 32, 
        0b111_1000, 15, 
        0b0, 0b11, 
        0b1010, 1, 
        0b0111_1000, 20,
        0b1101_0010, 
        0b1001_1000, 
        0b1100_0000, 0b1, 
        0b100_1110, 15, 
        0b1010_0000,  
        0b1100_0000, 0b0000_0010, 
        0b100_1110, 20
    })]
    //[InlineData("BinTest/sulay3", new byte[11]{0b1000, 0b11, 0b1001, 0b1010, 0b111_1000, 0b0111, 0b1101_0010, 0b10001, 0b000, 0b1100_0000, 0b0000_0001, 0b10110})]

    public void TestCompileFile(string filename, byte[] expected_res){
        void assertEqual(byte[] a, byte[] b){
            bool equalLength = (a.Length==b.Length);
            if(!equalLength){
                Console.Write("a.Length="+ Convert.ToString(a.Length));
                Console.WriteLine("   b.Length="+ Convert.ToString(b.Length));
                //Assert.True(false);
            }

            bool arrayEqual = equalLength;
            for(int i = 0; i<a.Length && arrayEqual; i++){
                if(a[i]!=b[i]) arrayEqual = false;
            }
            if(!arrayEqual){
                Console.Write("a = ["); for(int i = 0; i < a.Length; i++) Console.Write(a[i] + ","); Console.WriteLine(" size:" +Convert.ToString(a.Length));
                Console.Write("b = ["); for(int i = 0; i < b.Length; i++) Console.Write(b[i] + ","); Console.WriteLine(" size:" +Convert.ToString(b.Length));
                Assert.True(false);
            }
        }

        byte[] actual_res = Assembler.Translator.translateProgram(Common.readFile(filename));
        assertEqual(expected_res, actual_res);
    }
    
}
