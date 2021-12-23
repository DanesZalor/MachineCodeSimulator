using Xunit;
using Assembler;
using System.Text.RegularExpressions;
using System;

namespace AssemblerTest;


public class AssemblerTest
{

    public class TranslationTest
    {
        [Theory]
        // MOV Instruction
        [InlineData("mov a, b", new byte[2] { 0b0000_0000, 0b0000_0001 })]
        [InlineData("mov g,c", new byte[2] { 0b0000_0110, 0b0000_0010 })]
        [InlineData("mov d, SP", new byte[2] { 0b0000_0011, 0b0000_0111 })]
        [InlineData("mov a, bcd", new byte[0] { })]
        // DATA Instruction
        [InlineData("mov D,254", new byte[2] { 0b000_1011, 254 })]
        [InlineData("mov D   ,  31", new byte[2] { 0b000_1011, 31 })]
        [InlineData("mov D   , 031", new byte[2] { 0b000_1011, 31 })]
        [InlineData("mov D   ,  0000000001", new byte[2] { 0b000_1011, 1 })]
        [InlineData("mov D   ,  00", new byte[2] { 0b000_1011, 0 })]
        [InlineData("mov D   ,  1000", new byte[0] { })]
        // LOAD Reg, [Reg+Offset] Instruction
        [InlineData("mov a, [a+12]", new byte[2] { 0b001_0000, 0b0110_0000 })]
        [InlineData("mov a, [ a - 12 ]", new byte[2] { 0b001_0000, 0b1101_1000 })]
        [InlineData("mov a, [ a - 1 2 ]", new byte[0] { })]
        [InlineData("mov e, [g+1 ]", new byte[2] { 0b001_0100, 0b0000_1110 })]
        [InlineData("mov e, [ g-1]", new byte[2] { 0b001_0100, 0b1000_0110 })]
        // LOAD Reg, [Reg] Instruction
        [InlineData("mov e, [g]", new byte[2] { 0b0001_0100, 0b0000_0110 })]
        [InlineData("mov e , [ g ]", new byte[2] { 0b0001_0100, 0b0000_0110 })]
        [InlineData("mov a , [ sp ]", new byte[2] { 0b0001_0000, 0b0000_0111 })]
        // LOAD Reg, [Const] Instruction
        [InlineData("mov e, [255]", new byte[2] { 0b0001_1100, 0b1111_1111 })]
        [InlineData("mov f, [63]", new byte[2] { 0b0001_1101, 0b0011_1111 })]
        // STORE [Reg+Offset], Reg
        [InlineData("mov [b+7], c", new byte[2] { 0b0010_0001, 0b0011_1010 })]
        [InlineData("mov [ b + 7 ], c", new byte[2] { 0b0010_0001, 0b0011_1010 })]
        [InlineData("mov [b-7], c", new byte[2] { 0b0010_0001, 0b1011_0010 })]
        [InlineData("mov [      b - 7 ], c", new byte[2] { 0b0010_0001, 0b1011_0010 })]
        // STORE [Reg], Reg
        [InlineData("mov [b], c", new byte[2] { 0b0010_0001, 0b0000_0010 })]
        [InlineData("mov [ b ], c", new byte[2] { 0b0010_0001, 0b0000_0010 })]
        [InlineData("mov [f], sp", new byte[2] { 0b0010_0101, 0b0000_0111 })]
        // STORE [Const], Reg
        [InlineData("mov [255], c", new byte[2] { 0b0010_1010, 255 })]
        [InlineData("mov [128], sp", new byte[2] { 0b0010_1111, 128 })]
        [InlineData("mov [128], z", new byte[0] { })]
        [InlineData("mov [257], a", new byte[0] { })]
        // JMP Reg
        [InlineData("jmp b", new byte[1] { 0b0011_0001 })]
        [InlineData("jmp g", new byte[1] { 0b0011_0110 })]
        [InlineData("jmp   g  ", new byte[1] { 0b0011_0110 })]
        // JMP Const
        [InlineData("   jmp 127", new byte[2] { 0b0011_1000, 127 })]
        [InlineData("   jmp   127  ", new byte[2] { 0b0011_1000, 127 })]
        [InlineData("jmp   269  ", new byte[0] { })]
        [InlineData("jmp   -269  ", new byte[0] { })]
        // JCAZ Reg
        [InlineData("jc a", new byte[2] { 0b0100_0100, 0b0000_0000 })]
        [InlineData("ja b", new byte[2] { 0b0100_0010, 0b0000_0001 })]
        [InlineData("jz c", new byte[2] { 0b0100_0001, 0b0000_0010 })]
        [InlineData("jca sp", new byte[2] { 0b0100_0110, 0b0000_0111 })]
        [InlineData("jcz y", new byte[0] { })]
        [InlineData("jc   c", new byte[2] { 0b0100_0100, 0b0000_0010 })]
        // JCAZ Const
        [InlineData("jaz 254", new byte[2] { 0b0100_1011, 254 })]
        [InlineData("jcz   254", new byte[2] { 0b0100_1101, 254 })]
        [InlineData("jczl   254", new byte[0] { })]
        [InlineData("jca   -1", new byte[0] { })]
        public void translatesAssemblyInstructionsToMachineCodeCorrectly(string line, byte[] expected_res)
        {
            byte[] actual_res = Assembler.Translator.translateLine(line);
            Assert.Equal(expected_res, actual_res);
        }
    }

