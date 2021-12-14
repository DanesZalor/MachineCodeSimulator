using System.Text.RegularExpressions;

namespace Assembler;

public static class Assembler
{

    public static class LEXICON
    {
        private const string SPACE = "(\\s)*";
        private static class TOKENS
        {
            public const string REGISTER = "([a-g]|sp)";
            public const string DECIMAL = "(" +
                "(1[0-9]{0,2})|" +  // 1, 10-19, 100-199
                "([1-9][0-9]?)|" +  // 1-99
                "(2[0-4][0-9])|" +  // 200 - 249
                "(25[0-5])" +       // 250-255
            ")";
            public const string OFFSET = "(" +
                "(\\+([1-9]|(1[1-5])))|" +  // +1 to +15
                "(-([1-9]|(1[1-6])))" +     // -1 to -16
            ")";
            public const string ADDRESS = "\\[(" +
                REGISTER + "|" +    // [Register]
                DECIMAL + "|" +     // [Decimal]
                "(" +               // [Register+Offset]
                    REGISTER + OFFSET +
                ")" +
            ")\\]";
            public const string CONST = DECIMAL;
        }

        public static class SYNTAX
        {
            public const string MOV = "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;
            public const string DATA = "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.CONST + SPACE;
        }

        public static class VAGUE
        {
            private const string REGISTER = "(( [a-z]+)|(,[a-z])+)";
            public const string MOV = "mov " + SPACE + REGISTER + SPACE + "," + SPACE + REGISTER + SPACE;
        }
    }


    public static bool match(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase).Success;
    }

    public static string evaluate_MOV(string line)
    {
        string error_msg = line;

        if (!match(line, LEXICON.SYNTAX.MOV))
        {
            //Match m = Regex.Match(line, LEXICON.VAGUE.MOV);
            //for (int i = 0; i < m.Length; i++) error_msg += String.Format("\"{0}\" is not a valid Register", (i == 0 ? m.Value : m.NextMatch()));
            error_msg = "wrong grammar";
        }
        return error_msg;
    }

    public static string evaluateLine(string line)
    {
        string res = "";
        if (match(line, "^mov "))// Start with "mov "
        {

        }
        return res;
    }


    public static int test()
    {
        return 1;
    }
}
