using System.Text.RegularExpressions;
using System;

namespace Assembler{
public static class SyntaxChecker
{
    /// <summary> Lexicon containing somewhat correct grammar that can be recognized for evaluation </summary>
    private static class VAGUE_LEXICON
    {
        public static class TOKENS
        {
            public const string LABEL = "(([a-z])((\\w)*))";
            private const string DECIMAL = "([0-9]+)";
            private const string HEXADECIMAL = "(0x[a-f]*)";
            private const string BINARY = "(0b[0-1]*)";
            private const string STRING = "(\".*\")";

            public const string CONST = "("+BINARY+"|"+HEXADECIMAL+"|"+ DECIMAL + ")";
            public const string OFFSET = "([+-]" + LEXICON.SPACE + "(\\d)+)";
            public const string ANY = "(" + LABEL + "|" + CONST + ")";

        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public const string L = "(" + LEXICON.SPACE + TOKENS.LABEL + LEXICON.SPACE + ")";
                public const string C = "(" + LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE + ")";
                public const string A = "(" + LEXICON.SPACE + "\\[" + LEXICON.SPACE +
                    "(" +
                        L + "|" + C + "|" +
                        "(" +
                            L + VAGUE_LEXICON.TOKENS.OFFSET +
                        ")" +
                    ")" +
                LEXICON.SPACE + "\\]" + LEXICON.SPACE + ")";
                public const string X = "(" + L + "|" + C + "|" + A + ")";
                public const string R_X = "(" + L + "," + X + ")";
                public const string A_R = "(" + A + "," + L + ")";
            }
        }
    }

    /// <summary> Newly made lexicon with the base lexicon and the added labels
    private static class NEW_LEXICON 
    {
        public static class TOKENS
        {
            private static string EXISTING_LABELS = "()";
            public static string labels() { return EXISTING_LABELS; }
            public static void labelsClear() { EXISTING_LABELS = "()"; }
            public static string labelsAdd(string label)
            {
                if(label.Length<1) return "";
                else if(Common.match(label, LEXICON.RESERVED_WORDS,true)) return "label '"+label+"' is a reserved keyword";
                else if(label.Length<3) return "label '"+label+"' must have atleast 3 characters";
                else if(Common.match(label, EXISTING_LABELS,true)) return "label '"+label+"' is a duplicate";
                else if (Common.match(label, VAGUE_LEXICON.TOKENS.LABEL, true))
                {
                    EXISTING_LABELS = EXISTING_LABELS.Replace(")", (EXISTING_LABELS == "()" ? "" : "|") + (label + ")"));
                    return "";
                }
                return "Some unforeseen error";
            }
            public static string CONST { get => "(" + LEXICON.TOKENS.CONST + "|" + EXISTING_LABELS + ")"; }
            public static string ADDRESS_CONST { get => "(\\[" + LEXICON.SPACE + CONST + LEXICON.SPACE + "\\])"; }
            public static string ADDRESS
            {
                get => "(" +
                    LEXICON.TOKENS.ADDRESS_REGISTER_OFFSET + "|" +
                    ADDRESS_CONST + "|" +
                    LEXICON.TOKENS.ADDRESS_REGISTER +
                    ")";
            }
        }
        public static class SYNTAX
        {
            public static class ARGUEMENTS
            {
                public static string C { get => "(" + LEXICON.SPACE + TOKENS.CONST + LEXICON.SPACE + ")"; }
                public static string A { get => "(" + LEXICON.SPACE + TOKENS.ADDRESS + LEXICON.SPACE + ")"; }
                public static string X
                {
                    get => "(" + LEXICON.SPACE +
                            "(" + LEXICON.SYNTAX.ARGUEMENTS.R + "|" + C + "|" + A + ")" +
                        LEXICON.SPACE + ")";
                }
                public static string R_X { get => "(" + LEXICON.SYNTAX.ARGUEMENTS.R + "," + X + ")"; }
                public static string A_R { get => "(" + A + "," + LEXICON.SYNTAX.ARGUEMENTS.R + ")"; }
            }
            public static string POP
            { get => String.Format("(pop ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, SYNTAX.ARGUEMENTS.A); }
        }
    }

