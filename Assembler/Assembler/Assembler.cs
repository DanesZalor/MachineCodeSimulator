using System.Text.RegularExpressions;

namespace Assembler;

public static class Assembler
{

    private static class LEXICON
    {
        public const string SPACE = "(\\s)*";
        public static class TOKENS
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
            public const string LOAD = "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.ADDRESS + SPACE;
            public const string STORE = "mov " + SPACE + TOKENS.ADDRESS + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;
        }

        public static class VAGUE
        {
            public const string REGISTER = "(( [a-z]+)|(,[a-z])+)";
            public const string DECIMAL = "([0-1]*)";
            public const string CONST = DECIMAL;
            public const string MOV = "mov " + SPACE + REGISTER + SPACE + "," + SPACE + REGISTER + SPACE;
        }
    }

    private static byte RegToByte(string reg)
    {
        reg = reg.Trim().ToLower();
        if (reg == "sp") return 0b0000_0111;
        return Convert.ToByte(reg[0] - 97); // returns a : 0, b : 1, ... g: 6
    }

    public static bool match(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase).Success;
    }

    /// <sumary> returns an empty string if it is grammatically correct, and an error message if otherwise </summary> 
    public static byte[] evaluateMOV(string line)
    {
        byte[] r = new byte[0];
        if (match(line, LEXICON.SYNTAX.MOV, true))
        {
            Match m = Regex.Match(line, LEXICON.TOKENS.REGISTER);
            r = new byte[2] { RegToByte(m.Value), RegToByte(m.NextMatch().Value) };
        }
        return r;
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
