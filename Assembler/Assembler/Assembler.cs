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
            private const string DECIMAL = "(" +
                "(25[0-5])|" +       // 250-255
                "(2[0-4][0-9])|" +  // 200 - 249
                "(1[0-9]{0,2})|" +  // 1, 10-19, 100-199
                "([1-9][0-9]?)" +  // 1-99
            ")";
            public const string OFFSET = "(" +
                "(\\+" + SPACE + "((1[1-5])|[1-9]))|" +  // +1 to +15
                "(-" + SPACE + "((1[1-6])|[1-9]))" +     // -1 to -16
            ")";

            public const string ADDRESS_REGISTER = "\\[" + SPACE + REGISTER + SPACE + "\\]";
            public const string ADDRESS_CONST = "\\[" + SPACE + CONST + SPACE + "\\]";
            public const string ADDRESS_REGISTER_OFFSET = "\\[" + SPACE + REGISTER + SPACE + OFFSET + SPACE + "\\]";
            public const string ADDRESS = "(" +
                ADDRESS_REGISTER_OFFSET + "|" +
                ADDRESS_CONST + "|" +
                ADDRESS_REGISTER +
            ")";
            public const string CONST = DECIMAL;
        }

        public static class SYNTAX
        {
            ///<summary> mov reg, reg </summary> 
            public const string MOV = "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;

            ///<summary> mov reg, const </summary> 
            public const string DATA = "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.CONST + SPACE;


            public const string LOAD = "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.ADDRESS + SPACE;

            ///<summary> mov [reg+offset], reg </summary> 
            public const string STORE_0 = "mov " + SPACE + TOKENS.ADDRESS_REGISTER_OFFSET + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;
            ///<summary> mov [reg], reg </summary>
            public const string STORE_1 = "mov " + SPACE + TOKENS.ADDRESS_REGISTER + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;
            ///<summary> mov [const], reg </summary>
            public const string STORE_2 = "mov " + SPACE + TOKENS.ADDRESS_CONST + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;

            ///<summary> jmp reg </summary>
            public const string JMP_0 = "jmp " + SPACE + TOKENS.REGISTER + SPACE;
            ///<summary> jmp const </summary>
            public const string JMP_1 = "jmp " + SPACE + TOKENS.CONST + SPACE;


            public const string JCAZ = "((jca?z?)|(jc?az?)|(jc?a?z)) ";
            ///<summary> jcaz reg </summary>
            public const string JCAZ_0 = JCAZ + SPACE + TOKENS.REGISTER + SPACE;
            ///<summary> jcaz const </summary>
            public const string JCAZ_1 = JCAZ + SPACE + TOKENS.CONST + SPACE;

            public const string PUSH_0 = "push " + SPACE + TOKENS.REGISTER + SPACE;
            public const string PUSH_1 = "push " + SPACE + TOKENS.ADDRESS + SPACE;
        }


    }

    private static class Translator
    {
        private static byte RegToByte(string reg)
        {
            reg = reg.Trim().ToLower();
            if (reg == "sp") return 0b0000_0111;
            return Convert.ToByte(reg[0] - 97); // returns a : 0, b : 1, ... g: 6
        }
        private static Match getMatch(string line, string pattern, bool exact = false)
        {
            if (exact) pattern = "^" + pattern + "$";
            return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        }
        public static bool match(string line, string pattern, bool exact = false)
        {
            return getMatch(line, pattern, exact).Success;
        }

        public static byte[] translateMOV(string line)
        {
            byte[] r = new byte[0];
            if (match(line, LEXICON.SYNTAX.MOV, true))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                r = new byte[2] { RegToByte(m.Value), RegToByte(m.NextMatch().Value) };
            }
            else if (match(line, LEXICON.SYNTAX.DATA, true))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                byte b1 = RegToByte(getMatch(line, LEXICON.TOKENS.REGISTER).Value);
                r = new byte[2] {
                Convert.ToByte( b1 | 0b0000_1000),
                Convert.ToByte(getMatch(line,LEXICON.TOKENS.CONST).Value)
            };
            }
            else if (match(line, LEXICON.SYNTAX.LOAD, true))
            {
                if (match(line, LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET))
                {
                    Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                    byte b1 = RegToByte(m.Value);
                    byte b2 = RegToByte(m.NextMatch().Value);

                    {// Assign the <5:offset> bytes to b2
                     // get OFFSET substring and remove all spaces from the match. 
                        sbyte offset = Convert.ToSByte(getMatch(line, LEXICON.TOKENS.OFFSET).Value.Replace(" ", ""));
                        if (offset < 0) { b2 |= 0b1000_0000; offset = (sbyte)((offset * -1) - 1); }
                        if (offset >= 8) { b2 |= 0b0100_0000; offset -= 8; }
                        if (offset >= 4) { b2 |= 0b0010_0000; offset -= 4; }
                        if (offset >= 2) { b2 |= 0b0001_0000; offset -= 2; }
                        if (offset >= 1) { b2 |= 0b0000_1000; offset -= 1; }
                    }
                    r = new byte[2] { Convert.ToByte(b1 | 0b0001_0000), b2 };
                }
                else if (match(line, LEXICON.TOKENS.ADDRESS_REGISTER))
                {
                    Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                    byte b1 = RegToByte(m.Value);
                    byte b2 = RegToByte(m.NextMatch().Value);
                    r = new byte[2] { Convert.ToByte(b1 | 0b0001_0000), b2 };
                }
                else if (match(line, LEXICON.TOKENS.ADDRESS_CONST))
                {
                    byte b1 = RegToByte(getMatch(line, LEXICON.TOKENS.REGISTER).Value);
                    byte b2 = Convert.ToByte(getMatch(line, LEXICON.TOKENS.CONST).Value);
                    r = new byte[2] { Convert.ToByte(b1 | 0b0001_1000), b2 };
                }
            }
            else if (match(line, LEXICON.SYNTAX.STORE_0, true))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                byte b1 = RegToByte(m.Value);
                byte b2 = RegToByte(m.NextMatch().Value);

                {// Assign the <5:offset> bytes to b2
                 // get OFFSET substring and remove all spaces from the match. 
                    sbyte offset = Convert.ToSByte(getMatch(line, LEXICON.TOKENS.OFFSET).Value.Replace(" ", ""));
                    if (offset < 0) { b2 |= 0b1000_0000; offset = (sbyte)((offset * -1) - 1); }
                    if (offset >= 8) { b2 |= 0b0100_0000; offset -= 8; }
                    if (offset >= 4) { b2 |= 0b0010_0000; offset -= 4; }
                    if (offset >= 2) { b2 |= 0b0001_0000; offset -= 2; }
                    if (offset >= 1) { b2 |= 0b0000_1000; offset -= 1; }
                }
                r = new byte[2] { Convert.ToByte(b1 | 0b0010_0000), b2 };
            }
            else if (match(line, LEXICON.SYNTAX.STORE_1, true))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                byte b1 = RegToByte(m.Value);
                byte b2 = RegToByte(m.NextMatch().Value);
                r = new byte[2] { Convert.ToByte(b1 | 0b0010_0000), b2 };
            }
            else if (match(line, LEXICON.SYNTAX.STORE_2, true))
            {
                byte b1 = RegToByte(getMatch(line, LEXICON.TOKENS.REGISTER).Value);
                byte b2 = Convert.ToByte(getMatch(line, LEXICON.TOKENS.CONST).Value);
                r = new byte[2] { Convert.ToByte(b1 | 0b0010_1000), b2 };
            }
            return r;
        }

        public static byte[] translateJMP(string line)
        {
            byte[] r = new byte[0];
            if (match(line, LEXICON.SYNTAX.JMP_0, true))
            {
                r = new byte[1] { RegToByte(getMatch(line, LEXICON.TOKENS.REGISTER).Value) };
                r[0] |= 0b0011_0000;
            }
            else if (match(line, LEXICON.SYNTAX.JMP_1, true))
                r = new byte[2] { 0b0011_1000, Convert.ToByte(getMatch(line, LEXICON.TOKENS.CONST).Value) };
            return r;
        }

        public static byte[] translateJCAZ(string line)
        {
            byte[] r = new byte[0] { };
            if (match(line, LEXICON.SYNTAX.JCAZ_0, true))
            {
                string jcaz = getMatch(line, LEXICON.SYNTAX.JCAZ).Value;
                r = new byte[2] { 0b0100_0000, RegToByte(getMatch(line, " " + LEXICON.TOKENS.REGISTER).Value) };
                if (match(jcaz, "c")) r[0] |= 0b0000_0100;
                if (match(jcaz, "a")) r[0] |= 0b0000_0010;
                if (match(jcaz, "z")) r[0] |= 0b0000_0001;
            }
            else if (match(line, LEXICON.SYNTAX.JCAZ_1, true))
            {
                string jcaz = getMatch(line, LEXICON.SYNTAX.JCAZ).Value;
                r = new byte[2] { 0b0100_1000, Convert.ToByte(getMatch(line, " " + LEXICON.TOKENS.CONST).Value) };
                if (match(jcaz, "c")) r[0] |= 0b0000_0100;
                if (match(jcaz, "a")) r[0] |= 0b0000_0010;
                if (match(jcaz, "z")) r[0] |= 0b0000_0001;
            }
            return r;
        }

        public static byte[] translatePUSH(string line)
        {
            byte[] r = new byte[0] { };
            return r;
        }
    }

    /// <sumary>translated the line into its corresponding byte[] that represents machine code. returns an empty array if it is grammatically incorrect</summary> 
    public static byte[] translateLine(string line)
    {
        if (Translator.match(line, "^mov ")) return Translator.translateMOV(line);
        else if (Translator.match(line, "^jmp ")) return Translator.translateJMP(line);
        else if (Translator.match(line, "^(jc?a?z?) ")) return Translator.translateJCAZ(line);
        else return new byte[0];
    }

}
