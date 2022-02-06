namespace Assembler;

/// <summary> Base lexicon, does not include the labels </summary>
public static class LEXICON
{
    public const string SPACE = "( )*";
    public const string RESERVED_WORDS = "(mov|jmp|(jca?z?)|(jc?az?)|(jc?a?z)|push|pop|call|ret|clf|not|inc|dec|add|sub|mul|div|shl|shr|or|xor|and|cmp)";
    public static class TOKENS
    {
        public const string REGISTER = "([a-g]|(sp))";
        private const string STRING = "(\".*\")";
        public const string DECIMAL = "(0*(" +
            "(25[0-5])|" +       // 250-255
            "(2[0-4][0-9])|" +  // 200 - 249
            "(1[0-9]{0,2})|" +  // 1, 10-19, 100-199
            "([1-9][0-9]?)|" +  // 1-99
            "(0)" +
        "))";
        private const string HEXADECIMAL = "(0x([0-9]|[a-f]){1,2})";
        private const string BINARY = "(0b[01]{1,8})";

        public const string OFFSET = "(" +
            "(\\+" + SPACE + "((1[0-5])|[1-9]))|" +  // +1 to +15
            "(-" + SPACE + "((1[0-6])|[1-9]))" +     // -1 to -16
        ")";

        public const string ADDRESS_REGISTER = "(\\[" + SPACE + REGISTER + SPACE + "\\])";
        public const string ADDRESS_CONST = "(\\[" + SPACE + CONST + SPACE + "\\])";
        public const string ADDRESS_REGISTER_OFFSET = "(\\[" + SPACE + REGISTER + SPACE + OFFSET + SPACE + "\\])";
        public const string ADDRESS = "(" +
            ADDRESS_REGISTER_OFFSET + "|" +
            ADDRESS_CONST + "|" +
            ADDRESS_REGISTER +
        ")";
        public const string CONST = "(" + 
            BINARY + "|" + HEXADECIMAL + "|" + DECIMAL + 
        ")";
        public const string ANY = "(" +
            REGISTER + "|" +
            CONST + "|" +
            ADDRESS + "|" +
        ")";
    }



    public static class SYNTAX
    {
        public static class ARGUEMENTS
        {
            public const string R = "(" + SPACE + TOKENS.REGISTER + SPACE + ")";
            public const string C = "(" + SPACE + TOKENS.CONST + SPACE + ")";
            public const string A = "(" + SPACE + TOKENS.ADDRESS + SPACE + ")";
            public const string R_R = "(" + R + "," + R + ")";
            public const string R_C = "(" + R + "," + C + ")";
            public const string R_A = "(" + R + "," + A + ")";
            public const string A_R = "(" + A + "," + R + ")";

            public const string DYADIC = "(" +
                R_R + "|" +
                R_C + "|" +
                R_A + "|" +
                A_R + "|" +
            ")";
        }

        public const string MOV = SPACE + "mov " + ARGUEMENTS.DYADIC;
        ///<summary> mov reg, reg </summary> 
        public const string MOV_R_R = SPACE + "mov " + ARGUEMENTS.R_R;

        ///<summary> mov reg, const </summary> 
        public const string MOV_R_C = SPACE + "mov " + ARGUEMENTS.R_C;


        public const string MOV_R_A = SPACE + "mov " + ARGUEMENTS.R_A;

        public const string MOV_A_R = SPACE + "mov " + ARGUEMENTS.A_R;

        ///<summary> jmp reg </summary>
        public const string JMP_R = SPACE + "jmp " + ARGUEMENTS.R;
        ///<summary> jmp const </summary>
        public const string JMP_C = SPACE + "jmp " + ARGUEMENTS.C;


        public const string JCAZ = "(j((ca?z?)|(c?az?)|(c?a?z)))";
        ///<summary> jcaz reg </summary>
        public const string JCAZ_R= SPACE + JCAZ + " " + ARGUEMENTS.R;
        ///<summary> jcaz const </summary>
        public const string JCAZ_C = SPACE + JCAZ + " " + ARGUEMENTS.C;

        public const string PUSH_R = SPACE + "push " + ARGUEMENTS.R;
        public const string PUSH_A = SPACE + "push " + ARGUEMENTS.A;
        public const string PUSH_C = SPACE + "push " + ARGUEMENTS.C;
    }
}