namespace Assembler;



public static class SyntaxChecker
{
    private static string[] removeCommentsAndConvertToStringArray(string assemblyprogram)
    {
        string[] linesOfCode = assemblyprogram.Split("\n");
        for (int i = 0; i < linesOfCode.Length; i++)
            linesOfCode[i] = linesOfCode[i].Split(";")[0];

        return linesOfCode;
    }


    public static string evaluateMOV()
    {
        return "";
    }


}