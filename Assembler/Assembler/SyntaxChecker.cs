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
        public static class ARGUEMENTS
        {
            public const string R = LEXICON.SPACE + TOKENS.REGISTER + LEXICON.SPACE;
            public const string R_R = R + "," + R;
        }
        public const string MOV_R_R = LEXICON.SPACE + "mov " + ARGUEMENTS.R_R;
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
        string evaluation = "";
        if (match(movline, LEXICON.SYNTAX.MOV, true)) evaluation = ""; // no errors
        else if (match(movline, VAGUE_LEXICON.SYNTAX.MOV_R_R)) // MOV R R
        {
            Match m = getMatch(movline, VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R).NextMatch();
            string[] r = new string[2] { m.Value.Trim(), m.NextMatch().Value.Trim() }; // get the 2 registers substring trimmed
            bool[] validR = new bool[2] {
                match(r[0], LEXICON.SYNTAX.ARGUEMENTS.R, true),
                match(r[1], LEXICON.SYNTAX.ARGUEMENTS.R, true)
            };
            if (!validR[0] && !validR[1]) evaluation = String.Format("'{0}' and '{1}' are not valid registers", r[0], r[1]);
            else if (!validR[0]) evaluation = String.Format("'{0}' is not a valid register", r[0]);
            else if (!validR[1]) evaluation = String.Format("'{0}' is not a valid register", r[1]);
        }
        else evaluation = "Invalid MOV operands";
        return evaluation;
    }

    /// <summary> evaluates instructions' grammar. if grammatically correct, returns an empty string </summary>
    public static string evaluateLine(string line)
    {
        line = line.Split(";")[0];
        if (match(line, LEXICON.ETC.mov_starter)) return evaluateMOV(line);
        return "";
    }


}