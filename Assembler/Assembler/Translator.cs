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
        public const string PUSH = "^(" + LEXICON.SPACE + "push )";
        public const string POP = "^(" + LEXICON.SPACE + "pop )";
    }

    /** Convert a string read as a Register to byte code
    * " b " = 0b0000_0001
    * " g " = 0b0000_0110
    */
    private static byte RegToByte(string reg, byte conjunct = 0b0) 
    {
        reg = reg.Trim().ToLower();
        if (reg == "sp") return (byte)(0b0000_0111 | conjunct);
        return Convert.ToByte( (reg[0] - 97) | conjunct ); // returns a : 0, b : 1, ... g: 6
    }

    /** Convert Offset string to byte
    * "+   15" = 0b0111_1000
    * "- 14"   = 0b1111_0000
    */
    private static byte OffsetToByte(string offSetLine)
    {
        byte b2 = 0;
        //sbyte offset = Convert.ToSByte(Common.getMatch(offSetLine, LEXICON.TOKENS.OFFSET).Value.Replace(" ", ""));
        sbyte offset = Convert.ToSByte(offSetLine.Replace(" ",""));
        if (offset < 0) { b2 |= 0b1000_0000; offset = (sbyte)((offset * -1) - 1); }
        if (offset >= 8) { b2 |= 0b0100_0000; offset -= 8; }
        if (offset >= 4) { b2 |= 0b0010_0000; offset -= 4; }
        if (offset >= 2) { b2 |= 0b0001_0000; offset -= 2; }
        if (offset >= 1) { b2 |= 0b0000_1000; offset -= 1; }
        return b2;
    }

    private static byte RegOffsetToByte(string ro_line){
        return RegToByte(
            Common.getMatch(ro_line, LEXICON.TOKENS.REGISTER).Value,
            OffsetToByte(Common.getMatch(ro_line, LEXICON.TOKENS.OFFSET).Value)
        );
    }
    private static byte[] translateMOV(string line)
    {
        byte[] r = new byte[2];
        string[] args = line.Replace("mov ","").Split(',',StringSplitOptions.TrimEntries);
        if (Common.match(line, LEXICON.SYNTAX.MOV_R_R, true))
        {
            r[0] = RegToByte(args[0]);
            r[1] = RegToByte(args[1]);
        }
        else if (Common.match(line, LEXICON.SYNTAX.MOV_R_C, true))
        {
            r[0] = RegToByte(args[0], 0b0000_1000);
            r[1] = Convert.ToByte(args[1]);
        }
        else if (Common.match(line, LEXICON.SYNTAX.MOV_R_A, true)) // load
        {
            args[1] = args[1].Replace("[","").Replace("]","");
            r[0] = RegToByte(args[0], 0b0001_0000);
            if (Common.match(args[1], LEXICON.TOKENS.OFFSET))
                r[1] = RegOffsetToByte(args[1]);

            else if (Common.match(args[1], LEXICON.TOKENS.REGISTER))
                r[1] = RegToByte(args[1]);
                
            else if (Common.match(args[1], LEXICON.TOKENS.CONST))
            {
                r[0] |= 0b1000;
                r[1] = Convert.ToByte(args[1]);
            }
        }
        else if (Common.match(line, LEXICON.SYNTAX.MOV_A_R, true)) // store
        {
            args[0] = args[0].Replace("[","").Replace("]",""); 
            r[0] = RegToByte(args[1], 0b0010_0000);
            if (Common.match(args[0], LEXICON.TOKENS.OFFSET))   
                r[1] = RegOffsetToByte(args[0]);
                
            else if (Common.match(args[0], LEXICON.TOKENS.REGISTER))
                r[1] = RegToByte(args[0]);
            
            else if (Common.match(args[0], LEXICON.TOKENS.CONST))
            {
                r[0] |= 0b1000;
                r[1] = Convert.ToByte(args[0]);
            }
        }
        return r;
    }
    private static byte[] translateJMP(string line)
    {
        string arg = line.Replace("jmp ", "");
        byte[] r = new byte[0];
        if (Common.match(line, LEXICON.SYNTAX.JMP_R, true))
            r = new byte[1] { RegToByte(arg, 0b0011_0000) };
        
        else if (Common.match(line, LEXICON.SYNTAX.JMP_C, true))
            r = new byte[2] { 0b0011_1000, Convert.ToByte(arg) };
        return r;
    }
    private static byte[] translateJCAZ(string line)
    {
        byte getJCAZFlags(string flagstring, byte conjugate = 0b0)
        {
            if (Common.match(flagstring, "c")) conjugate |= 0b0000_0100;
            if (Common.match(flagstring, "a")) conjugate |= 0b0000_0010;
            if (Common.match(flagstring, "z")) conjugate |= 0b0000_0001;
            return conjugate;
        }
        byte[] r = new byte[2];
        string arg = line.Split(' ')[1];
        r[0] = getJCAZFlags(Common.getMatch(line, LEXICON.SYNTAX.JCAZ).Value, 0b0100_0000);
        if (Common.match(line, LEXICON.SYNTAX.JCAZ_R, true))
            r[1] = RegToByte(arg);
            
        else if (Common.match(line, LEXICON.SYNTAX.JCAZ_C, true))
        {
            r[0] |= 0b1000;
            r[1] = Convert.ToByte(arg);
        }
        return r;
    }
    private static byte[] translatePUSH(string line)
    {
        byte[] r = new byte[2];
        string arg = line.Replace("push ","");

        if (Common.match(line, LEXICON.SYNTAX.PUSH_R))
            r = new byte[1] { RegToByte(arg, 0b0101_0000) };
        
        else if (Common.match(line, LEXICON.SYNTAX.PUSH_A)){
            if(Common.match(line, LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET)){
                r[0] = 0b0101_1000;
                r[1] = RegOffsetToByte(arg);
            }
        }
        else if(Common.match(line, LEXICON.SYNTAX.PUSH_C)){
            r[0] = 0b0101_1010;
            r[1] = Convert.ToByte(arg);
        }
        
        return r;
    }

    /// <sumary>translated the line into its corresponding byte[] that represents machine code. returns an empty array if it is grammatically incorrect</summary> 
    public static byte[] translateLine(string line)
    {
        line = line.Trim();
        if (Common.match(line, STARTERS.MOV))
            return translateMOV(line);
        else if (Common.match(line, STARTERS.JMP))
            return translateJMP(line);
        else if (Common.match(line, STARTERS.JCAZ))
            return translateJCAZ(line);
        else if (Common.match(line, STARTERS.PUSH))
            return translatePUSH(line);
        else
            return new byte[0];
    }
}