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
            public static string POP
            { get => String.Format("pop ({0}|{1})", SYNTAX.ARGUEMENTS.R, SYNTAX.ARGUEMENTS.A); }
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
            public static string POP
            { get => String.Format("(pop ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, SYNTAX.ARGUEMENTS.A); }
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
            string[,] ArgsLexiconTable = new string[5, 3] {
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
                            ):( match(single_arg.Split('+', StringSplitOptions.RemoveEmptyEntries)[0], VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L)?
                                String.Format("'{0}' non-existent token",single_arg.Replace("[","").Replace("]","").Trim()):
                                String.Format("'{0}' not an 8-bit constant", single_arg.Replace("[","").Replace("]","").Trim())
                            )
                        )
                    )
                },{
                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L,
                    "("+NEW_LEXICON.SYNTAX.ARGUEMENTS.C +"|"+LEXICON.SYNTAX.ARGUEMENTS.R+")" ,
                    String.Format("'{0}' is a non-existent token", single_arg)
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
        string movSyntaxVague = "mov (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R_X + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A_R + ")";
        string movSyntax = "mov (" + NEW_LEXICON.SYNTAX.ARGUEMENTS.R_X + "|" + NEW_LEXICON.SYNTAX.ARGUEMENTS.A_R + ")";
        if (!match(movline, movSyntax, true))
        {
            if (!match(movline, movSyntaxVague, true)) return "invalid MOV statement";
            else return evaluateArgs(
                movline.Substring(movline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        else return "";
    }
    private static string evaluateJMP(string jmpline)
    {
        string jmpSyntax = String.Format("(jmp ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string jmpSyntaxVague = "jmp (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L + ")";
        if (!match(jmpline, jmpSyntax, true))
        {
            if (!match(jmpline, jmpSyntaxVague, true)) return "invalid JMP arguement";
            else return evaluateArgs(
                jmpline.Substring(jmpline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }
    private static string evaluateJmpIf(string jmpline)
    {
        string[] jmpline_splitted = jmpline.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        string jcaz = jmpline_splitted[0];
        string arg = jmpline_splitted[1];
        string jcazflagSyntax = "(JN?(C|A|Z|E|B|AE|BE))";
        string jmpSyntax = String.Format("({0} ({1}|{2}))", jcazflagSyntax, LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string jmpSyntaxVague = "jn?[a-z]+ (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L + ")";
        if (!match(jmpline, jmpSyntax, true))
        {
            if (!match(jmpline, jmpSyntaxVague, true)) return "invalid JumpIf arguement";
            else if (!match(jcaz, jcazflagSyntax, true)) return "'" + jcaz + "' invalid JumpIf flags";
            else return evaluateArgs(arg);
        }
        return "";
    }
    private static string evaluatePUSH(string pushline)
    {
        string pushSyntax = String.Format("(push ({0}|{1}|{2}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.A, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string pushSyntaxVague = String.Format("push ({0}|{1}|{2}|{3})", VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R,
                                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A, VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C,
                                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L);
        if (!match(pushline, pushSyntax, true))
        {
            if (!match(pushline, pushSyntaxVague, true)) return "invalid PUSH arguement";
            else return evaluateArgs(
                pushline.Substring(pushline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }

    private static string evaluatePOP(string popline)
    {
        string popSyntax = String.Format("(pop ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.A);
        string popSyntaxVague = String.Format("pop ({0}|{1})", VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R,
                                VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A);
        if (!match(popline, popSyntax, true))
        {
            if (!match(popline, popSyntaxVague, true)) return "invalid POP arguement";
            else return evaluateArgs(
                popline.Substring(popline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }


    /// <summary> evaluates instructions' grammar. if grammatically correct, returns an empty string </summary>
    public static string evaluateLine(string line)
    {
        line = line.Split(";")[0]; // remove comments
        if (match(line, "^(mov )")) return evaluateMOV(line);
        else if (match(line, "^(jmp )")) return evaluateJMP(line);
        else if (match(line, "^(jn?[a-z]+ )")) return evaluateJmpIf(line);
        else if (match(line, "^(push )")) return evaluatePUSH(line);
        else if (match(line, "^(pop )")) return evaluatePOP(line);
        else return "unrecognzied statement";
    }
}