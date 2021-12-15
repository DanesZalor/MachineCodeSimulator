namespace Assembler;
public static class LEXICON
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
        public const string MOV = SPACE + "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;

        ///<summary> mov reg, const </summary> 
        public const string DATA = SPACE + "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.CONST + SPACE;


        public const string LOAD = SPACE + "mov " + SPACE + TOKENS.REGISTER + SPACE + "," + SPACE + TOKENS.ADDRESS + SPACE;

        public const string STORE = SPACE + "mov " + SPACE + TOKENS.ADDRESS + SPACE + "," + SPACE + TOKENS.REGISTER + SPACE;

        ///<summary> jmp reg </summary>
        public const string JMP_0 = SPACE + "jmp " + SPACE + TOKENS.REGISTER + SPACE;
        ///<summary> jmp const </summary>
        public const string JMP_1 = SPACE + "jmp " + SPACE + TOKENS.CONST + SPACE;


        public const string JCAZ = "(j((ca?z?)|(c?az?)|(c?a?z)))";
        ///<summary> jcaz reg </summary>
        public const string JCAZ_0 = SPACE + JCAZ + " " + SPACE + TOKENS.REGISTER + SPACE;
        ///<summary> jcaz const </summary>
        public const string JCAZ_1 = SPACE + JCAZ + " " + SPACE + TOKENS.CONST + SPACE;

        public const string PUSH_0 = "push " + SPACE + TOKENS.REGISTER + SPACE;
        public const string PUSH_1 = "push " + SPACE + TOKENS.ADDRESS + SPACE;
    }


}