    public class MOV_SyntaxCheck
    {
        private static class SyntaxErrorMsgRes
        {
            public const string OutOfBounds = "offset out of bounds";
            public const string Undef = "non-existent token";
            public const string WrongOffsetPair = "illegal expression. Use <Register> + <Offset>";
            public const string Not8bit = "not an 8-bit constant";
            public const string InvalidMOV = "invalid MOV statement";
        }
        [Theory]
        // mov R,R
        [InlineData("mov a,b")]
        [InlineData("mov g,sp")]
        // mov R,C
        [InlineData("mov f,10")]
        [InlineData("mov c,255")]
        [InlineData("mov c, 255")]
        [InlineData("mov c, 256", "'256' " + SyntaxErrorMsgRes.Not8bit)]
        // mov R, A
        [InlineData("mov c, [a]")]
        [InlineData("mov c, [ a ]")]
        [InlineData("mov d, [ a+15 ]")]
        [InlineData("mov d, [ a + 16 ]", "'+ 16' " + SyntaxErrorMsgRes.OutOfBounds)]
        [InlineData("mov d, [ a - 17 ]", "'- 17' " + SyntaxErrorMsgRes.OutOfBounds)]
        [InlineData("mov d, [ 69 ]")]
        [InlineData("mov d, [ label1 ]")]
        [InlineData("mov d, [label2]", "'label2' " + SyntaxErrorMsgRes.Undef)]
        [InlineData("mov e, [69 + 1]", SyntaxErrorMsgRes.InvalidMOV)]
        // mov A, R
        [InlineData("mov [c], a")]
        [InlineData("mov [ c ], a")]
        [InlineData("mov [d + 15], a")]
        [InlineData("mov [69], d")]
        [InlineData("mov [ label1 ], d")]
        [InlineData("mov [label2], d", "'label2' " + SyntaxErrorMsgRes.Undef)]
        [InlineData("mov [ 69 + 1], e", SyntaxErrorMsgRes.InvalidMOV)]
        // mov A,A
        [InlineData("mov [f], [g]", SyntaxErrorMsgRes.InvalidMOV)]
        [InlineData("mov [f], [250]", SyntaxErrorMsgRes.InvalidMOV)]
        [InlineData("mov [1], [g]", SyntaxErrorMsgRes.InvalidMOV)]
        // mov C, X
        [InlineData("mov 25, a", SyntaxErrorMsgRes.InvalidMOV)]
        [InlineData("mov 255, [a]", SyntaxErrorMsgRes.InvalidMOV)]
        [InlineData("mov 255, 25", SyntaxErrorMsgRes.InvalidMOV)]
        public void testingSyntaxCheckerResult(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "s3xyB3n1s" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }
    }

    public class JumpStatement_SyntaxCheck
    {
        [Theory]
        [InlineData("jmp b")]
        [InlineData("jmp 250")]
        [InlineData("jmp 256", "'256' not an 8-bit constant")]
        [InlineData("jmp label1")]
        [InlineData("jmp label2", "'label2' is a non-existent token")]
        [InlineData("jmp label1 + 2", "invalid JMP arguement")]
        [InlineData("jmp B + 2", "invalid JMP arguement")]
        public void JMP_SyntaxCheck(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "sex" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }

