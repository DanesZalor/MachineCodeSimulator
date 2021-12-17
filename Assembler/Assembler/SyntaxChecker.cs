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
            private static string EXISTING_LABELS = "()";
            private const string DECIMAL = "";
            public const string CONST = "[0-9]+";
            public const string OFFSET = "([+-]" + LEXICON.SPACE + "(\\d)+)";
            private const string ADDRESS_REGISTER = "(\\[" + LEXICON.SPACE + REGISTER + LEXICON.SPACE + "\\])";
            private const string ADDRESS_CONST = "(\\[" + LEXICON.SPACE + CONST + LEXICON.SPACE + "\\])";
            private static string ADDRESS_LABEL
            {
                get => "(\\[" + LEXICON.SPACE + EXISTING_LABELS + LEXICON.SPACE + "\\])";
            }
            private const string ADDRESS_REGISTER_OFFSET = "(\\[" + LEXICON.SPACE + REGISTER + LEXICON.SPACE + OFFSET + LEXICON.SPACE + "\\])";
            public static string ADDRESS
            {
                get => String.Format("({0}|{1}|{2}|{3})", ADDRESS_REGISTER_OFFSET, ADDRESS_REGISTER, ADDRESS_CONST, ADDRESS_LABEL);
            }
            public static string ANY { get => String.Format("({0}|{1}|{2})", REGISTER, CONST, ADDRESS); }

            public static string labels() { return EXISTING_LABELS; }
            public static void labelsClear() { EXISTING_LABELS = "()"; }
            public static void labelsAdd(string label)
            {
                if (match(label, "([a-z](\\w)+)", true))
                    EXISTING_LABELS = EXISTING_LABELS.Replace(")", (EXISTING_LABELS == "()" ? "" : "|") + (label + ")"));
            }

        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public const string R = "(" + LEXICON.SPACE + TOKENS.REGISTER + LEXICON.SPACE + ")";
                public const string L = "(" + LEXICON.SPACE + TOKENS.LABEL + LEXICON.SPACE + ")";
                public const string C = "(" + LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE + ")";
                public const string WithOFFSET = "(" + R + TOKENS.OFFSET + LEXICON.SPACE + ")";
                public static string A { get => "(" + LEXICON.SPACE + TOKENS.ADDRESS + LEXICON.SPACE + ")"; }
                public static string X { get => "(" + LEXICON.SPACE + TOKENS.ANY + LEXICON.SPACE + ")"; }
                public static string R_X { get => String.Format("({0},{1})", R, X); }
                public static string A_R { get => String.Format("({0},{1})", A, R); }
            }
            public static string MOV
            {
                get => LEXICON.ETC.mov_starter + "(" + SYNTAX.ARGUEMENTS.R_X + "|" + SYNTAX.ARGUEMENTS.A_R + ")";
            }



        }
    }

    public static void setLabels(string[] labels)
    {
        VAGUE_LEXICON.TOKENS.labelsClear();
        for (int i = 0; i < labels.Length; i++)
            VAGUE_LEXICON.TOKENS.labelsAdd(labels[i]);
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
                    "("+LEXICON.SYNTAX.ARGUEMENTS.R +"|"+ VAGUE_LEXICON.TOKENS.labels()+")",
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