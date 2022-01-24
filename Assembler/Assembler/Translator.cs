using System.Text.RegularExpressions;

namespace Assembler;

/// <summary> Contains the necessary function for the translation </summary>
public static class Translator
{
    /// <summary> starters of statement like MOV: "((space)*mov )"
    private static class STARTERS
    {
        public const string MOV = "^(" + LEXICON.SPACE + "mov )";
        public const string JMP = "^(" + LEXICON.SPACE + "jmp )";
        public const string JCAZ = "^(" + LEXICON.SPACE + LEXICON.SYNTAX.JCAZ + " )";
    }
    private static byte RegToByte(string reg)
    {
        reg = reg.Trim().ToLower();
        if (reg == "sp") return 0b0000_0111;
        return Convert.ToByte(reg[0] - 97); // returns a : 0, b : 1, ... g: 6
    }
    private static byte OffsetToByte(string offSetLine)
    {
        byte b2 = 0;
        sbyte offset = Convert.ToSByte(getMatch(offSetLine, LEXICON.TOKENS.OFFSET).Value.Replace(" ", ""));
        if (offset < 0) { b2 |= 0b1000_0000; offset = (sbyte)((offset * -1) - 1); }
        if (offset >= 8) { b2 |= 0b0100_0000; offset -= 8; }
        if (offset >= 4) { b2 |= 0b0010_0000; offset -= 4; }
        if (offset >= 2) { b2 |= 0b0001_0000; offset -= 2; }
        if (offset >= 1) { b2 |= 0b0000_1000; offset -= 1; }
        return b2;
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

    private static byte[] translateMOV(string line)
    {
        byte[] r = new byte[2];
        if (match(line, LEXICON.SYNTAX.MOV_R_R, true))
        {
            Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
            r[0] = RegToByte(m.Value);
            r[1] = RegToByte(m.NextMatch().Value);
        }
        else if (match(line, LEXICON.SYNTAX.MOV_R_C, true))
        {
            Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
            r[0] = Convert.ToByte( RegToByte(getMatch(line, LEXICON.TOKENS.REGISTER).Value) | 0b0000_1000);
            r[1] = Convert.ToByte(getMatch(line,LEXICON.TOKENS.CONST).Value);
        }
        else if (match(line, LEXICON.SYNTAX.MOV_R_A, true))
        {
            if (match(line, LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                r = new byte[2] {
                    Convert.ToByte(RegToByte(m.Value) | 0b0001_0000),
                    Convert.ToByte(
                        RegToByte(m.NextMatch().Value) |
                        OffsetToByte(getMatch(line, LEXICON.TOKENS.OFFSET).Value.Replace(" ", ""))
                    )
                };
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
        else if (match(line, LEXICON.SYNTAX.MOV_A_R, true))
        {
            if (match(line, LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                r = new byte[2] {
                    Convert.ToByte(RegToByte(m.Value) | 0b0010_0000),
                    Convert.ToByte(
                        RegToByte(m.NextMatch().Value) |
                        OffsetToByte(getMatch(line, LEXICON.TOKENS.OFFSET).Value.Replace(" ", ""))
                    )
                };
            }
            else if (match(line, LEXICON.TOKENS.ADDRESS_REGISTER))
            {
                Match m = getMatch(line, LEXICON.TOKENS.REGISTER);
                byte b1 = RegToByte(m.Value);
                byte b2 = RegToByte(m.NextMatch().Value);
                r = new byte[2] { Convert.ToByte(b1 | 0b0010_0000), b2 };
            }
            else if (match(line, LEXICON.TOKENS.ADDRESS_CONST))
            {
                byte b1 = RegToByte(getMatch(line, LEXICON.TOKENS.REGISTER).Value);
                byte b2 = Convert.ToByte(getMatch(line, LEXICON.TOKENS.CONST).Value);
                r = new byte[2] { Convert.ToByte(b1 | 0b0010_1000), b2 };
            }
        }
        return r;
    }

    private static byte[] translateJMP(string line)
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

    private static byte[] translateJCAZ(string line)
    {
        byte getJCAZFlags(string flagstring)
        {
            byte return_byte = 0b0000_0000;
            if (match(flagstring, "c")) return_byte |= 0b0000_0100;
            if (match(flagstring, "a")) return_byte |= 0b0000_0010;
            if (match(flagstring, "z")) return_byte |= 0b0000_0001;
            return return_byte;
        }
        byte[] r = new byte[0] { };
        if (match(line, LEXICON.SYNTAX.JCAZ_0, true))
        {
            r = new byte[2] {
                (byte)(getJCAZFlags(getMatch(line, LEXICON.SYNTAX.JCAZ).Value) | 0b0100_0000),
                RegToByte(getMatch(line, " " + LEXICON.TOKENS.REGISTER).Value)
            };
        }
        else if (match(line, LEXICON.SYNTAX.JCAZ_1, true))
        {
            r = new byte[2] {
                (byte)(getJCAZFlags(getMatch(line, LEXICON.SYNTAX.JCAZ).Value) | 0b0100_1000),
                Convert.ToByte(getMatch(line, " " + LEXICON.TOKENS.CONST).Value)
            };
        }
        return r;
    }

    private static byte[] translatePUSH(string line)
    {
        byte[] r = new byte[0] { };
        return r;
    }

    /// <sumary>translated the line into its corresponding byte[] that represents machine code. returns an empty array if it is grammatically incorrect</summary> 
    public static byte[] translateLine(string line)
    {
        line = line.Trim();
        if (match(line, STARTERS.MOV))
            return Translator.translateMOV(line);
        else if (match(line, STARTERS.JMP))
            return Translator.translateJMP(line);
        else if (match(line, STARTERS.JCAZ))
            return Translator.translateJCAZ(line);
        else
            return new byte[0];
    }
}