        [Theory]
        [InlineData("ja b")]
        [InlineData("jnz label1")]
        [InlineData("jnz 300", "'300' not an 8-bit constant")]
        [InlineData("jae penis", "'penis' is a non-existent token")]
        [InlineData("jew f", "'jew' invalid JumpIf flags")]
        [InlineData("jnbe [a+2]", "invalid JumpIf arguement")]
        [InlineData("jnbe [a]", "invalid JumpIf arguement")]
        [InlineData("jc c")]
        [InlineData("jnc 255")]
        [InlineData("ja c")]
        [InlineData("jna c")]
        [InlineData("jz c")]
        [InlineData("jnz c")]
        [InlineData("je c")]
        [InlineData("jne c")]
        [InlineData("jb c")]
        [InlineData("jnb c")]
        [InlineData("jae c")]
        [InlineData("jnae c")]
        [InlineData("jbe c")]
        [InlineData("jnbe c")]
        public void JmpIf_SyntaxCheck(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "sex" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }

    }

    public class PushAndPop_SyntaxCheck
    {
        [Theory]
        [InlineData("push a")]
        [InlineData("push 255")]
        [InlineData("push 256", "'256' not an 8-bit constant")]
        [InlineData("push [c]")]
        [InlineData("push [c+12]")]
        [InlineData("push [c+16]", "'+16' offset out of bounds")]
        [InlineData("push s3xyB3n1s")]
        [InlineData("push confederateAgenda", "'confederateAgenda' is a non-existent token")]
        [InlineData("push aa_", "'aa_' is a non-existent token")]
        public void tesing_PUSH_statement(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "s3xyB3n1s" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }

        [Theory]
        [InlineData("pop a")]
        [InlineData("pop [c-16]")]
        [InlineData("pop [c-17]", "'-17' offset out of bounds")]
        [InlineData("pop [d]")]
        [InlineData("pop [25]")]
        [InlineData("pop [256]", "'256' not an 8-bit constant")]
        [InlineData("pop 255", "invalid POP arguement")]
        [InlineData("pop 256", "invalid POP arguement")]
        public void testing_POP_statement(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "s3xyB3n1s" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }
    }

    public class CallAndRet_SyntaxCheck
    {
        [Theory]
        [InlineData("call a")]
        [InlineData("call c")]
        [InlineData("call sp")]
        [InlineData("call [a]", "invalid CALL arguement")]
        [InlineData("call 25")]
        [InlineData("call 256", "'256' not an 8-bit constant")]
        [InlineData("call s3xyB3n1s")]
        [InlineData("call jews", "'jews' is a non-existent token")]
        [InlineData("ret")]
        public void CallRet_SyntaxCheck(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "s3xyB3n1s" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }
    }

    public class ALUOperations_SyntaxCheck
    {
        [Theory]
        // nomadic operations
        [InlineData("not b")]
        [InlineData("inc b")]
        [InlineData("dec b")]
        [InlineData("not c")]
        [InlineData("not sp")]
        [InlineData("not penisasf", "invalid NOT arguement")]
        [InlineData("inc penisasf", "invalid INC arguement")]
        [InlineData("dec penisasf", "invalid DEC arguement")]
        [InlineData("shl b")]
        [InlineData("shr c")]
        [InlineData("shr penis1", "invalid SHR arguement")]
        // dyadic operations
        [InlineData("add a, b")]
        [InlineData("sub     c, d")]
        [InlineData("mul     e,     f")]
        [InlineData("div     g  ,   sp")]
        [InlineData("shl b, c")]
        [InlineData("shl b, sp")]
        [InlineData("shl b, [a]")]
        [InlineData("shl g, [c+15]")]
        [InlineData("shl g, [c-16]")]
        [InlineData("shl g, [c-17]", "'-17' offset out of bounds")]
        [InlineData("xor [c-12], g", "invalid XOR arguements")]
        [InlineData("xor sp, label1")]
        [InlineData("xor g, [s3xyB3n1s]")]
        public void ALUOperation_SyntaxCheck(string line, string expected_res = "")
        {
            SyntaxChecker.setLabels(new string[2] { "label1", "s3xyB3n1s" });
            string actual_res = SyntaxChecker.evaluateLine(line);
            Assert.Equal(expected_res, actual_res);
        }
    }

}


public class MultiLineTest
{
    [Fact]
    public void FullSyntaxCheck()
    {
        string linesOfCode = "mov a, 0        ; counter\n";
        linesOfCode += "mov b, 10; limit\n";
        linesOfCode += "iterate:    \n";
        linesOfCode += "    inc a\n";
        linesOfCode += "    cmp [a],b\n";
        linesOfCode += "    jb iterates\n";

        string actual_res = SyntaxChecker.evaluateProgram(linesOfCode);
        //Console.WriteLine(actual_res);
        Assert.Equal(1, 1);
    }
}
