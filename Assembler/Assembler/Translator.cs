using System.Text.RegularExpressions;

namespace Assembler;

/// <summary> Contains the necessary function for the translation </summary>
public static class Translator
{
    /** Convert a string read as a Register to byte code
    * " b " = 0b0000_0001
    * " g " = 0b0000_0110
    */
    private static byte RegToByte(string reg, byte conjunct = 0b0) 
    {
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
        sbyte offset = Convert.ToSByte(offSetLine);
        if (offset < 0) { b2 |= 0b1000_0000; offset = (sbyte)((offset * -1) - 1); } // + or -
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
        string[] args = line.Substring(4).Split(','); // remove "mov " and split
        
        if(Common.match(args[0],LEXICON.TOKENS.REGISTER,true)){     // mov r,_
            if(Common.match(args[1],LEXICON.TOKENS.ADDRESS)){       // mov r,a
                // remove []
                args[1] = args[1].Substring(1,args[1].Length-2);
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
            else if(Common.match(args[1],LEXICON.TOKENS.REGISTER,true)){ // mov r,r
                r[0] = RegToByte(args[0]);
                r[1] = RegToByte(args[1]);

            }else if(Common.match(args[1],LEXICON.TOKENS.CONST,true)){ // mov r,c
                r[0] = RegToByte(args[0], 0b0000_1000);
                r[1] = Convert.ToByte(args[1]);

            }
        }else if(Common.match(args[0],LEXICON.TOKENS.ADDRESS,true)){ // mov a,r
            args[0] = args[0].Substring(1,args[0].Length-2); // remove the "[" & "]"
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
        string arg = line.Substring(4); // remove "jmp " 
        if (Common.match(line, LEXICON.SYNTAX.JMP_R, true))
            return new byte[1] { RegToByte(arg, 0b0011_0000) };
        
        else if (Common.match(line, LEXICON.SYNTAX.JMP_C, true))
            return new byte[2] { 0b0011_1000, Convert.ToByte(arg) };
        
        else return new byte[0]; // never called when the input program is in correct syntax
    }
    private static byte[] translateJCAZ(string line)
    {
        byte getJCAZFlags(string flagstring, byte conjugate = 0b0100_0000)
        {
            if (flagstring.Contains('c')) conjugate |= 0b0000_0100;
            if (flagstring.Contains('a')) conjugate |= 0b0000_0010;
            if (flagstring.Contains('z')) conjugate |= 0b0000_0001;
            return conjugate;
        }
        byte[] r = new byte[2];
        string arg = line.Split(' ')[1];
        r[0] = getJCAZFlags(Common.getMatch(line, LEXICON.SYNTAX.JCAZ).Value);
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
        string arg = line.Substring(5); // remove "push "

        if (Common.match(line, LEXICON.SYNTAX.PUSH_R))
            r = new byte[1] { RegToByte(arg, 0b0101_0000) };
        
        else if (Common.match(line, LEXICON.SYNTAX.PUSH_A)){
            arg = arg.Substring(1,arg.Length-2);
            r[0] = 0b0101_1000;

            if(Common.match(line, LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET))
                r[1] = RegOffsetToByte(arg);
            
            else if (Common.match(arg, LEXICON.TOKENS.REGISTER))
                r[1] = RegToByte(arg);

            else if(Common.match(line, LEXICON.TOKENS.CONST)){
                r[0] |= 0b1;
                r[1] = Convert.ToByte(arg);
            }
        }
        else if(Common.match(line, LEXICON.SYNTAX.PUSH_C)){
            r[0] = 0b0101_1010;
            r[1] = Convert.ToByte(arg);
        }
        
        return r;
    }
    private static byte[] translatePOP(string line){
        byte[] r = new byte[2];
        string arg = line.Substring(4); // remove "pop "

        if (Common.match(line, LEXICON.SYNTAX.PUSH_R))
            r = new byte[1] { RegToByte(arg, 0b0110_0000) };


        return r;
    }

    /// <sumary>translated the line into its corresponding byte[] that represents machine code. returns an empty array if it is grammatically incorrect</summary> 
    public static byte[] translateLine(string line)
    {
        if (Common.match(line, "^mov "))
            return translateMOV(line);
        else if (Common.match(line, "^jmp "))
            return translateJMP(line);
        else if (Common.match(line, "^"+LEXICON.SYNTAX.JCAZ+" "))
            return translateJCAZ(line);
        else if (Common.match(line, "^push "))
            return translatePUSH(line);
        else
            return new byte[0];
    }
}