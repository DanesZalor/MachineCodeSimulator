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
        if (Common.match(arg, LEXICON.TOKENS.REGISTER)) // jmp r
            return new byte[1] { RegToByte(arg, 0b0011_0000) };
        
        else if (Common.match(arg, LEXICON.TOKENS.DECIMAL)) // jmp c
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
        if (Common.match(arg, LEXICON.TOKENS.REGISTER, true))
            r[1] = RegToByte(arg);
            
        else if (Common.match(arg, LEXICON.TOKENS.CONST, true))
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

        if(Common.match(arg, LEXICON.TOKENS.ADDRESS,true)){ // push a
            // remove []
            arg = arg.Substring(1,arg.Length-2);
            r[0] = 0b0101_1000;

            if(Common.match(arg, LEXICON.TOKENS.OFFSET))
                r[1] = RegOffsetToByte(arg);
            
            else if (Common.match(arg, LEXICON.TOKENS.REGISTER))
                r[1] = RegToByte(arg);

            else if(Common.match(line, LEXICON.TOKENS.CONST)){
                r[0] |= 0b1;
                r[1] = Convert.ToByte(arg);
            }
        }else if(Common.match(arg, LEXICON.TOKENS.REGISTER,true)){ // push r
            r = new byte[1] { RegToByte(arg, 0b0101_0000) };
            
        }else if(Common.match(arg, LEXICON.TOKENS.CONST,true)){ // push c
            r[0] = 0b0101_1010;
            r[1] = Convert.ToByte(arg);
        }
        
        return r;
    }
    private static byte[] translatePOP(string line){
        byte[] r = new byte[2];
        string arg = line.Substring(4); // remove "pop "

        if(Common.match(arg, LEXICON.TOKENS.ADDRESS,true)){ // pop a
            // remove []
            arg = arg.Substring(1,arg.Length-2);
            r[0] = 0b0110_1000;

            if(Common.match(arg, LEXICON.TOKENS.OFFSET))
                r[1] = RegOffsetToByte(arg);
            
            else if (Common.match(arg, LEXICON.TOKENS.REGISTER))
                r[1] = RegToByte(arg);

            else if(Common.match(line, LEXICON.TOKENS.CONST)){
                r[0] |= 0b1;
                r[1] = Convert.ToByte(arg);
            }
        }else if(Common.match(arg, LEXICON.TOKENS.REGISTER,true)){ // pop r
            r = new byte[1] { RegToByte(arg, 0b0110_0000) };
            
        }
        return r;
    }

    private static byte[] translateCALL(string line){
        byte[] r = new byte[2];
        string arg = line.Substring(5); // remove "call "

        if(Common.match(arg, LEXICON.TOKENS.REGISTER,true)) // call reg
            r = new byte[1] { RegToByte(arg, 0b0111_0000)};

        else if (Common.match(arg, LEXICON.TOKENS.DECIMAL)){ // call const
            r[0] = 0b0111_1000;
            r[1] = Convert.ToByte(arg);
        } 

        return r;
    }

    public static byte[] translateALU(string line){
        string[] tmp = line.Split(' ');
        string op = tmp[0]; 
        string[] args = tmp[1].Split(',');

        if(args.Length==1 && Common.match(op,"(not|inc|dec|shl|shr)")){ // ALU 1 arg
            byte op_conjugate = 0b0;
            switch(op){
                case "not": op_conjugate = 0b1000_0000; break;
                case "inc": op_conjugate = 0b1000_1000; break;
                case "dec": op_conjugate = 0b1001_0000; break;
                case "shl": op_conjugate = 0b1001_1000; break;
                case "shr": op_conjugate = 0b1010_0000; break;
            }
            return new byte[1] { RegToByte(args[0],op_conjugate) };
        }
        else if(args.Length==2 && Common.match(op,"(cmp|xor|and|or|shl|shr|mul|div|add|sub)")){
            byte op_conjugate = 0b0;
            switch(op){
                case "cmp": op_conjugate = 0b1100_0000; break;
                case "xor": op_conjugate = 0b1100_0001; break;
                case "and": op_conjugate = 0b1100_0010; break;
                case "or":  op_conjugate = 0b1100_0011; break;
                case "shl": op_conjugate = 0b1100_0100; break;
                case "shr": op_conjugate = 0b1100_0101; break;
                case "mul": op_conjugate = 0b1100_0110; break;
                case "div": op_conjugate = 0b1100_0111; break;
                case "add": op_conjugate = 0b1100_1000; break;
                case "sub": op_conjugate = 0b1100_1001; break;
            }
            
            byte regA = (byte)(RegToByte(args[0])<<4);
            if(Common.match(args[1], LEXICON.TOKENS.REGISTER,true)) // op r,r
                return new byte[2]{op_conjugate, RegToByte(args[1], regA ) };
            
            else{ // op r,x
                byte[] r = new byte[3]{op_conjugate, (byte)(regA | 0b0000_1000), 0b0};
                
                if(Common.match(args[1], LEXICON.TOKENS.ADDRESS,true)){ // op r,a
                    // remove []
                    args[1] = args[1].Substring(1,args[1].Length-2);
                    
                    if(Common.match(args[1], LEXICON.TOKENS.OFFSET))
                        r[2] = RegOffsetToByte(args[1]);
                    
                    else if (Common.match(args[1], LEXICON.TOKENS.REGISTER))
                        r[2] = RegToByte(args[1]);

                    else if (Common.match(args[1], LEXICON.TOKENS.DECIMAL)){
                        r[1] |= 0b1;
                        r[2] = Convert.ToByte(args[1]);
                    }
                        
                }else if(Common.match(args[1],LEXICON.TOKENS.DECIMAL,true)){ // op r,c
                    r[1] |= 0b10;
                    r[2] = Convert.ToByte(args[1]);
                }

                return r;
            }
        }
        return new byte[0]{};
    }

    private static byte[] translateDB(string line){
        string arg = line.Substring(3); // remove "db "
        return new byte[]{Convert.ToByte(arg)};
    }

    private static byte[] translateETC(string line){
        byte[] r = new byte[1];
        switch(line){
            case "clf": r[0] = 0b1101_0000; break;
            case "ret": r[0] = 0b1101_0001; break;
            case "hlt": r[0] = 0b1101_0010; break;
        }
        return r;
    }

    private static string[] LineStarters = {
        "mov", "jmp", LEXICON.SYNTAX.JCAZ, "push", "pop", "call", 
        "(cmp|xor|and|or|shl|shr|mul|div|add|sub|inc|dec|not)",
        "db","(ret|hlt|clf)"
    };
    private static Func<string,byte[]>[] translateFuncs = {
        translateMOV, translateJMP, translateJCAZ, translatePUSH,
        translatePOP, translateCALL, translateALU, translateDB, translateETC
    };
    /// <sumary>translated the line into its corresponding byte[] that represents machine code. returns an empty array if it is grammatically incorrect</summary> 
    public static byte[] translateLine(string line)
    {
        for(int i = 0; i < 9; i++)
            if (Common.match(line, LineStarters[i])) return translateFuncs[i](line);
        
        return new byte[0];
    }

    /** Assumptions: 
            The arguement is syntactically correct
            and is derived via Preprocessor Translations Phase
            The program does not exceed 256 byte
    */
    public static byte[] translateProgram(string linesOfCode){
        byte[] bin = new byte[256];

        string[] lines = linesOfCode.Split("\n");
        byte ctr = 0;
        foreach(string line in lines){
            byte[] bytes = translateLine(line);
            
            foreach(byte b in bytes)
                bin[ctr++] = b;
        }
        
        byte[] finalbin = new byte[ctr];
        for(int i = 0; i<finalbin.Length; i++)
            finalbin[i] = bin[i];

        return finalbin;
    }
}