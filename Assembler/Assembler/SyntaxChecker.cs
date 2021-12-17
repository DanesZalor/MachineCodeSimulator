using System.Text.RegularExpressions;
namespace Assembler;



public static class SyntaxChecker
{
    /// <summary> Lexicon containing somewhat correct grammar that can be recognized for evaluation </summary>
    private static class VAGUE_LEXICON
    {
        public static class TOKENS
        {
            public const string REGISTER = "([a-z]+)";
            public const string LABEL = "(([a-z])((\\w)+))";
            //public const string LABEL_OR_REGISTER = "(" + REGISTER + "|" + LABEL + ")";
            public static string EXISTING_LABELS = "()";
            private const string DECIMAL = "";
            public const string CONST = "[0-9]+";
            public const string OFFSET = "([+-]" + LEXICON.SPACE + "(\\d)+)";
            public const string ADDRESS_REGISTER = "(\\[" + LEXICON.SPACE + REGISTER + LEXICON.SPACE + "\\])";
            public const string ADDRESS_CONST = "(\\[" + LEXICON.SPACE + CONST + LEXICON.SPACE + "\\])";
            public const string ADDRESS_REGISTER_OFFSET = "(\\[" + LEXICON.SPACE + REGISTER + LEXICON.SPACE + OFFSET + LEXICON.SPACE + "\\])";
            public const string ADDRESS = "(" +
                ADDRESS_REGISTER_OFFSET + "|" +
                ADDRESS_CONST + "|" +
                ADDRESS_REGISTER +
            ")";
            public const string ANY = "(" +
            REGISTER + "|" +
            CONST + "|" +
            ADDRESS + "|" +
        ")";
        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public const string R = "(" + LEXICON.SPACE + TOKENS.REGISTER + LEXICON.SPACE + ")";
                public const string L = "(" + LEXICON.SPACE + TOKENS.LABEL + LEXICON.SPACE + ")";
                public const string C = "(" + LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE + ")";
                public const string WithOFFSET = "(" + R + TOKENS.OFFSET + LEXICON.SPACE + ")";
                public const string A = "(" + LEXICON.SPACE + TOKENS.ADDRESS + LEXICON.SPACE + ")";
                public const string X = "(" + LEXICON.SPACE + TOKENS.ANY + LEXICON.SPACE + ")";
                public const string R_X = "(" + R + "," + X + ")";
                public const string A_R = "(" + A + "," + R + ")";
            }
            public const string MOV = LEXICON.ETC.mov_starter + "(" + SYNTAX.ARGUEMENTS.R_X + "|" + SYNTAX.ARGUEMENTS.A_R + ")";



        }
    }


    public static void labelsClear() { VAGUE_LEXICON.TOKENS.EXISTING_LABELS = "()"; }
    public static void labelsAdd(string label)
    {
        if (match(label, "([a-z](\\w)+)", true))
            VAGUE_LEXICON.TOKENS.EXISTING_LABELS = VAGUE_LEXICON.TOKENS.EXISTING_LABELS.Replace(")", (VAGUE_LEXICON.TOKENS.EXISTING_LABELS == "()" ? "" : "|") + (label + ")"));
    }
    private static Match getMatch(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }
    private static bool match(string line, string pattern, bool exact = false) { return getMatch(line, pattern, exact).Success; }

    /// <summary> find out whats wrong with the arguements </summary> 
    private static string evaluateArgs(string argsline)
    {
        string evaluation_result = "";

        string single_evaluation(string single_arg)
        {
            single_arg = single_arg.Replace("[", "").Replace("]", "").Trim(); // if an address, break it down yo
            if (match(single_arg, VAGUE_LEXICON.SYNTAX.ARGUEMENTS.WithOFFSET, true)) // if an arguement with an offset, evaluate both R/L and Offset
            {
                char splitter = (single_arg.Contains('+') ? '+' : '-');
                string[] RandOffset = single_arg.Split(splitter, StringSplitOptions.TrimEntries);
                return evaluateArgs(RandOffset[0] + "," + splitter + RandOffset[1]);
            }
            string[,] ArgsLexiconTable = new string[4, 3] { //each array contains {VagueGrammar, CorrectGrammar, errorMsg}
                {VAGUE_LEXICON.TOKENS.OFFSET, LEXICON.TOKENS.OFFSET, "an offset out of bounds (-16+15)"},
                {   VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L,
                    "("+LEXICON.SYNTAX.ARGUEMENTS.R +"|"+ VAGUE_LEXICON.TOKENS.EXISTING_LABELS+")",
                    (match(single_arg, VAGUE_LEXICON.TOKENS.REGISTER)?"neither an addressible label or register":"an unrecognized label")
                },
                {VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C, LEXICON.SYNTAX.ARGUEMENTS.C, "not an 8-bit constant"},
                {".*", LEXICON.TOKENS.ANY,"an unrecognized token"}
            };
            for (int j = 0; j < 3; j++)
            {
                if (match(single_arg, ArgsLexiconTable[j, 0], true))
                {
                    if (!match(single_arg, ArgsLexiconTable[j, 1], true))
                        return String.Format("'{0}' is {1}", single_arg, ArgsLexiconTable[j, 2]);
                    return "";
                }
            }
            return "";
        }

        string[] args = argsline.Split(",");
        for (int i = 0; i < args.Length; i++)
        {
            string s = single_evaluation(args[i].Trim());
            if (s != "")
                evaluation_result += (evaluation_result != "" ? "\n" : "") + s;
        }
        return evaluation_result;
    }
    private static string evaluateMOV(string movline)
    {
        string evaluation = "";
        string movline_args = movline.Substring(getMatch(movline, LEXICON.ETC.mov_starter).Value.Length); // get the args line
        Console.WriteLine(movline + " " + match(movline, VAGUE_LEXICON.SYNTAX.MOV, true));
        if (!match(movline, LEXICON.SYNTAX.MOV, true))
        {
            evaluation = evaluateArgs(movline_args);
            if (evaluation == "" && !match(movline, VAGUE_LEXICON.SYNTAX.MOV)) evaluation = "invalid MOV operands";

            //evaluation = evaluateArgs(movline_args);
        }
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