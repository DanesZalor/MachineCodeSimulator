using System.Text.RegularExpressions;
namespace Assembler;

public static class VAGUE_LEXICON
{
    public static class TOKENS
    {
        public const string REGISTER = "[a-z]+";
    }
    public static class SYNTAX
    {

    }
}

public static class SyntaxChecker
{
    private static Match getMatch(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }
    private static bool match(string line, string pattern, bool exact = false)
    {
        return getMatch(line, pattern, exact).Success;
    }
    private static string[] removeCommentsAndConvertToStringArray(string assemblyprogram)
    {
        string[] linesOfCode = assemblyprogram.Split("\n");
        for (int i = 0; i < linesOfCode.Length; i++)
            linesOfCode[i] = linesOfCode[i].Split(";")[0];
        return linesOfCode;
    }



    private static string evaluateMOV(string movline)
    {
        if (match(movline, LEXICON.SYNTAX.MOV, true)) return ""; // no errors

        else return "Invalid MOV operands";
    }

    /// <summary> evaluates instructions' grammar. if grammatically correct, returns an empty string </summary>
    public static string evaluateLine(string line)
    {
        if (match(line, LEXICON.ETC.mov_starter)) return evaluateMOV(line);
        return "";
    }


}