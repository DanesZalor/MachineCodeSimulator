using Xunit;
using Assembler;
public class PreprocessorDirectivesTest
{   
    
    [Fact]
    public void FullLineCheck()
    {
        string linesOfCode = "mov a, 0\n";
        linesOfCode += "mov b, 10\n\n";
        linesOfCode += "iterate: add a, 1 ; a++\n";
        linesOfCode += "mul b, 1; useless\n";
        linesOfCode += "cmp a, b\n";
        linesOfCode += "jb iterate\n";

        string actual_res = PreprocessorDirectives.translateAlias(linesOfCode);
        string expected_res = "mov a, 0\n";
        expected_res += "mov b, 10\n";
        expected_res += "inc a\n";
        expected_res += "cmp a, b\n";
        expected_res += "jc 4";

        Assert.Equal(expected_res, actual_res);
    }

    [Fact]
    public void FullLineCheck2()
    {
        string linesOfCode = 
                       "start:  mov a, 10\n";
        linesOfCode += "        mov b, 1\n";
        linesOfCode += "\n";
        linesOfCode += "loopstart:  cmp a, b\n";
        linesOfCode += "            jnbe loopend    ;end loop if a<=b\n";
        linesOfCode += "            sub a, 1\n";
        linesOfCode += "            jmp loopstart\n";
        linesOfCode += "loopend:    hlt\n";

        string actual_res = PreprocessorDirectives.translateAlias(linesOfCode);
        string expected_res = 
                        "mov a, 10\n";
        expected_res += "mov b, 1\n";
        expected_res += "cmp a, b\n";
        expected_res += "ja 12\n";
        expected_res += "dec a\n";
        expected_res += "jmp 4\n";
        expected_res += "hlt";

        //Console.WriteLine(actual_res);
        //Console.WriteLine(expected_res);
        Assert.Equal(expected_res, actual_res);
    }

}