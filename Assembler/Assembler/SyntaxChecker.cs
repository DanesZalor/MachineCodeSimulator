using System.Text.RegularExpressions;
namespace Assembler;



public static class SyntaxChecker
{
    /// <summary> Lexicon containing somewhat correct grammar that can be recognized for evaluation </summary>
    private static class VAGUE_LEXICON
    {
        public static class TOKENS
        {
            public const string REGISTER = "[a-z]+";
            private const string DECIMAL = "";
            public const string CONST = "[0-9]+";
        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public const string R = LEXICON.SPACE + TOKENS.REGISTER + LEXICON.SPACE;
                public const string C = LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE;

                public const string R_R = R + "," + R;
                public const string R_C = R + "," + C;
            }
            public const string MOV = LEXICON.SPACE + "mov " + ARGUEMENTS.R_R;
            public const string DATA = LEXICON.SPACE + "mov " + ARGUEMENTS.R_C;

        }
    }
    private static Match getMatch(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }
    private static bool match(string line, string pattern, bool exact = false)
    {
        return getMatch(line, pattern, exact).Success;
    }

    /// <summary> find out whats wrong with the arguements </summary> 
    private static string evaluateArgs(string argsline)
    {
        string evaluation_result = "";

        string single_evaluation(string single_arg)
        {
            string[,] ArgsLexiconTable = new string[3, 3] {
                {VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SYNTAX.ARGUEMENTS.R, "not a valid register"},
                {VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C, LEXICON.SYNTAX.ARGUEMENTS.C, "not an 8-bit constant"},
                {"(.)*", LEXICON.TOKENS.ANY," an unrecognized token"}
            };
            for (int j = 0; j < ArgsLexiconTable.GetLength(0); j++)
            {
                if (match(single_arg, ArgsLexiconTable[j, 0], true))
                {
                    if (!match(single_arg, ArgsLexiconTable[j, 1], true))
                        return String.Format("'{0}' is {1}", single_arg, ArgsLexiconTable[j, 2]);
                }
            }
            return "";
        }

        string[] args = argsline.Split(",", StringSplitOptions.TrimEntries);

        for (int i = 0; i < args.Length; i++)
        {
            string s = single_evaluation(args[i]);
            if (s != "")
                evaluation_result += (evaluation_result != "" ? "\n" : "") + single_evaluation(args[i]);
        }
        return evaluation_result;
    }
    private static string evaluateMOV(string movline)
    {
        string evaluation = "";
        string movline_args = movline.Substring(getMatch(movline, LEXICON.ETC.mov_starter).Value.Length); // get the args line

        if (!match(movline, LEXICON.SYNTAX.MOV, true))
            evaluation = evaluateArgs(movline_args);

        return evaluation;
    }

    /// <summary> evaluates instructions' grammar. if grammatically correct, returns an empty string </summary>
    public static string evaluateLine(string line)
    {
        line = line.Split(";")[0]; // remove comments
        if (match(line, LEXICON.ETC.mov_starter)) return evaluateMOV(line);
        return "";
    }


}