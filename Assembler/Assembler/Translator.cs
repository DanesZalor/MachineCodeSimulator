using System.Text.RegularExpressions;

namespace Assembler;

/// <summary> Contains the necessary function for the translation 
public static class Translator
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
    private static bool match(string line, string pattern, bool exact = false)
    {
        return getMatch(line, pattern, exact).Success;
    }

    private static byte[] translateMOV(string line)
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
        else if (match(line, LEXICON.SYNTAX.STORE, true))
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
                r = new byte[2] { Convert.ToByte(b1 | 0b0010_0000), b2 };
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

    private static byte[] translatePUSH(string line)
    {
        byte[] r = new byte[0] { };
        return r;
    }

    /// <sumary>translated the line into its corresponding byte[] that represents machine code. returns an empty array if it is grammatically incorrect</summary> 
    public static byte[] translateLine(string line)
    {
        line = line.Trim();
        if (match(line, LEXICON.ETC.mov_starter))
            return Translator.translateMOV(line);
        else if (match(line, LEXICON.ETC.jmp_starter))
            return Translator.translateJMP(line);
        else if (match(line, LEXICON.ETC.jcaz_starter))
            return Translator.translateJCAZ(line);
        else
            return new byte[0];
    }
}