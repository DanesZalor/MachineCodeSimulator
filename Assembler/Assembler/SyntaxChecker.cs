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
            public const string LABEL = "(([a-z])((\\w)*))";
            private const string DECIMAL = "";
            public const string CONST = "([0-9]+)";
            public const string OFFSET = "([+-]" + LEXICON.SPACE + "(\\d)+)";
            public const string ANY = "(" + REGISTER + "|" + LABEL + "|" + CONST + ")";

        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public const string R = "(" + LEXICON.SPACE + TOKENS.REGISTER + LEXICON.SPACE + ")";
                public const string L = "(" + LEXICON.SPACE + TOKENS.LABEL + LEXICON.SPACE + ")";
                public const string C = "(" + LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE + ")";
                public const string A = "(" + LEXICON.SPACE + "\\[" + LEXICON.SPACE +
                    "(" +
                        R + "|" + L + "|" + C + "|" +
                        "(" +
                            R + VAGUE_LEXICON.TOKENS.OFFSET +
                        ")" +
                    ")" +
                LEXICON.SPACE + "\\]" + LEXICON.SPACE + ")";
                public const string X = "(" + R + "|" + L + "|" + C + "|" + A + ")";
                public const string R_X = "(" + R + "," + X + ")";
                public const string A_R = "(" + A + "," + R + ")";
            }
            public static string MOV
            {
                get => LEXICON.ETC.mov_starter + "(" + SYNTAX.ARGUEMENTS.R_X + "|" + SYNTAX.ARGUEMENTS.A_R + ")";
            }
            public static string JMP
            {
                get => LEXICON.ETC.jmp_starter + "(" + SYNTAX.ARGUEMENTS.R + "|" + SYNTAX.ARGUEMENTS.C + ")";
            }
        }
    }

    /// <summary> Newly made lexicon with the base lexicon and the added labels
    private static class NEW_LEXICON
    {
        public static class TOKENS
        {
            private static string EXISTING_LABELS = "()";
            public static string labels() { return EXISTING_LABELS; }
            public static void labelsClear() { EXISTING_LABELS = "()"; }
            public static void labelsAdd(string label)
            {
                if (match(label, VAGUE_LEXICON.TOKENS.LABEL, true))
                {
                    label = "(" + label + ")";
                    EXISTING_LABELS = EXISTING_LABELS.Replace(")", (EXISTING_LABELS == "()" ? "" : "|") + (label + ")"));
                }
            }
            public static string CONST { get => "(" + LEXICON.TOKENS.CONST + "|" + EXISTING_LABELS + ")"; }
            public static string ADDRESS_CONST { get => "(\\[" + LEXICON.SPACE + CONST + LEXICON.SPACE + "\\])"; }
            public static string ADDRESS
            {
                get => "(" +
                    LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET + "|" +
                    ADDRESS_CONST + "|" +
                    LEXICON.TOKENS.ADDRESS_REGISTER +
                    ")";
            }
        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public static string C { get => "(" + LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE + ")"; }
                public static string A { get => "(" + LEXICON.SPACE + TOKENS.ADDRESS + LEXICON.SPACE + ")"; }
                public static string X
                {
                    get => "(" + LEXICON.SPACE +
                            "(" + LEXICON.SYNTAX.ARGUEMENTS.R + "|" + C + "|" + A + ")" +
                        LEXICON.SPACE + ")";
                }
                public static string R_X { get => "(" + LEXICON.SYNTAX.ARGUEMENTS.R + "," + X + ")"; }
                public static string A_R { get => "(" + A + "," + LEXICON.SYNTAX.ARGUEMENTS.R + ")"; }
            }

            public static string MOV
            {
                get => LEXICON.ETC.mov_starter + "(" + SYNTAX.ARGUEMENTS.R_X + "|" + SYNTAX.ARGUEMENTS.A_R + ")";
            }
            public static string JMP
            {
                get => LEXICON.ETC.jmp_starter + "(" + LEXICON.SYNTAX.ARGUEMENTS.R + "|" + SYNTAX.ARGUEMENTS.C + ")";
            }
        }


    }

    public static void setLabels(string[] labels)
    {
        NEW_LEXICON.TOKENS.labelsClear();
        for (int i = 0; i < labels.Length; i++)
            NEW_LEXICON.TOKENS.labelsAdd(labels[i]);
    }
    private static Match getMatch(string line, string pattern, bool exact = false, bool inverse = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        if (inverse) pattern = "\\[^" + pattern + "\\]";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }
    private static bool match(string line, string pattern, bool exact = false) { return getMatch(line, pattern, exact).Success; }

    /// <summary> find out whats wrong with the arguement(s) </summary> 
    private static string evaluateArgs(string argsline)
    {
        string evaluation_result = "";

        string single_evaluation(string single_arg)
        {
            //single_arg = single_arg.Replace("[", "").Replace("]", "").Trim(); // if an address, break it down yo
            /*each array contains {VagueGrammar, CorrectGrammar, ErrorMsg}
                We will loop thru the array, check if the \"single_arg\" grammatically matches VagueGrammar,
                then check if grammatically matches CorrectGrammar: if it doesn't return the ErrorMsg
            */
            string[,] ArgsLexiconTable = new string[4, 3] {
                {
                    LEXICON.RESERVED_WORDS, "(\\s){1000}", "a reserved word"
                },{
                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A,NEW_LEXICON.SYNTAX.ARGUEMENTS.A,(
                        (
                            match(single_arg,VAGUE_LEXICON.TOKENS.OFFSET)?( // is there an offset
                                !match(getMatch(single_arg, VAGUE_LEXICON.TOKENS.OFFSET).Value, LEXICON.TOKENS.OFFSET, true)?( // is it not a legit offset
                                    (
                                        String.Format("'{0}' offset out of bounds",getMatch(single_arg, VAGUE_LEXICON.TOKENS.OFFSET).Value)
                                    )
                                ):("")
                            ):(String.Format("'{0}' non-existent token",single_arg.Replace("[","").Replace("]","").Trim() ))
                        )
                    )
                },{
                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C, LEXICON.SYNTAX.ARGUEMENTS.C,
                    String.Format("'{0}' not an 8-bit constant", single_arg)
                },
                { ".*", LEXICON.TOKENS.ANY,String.Format("'{0}' invalid expression",single_arg)}
            };
            for (int j = 0; j < 4; j++)
            {
                if (match(single_arg, ArgsLexiconTable[j, 0], true))
                {
                    if (!match(single_arg, ArgsLexiconTable[j, 1], true))
                        return ArgsLexiconTable[j, 2];
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
        if (!match(movline, NEW_LEXICON.SYNTAX.MOV, true))
        {
            if (!match(movline, VAGUE_LEXICON.SYNTAX.MOV, true)) return "invalid MOV statement";
            else return evaluateArgs(
                movline.Substring(getMatch(movline, LEXICON.ETC.mov_starter).Value.Length)
            );
        }
        else return "";
    }

    private static string evaluateJMP(string jmpline)
    {
        //string arg = ; // get the arg
        if (!match(jmpline, NEW_LEXICON.SYNTAX.JMP, true))
        {
            if (!match(jmpline, VAGUE_LEXICON.SYNTAX.JMP)) return "invalid JMP statement";
            else return evaluateArgs(
                jmpline.Substring(getMatch(jmpline, LEXICON.ETC.jmp_starter).Value.Length)
            );
        }
        return "";
    }

    /// <summary> evaluates instructions' grammar. if grammatically correct, returns an empty string </summary>
    public static string evaluateLine(string line)
    {
        line = line.Split(";")[0]; // remove comments
        if (match(line, LEXICON.ETC.mov_starter)) return evaluateMOV(line);
        else return "unrecognzied statement";
    }
}