    public static void setLabels(string[] labels)
    {
        NEW_LEXICON.TOKENS.labelsClear();
        for (int i = 0; i < labels.Length; i++)
            NEW_LEXICON.TOKENS.labelsAdd(labels[i]);
    }

    /// <summary> find out whats wrong with the arguement(s) </summary> 
    private static string evaluateArgs(string argsline)
    {
        string evaluation_result = "";

        string single_evaluation(string single_arg)
        {
            //single_arg = single_arg.Replace("[", "").Replace("]", "").Trim(); // if an address, break it down yo
            /*each array contains {VagueGrammar, CorrectGrammar, ErrorMsg}
                We will loop thru the array, check if the \"single_arg\" grammatically matches VagueGrammar,
                then check if grammatically matches CorrectGrammar: if it doesn't return the ErrorMsg
            */
            string[,] ArgsLexiconTable = new string[5, 3] {
                {
                    LEXICON.RESERVED_WORDS, "(\\s){100}", "a reserved word"
                },{
                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A,NEW_LEXICON.SYNTAX.ARGUEMENTS.A,(
                        (
                            Common.match(single_arg,VAGUE_LEXICON.TOKENS.OFFSET)?( // is there an offset
                                !Common.match(Common.getMatch(single_arg, VAGUE_LEXICON.TOKENS.OFFSET).Value, LEXICON.TOKENS.OFFSET, true)?( // is it not a legit offset
                                    (
                                        String.Format("'{0}' offset out of bounds",Common.getMatch(single_arg, VAGUE_LEXICON.TOKENS.OFFSET).Value)
                                    )
                                ):(  // is it a legit offset, then check the register
                                    !Common.match(single_arg.Split('+', StringSplitOptions.RemoveEmptyEntries)[0].Trim(), LEXICON.SYNTAX.ARGUEMENTS.R, true)?
                                        String.Format("'{0}' not a register",single_arg.Replace("[","").Replace("]","").Trim()):("")
                                )
                            ):( String.Format("'{0}' {1}", Regex.Replace(single_arg, "(\\[|\\])", "").Trim(), 
                                Common.match( Regex.Replace(single_arg, "(\\[|\\])", "").Trim(), VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C, true)?
                                        "not an 8-bit constant":"non-existent token" 
                                )
                            )
                        )
                    )
                },{
                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C, LEXICON.SYNTAX.ARGUEMENTS.C,
                    String.Format("'{0}' not an 8-bit constant", single_arg)
                },{
                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L,
                    "("+NEW_LEXICON.SYNTAX.ARGUEMENTS.C +"|"+LEXICON.SYNTAX.ARGUEMENTS.R+")" ,
                    String.Format("'{0}' is a non-existent token", single_arg)
                },
                { ".*", "(\\s){10}",String.Format("'{0}' invalid expression",single_arg)}
            };
            for (int j = 0; j < 4; j++)
            {
                if (Common.match(single_arg, ArgsLexiconTable[j, 0], true))
                {
                    if (!Common.match(single_arg, ArgsLexiconTable[j, 1], true))
                        return ArgsLexiconTable[j, 2];
                    return "";
                }
            }
            return "";
        }

        string[] args = argsline.Split(",");
        for (int i = 0; i < args.Length; i++)
        {
            string s = single_evaluation(args[i].Trim());
            if (s != "")
                evaluation_result += (evaluation_result != "" ? "\n" : "") + s;
        }
        return evaluation_result;
    }
    private static string evaluateMOV(string movline)
    {
        string movSyntaxVague = "mov (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.R_X + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A_R + ")";
        string movSyntax = "mov (" + NEW_LEXICON.SYNTAX.ARGUEMENTS.R_X + "|" + NEW_LEXICON.SYNTAX.ARGUEMENTS.A_R + ")";
        if (!Common.match(movline, movSyntax, true))
        {
            if (!Common.match(movline, movSyntaxVague, true)) return "invalid MOV statement";
            else return evaluateArgs(
                movline.Substring(movline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        else return "";
    }
    private static string evaluateJMP(string jmpline)
    {
        string jmpSyntax = String.Format("(jmp ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string jmpSyntaxVague = "jmp (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C + ")";
        if (!Common.match(jmpline, jmpSyntax, true))
        {
            if (!Common.match(jmpline, jmpSyntaxVague, true)) return "invalid JMP arguement";
            else return evaluateArgs(
                jmpline.Substring(jmpline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }
    private static string evaluateJmpIf(string jmpline)
    {
        string[] jmpline_splitted = jmpline.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        string jcaz = jmpline_splitted[0];
        string arg = jmpline_splitted[1];
        string jcazflagSyntax = "(JN?(C|A|Z|E|B|AE|BE))";
        string jmpSyntax = String.Format("({0} ({1}|{2}))", jcazflagSyntax, LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string jmpSyntaxVague = "jn?[a-z]+ (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C + "|" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L + ")";
        if (!Common.match(jmpline, jmpSyntax, true))
        {
            if (!Common.match(jmpline, jmpSyntaxVague, true)) return "invalid JumpIf arguement";
            else if (!Common.match(jcaz, jcazflagSyntax, true)) return "'" + jcaz + "' invalid JumpIf flags";
            else return evaluateArgs(arg);
        }
        return "";
    }
    private static string evaluatePUSH(string pushline)
    {
        string pushSyntax = String.Format("(push ({0}|{1}|{2}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.A, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string pushSyntaxVague = String.Format("push ({0}|{1}|{2})", VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A, VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C,
                                    VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L);
        if (!Common.match(pushline, pushSyntax, true))
        {
            if (!Common.match(pushline, pushSyntaxVague, true)) return "invalid PUSH arguement";
            else return evaluateArgs(
                pushline.Substring(pushline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }
    private static string evaluatePOP(string popline)
    {
        string popSyntax = String.Format("(pop ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.A);
        string popSyntaxVague = String.Format("pop ({0}|{1})", VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L,
                                VAGUE_LEXICON.SYNTAX.ARGUEMENTS.A);
        if (!Common.match(popline, popSyntax, true))
        {
            if (!Common.match(popline, popSyntaxVague, true)) return "invalid POP arguement";
            else return evaluateArgs(
                popline.Substring(popline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }
    private static string evaluateCALL(string callline)
    {
        string callSyntax = String.Format("(call ({0}|{1}))", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
        string callSyntaxVague = String.Format("(call ({0}|{1}))", VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L, VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C);
        if (!Common.match(callline, "ret", true) && !Common.match(callline, callSyntax, true))
        {
            if (!Common.match(callline, callSyntaxVague, true)) return "invalid CALL arguement";
            else return evaluateArgs(
                callline.Substring(callline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Length)
            );
        }
        return "";
    }
    private static string evaluateALU(string aluline)
    {
        string evaluateALU_Nomadic(string aluline)
        {
            string operation = aluline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
            string syntax = String.Format("((not|inc|dec|shl|shr) {0})", LEXICON.SYNTAX.ARGUEMENTS.R);
            if (!Common.match(aluline, syntax, true)) return "invalid " + operation.ToUpper() + " arguement";
            return "";
        }
        string evaluateALU_Dyadic(string aluline)
        {
            string operation = aluline.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
            string syntax = String.Format("(cmp|xor|and|or|not|shr|shl|div|mul|sub|add) {0},({0}|{1}|{2})", LEXICON.SYNTAX.ARGUEMENTS.R, NEW_LEXICON.SYNTAX.ARGUEMENTS.A, NEW_LEXICON.SYNTAX.ARGUEMENTS.C);
            string syntaxVague = String.Format("(cmp|xor|and|or|not|shr|shl|div|mul|sub|add) {0},{1}", VAGUE_LEXICON.SYNTAX.ARGUEMENTS.L, VAGUE_LEXICON.SYNTAX.ARGUEMENTS.X);
            if (!Common.match(aluline, syntax, true))
            {
                if (!Common.match(aluline, syntaxVague, true)) return "invalid " + operation.ToUpper() + " arguements";
                else return evaluateArgs(aluline.Substring(operation.Length));
            }
            return "";
        }

        string evaluation = "";
        if (Common.match(aluline, "^(not|inc|dec) "))
            evaluation = evaluateALU_Nomadic(aluline);
        else if (Common.match(aluline, "^(cmp|xor|and|or|div|mul|sub|add) "))
            evaluation = evaluateALU_Dyadic(aluline);
        else if (Common.match(aluline, "^(shl|shr)"))
            evaluation = (aluline.Contains(',') ? evaluateALU_Dyadic(aluline) : evaluateALU_Nomadic(aluline));
        return evaluation;
    }
    private static string evaluateDB(string dbline){
        const string dbSyntax = "(db (" + LEXICON.SYNTAX.ARGUEMENTS.C+"|(\".*\")))";
        const string dbVague = "(db (" + VAGUE_LEXICON.SYNTAX.ARGUEMENTS.C +"|(\".*\")))";
        string dbarg = dbline.Substring(2).Trim();
        if(!Common.match(dbline, dbSyntax, true)){
            if(!Common.match(dbline, dbVague, true)) return "'"+dbarg+"' invalid db arguement";
            else return evaluateArgs(dbarg);
        }
        return "";
    }
    private static string evaluateETC(string etcline){
        if(!Common.match(etcline,"(call|ret|hlt|clf)",true)) return "unrecognzied statement";
        else return "";
    }
    
    private static Func<string, string>[] lineEvaluatorFuncs = {
        evaluateMOV, evaluateJMP, evaluateJmpIf, evaluatePUSH, 
        evaluatePOP, evaluateCALL, evaluateALU, evaluateDB, evaluateETC
    };
    private static string[] lineStarterGrammars = {
        "^(mov )","^(jmp )","^(jn?[a-z]+ )","^(push )","^(pop )","((^(call ))|(^ret$))", 
        "^(cmp|xor|and|or|shr|shl|div|mul|sub|add|not|inc|dec) ","^db ",".*"
    };
    /// <summary> evaluates instructions' grammar. </summary>
    /// <returns> a detailed feedback of syntax error. Returns an empty string if there is none </returns>
    public static string evaluateLine(string line)
    {
        
        { // extract the instruction line
            byte numOfColons = 0; 
            for(int i = 0; i<line.Length; i++){ // to prevent line with more than 1 label 
                if(line[i]==';') break;
                if(line[i]==':') numOfColons++;
                if(numOfColons>1) return "only 1 label per line";
            }
            
            line = Common.replace(line, "((;.*)|(([a-z])((\\w)*)):)","").Trim();
            if(line.Length < 1) return "";
        }
        
        // if line matches the ith grammar, use the ith function 
        for(int i = 0; i<lineStarterGrammars.Length; i++)
            if(Common.match(line, lineStarterGrammars[i])) return lineEvaluatorFuncs[i](line);
        return "";
    }

    ///<summary> evaluates a multiline program including label declarations </summary> 
    public static string evaluateProgram(string linesOfCode)
    {
        string[] lines = linesOfCode.Split('\n');
        string codeEval = "";

        // Scanning Labels Phase
        NEW_LEXICON.TOKENS.labelsClear();
        for (int i = 0; i < lines.Length; i++){
            string temp = Common.getMatch(lines[i], "^"+VAGUE_LEXICON.TOKENS.LABEL+":").Value.Trim().Replace(":","");
            if(temp.Length<1) continue;
            string label_res = NEW_LEXICON.TOKENS.labelsAdd(temp);
            if(label_res.Length>0){
                codeEval = string.Concat(codeEval, String.Format("[Line {0}] {1}\n{2}\n", i+1, lines[i], label_res));
            }
        }

        // Grammar Evaluation Phase
        for (int i = (codeEval.Length>0?lines.Length:0); i < lines.Length; i++){
            string singleLine = lines[i].Trim();
            
            if ( singleLine.StartsWith(";") || singleLine.Length < 1 || 
                 (singleLine.EndsWith(":") && Common.match(singleLine, VAGUE_LEXICON.TOKENS.LABEL, true)) 
                ) 
                continue; // is a comment line or an empty line
            
            string lineEval = evaluateLine(singleLine);
            if (lineEval != "")
                codeEval = string.Concat(codeEval, String.Format("[Line {0}] {1}\n{2}\n", i+1, lines[i].Trim(), lineEval));
       
        }

        if (codeEval != "") codeEval = "SYNTAX ERROR(s)\n" + codeEval;
        return codeEval;
    }
}

}