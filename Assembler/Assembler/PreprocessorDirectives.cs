using System;

using System.IO;
using System.Text.RegularExpressions;

namespace Assembler{

/// <summary> Pretranslation that happens before compilation. In this stage, comments are removed, aliases (such as Jxxx) are translated, 
/// and labels are converted to constants. Before this phase, the custom written program should already be syntactically correct </summary>
public static class PreprocessorDirectives
{
    private static string removeExcessWhitespace(string linesOfCode){
        // replace all consecutive spaces with a single space
        string newLines = linesOfCode;
        
        string[,] grammarReplace = {
            {"(\n){2,}","\n"},      // replace all consecutive (2 or more) new lines with a single new line 
            {"( ){2,}"," "},        // replace all consecutive (2 or more) spaces with a single space
            {"(\\[( ){1,})","["},   // remove extra spaces in []
            {"(( ){1,}\\])","]"}
        }; 
        for(int i = 0; i<4; i++)
            newLines = Common.replace(newLines,grammarReplace[i,0],grammarReplace[i,1]);
    
        //foreach(char c in "+-,") newLines = String.Join(c, newLines.Split(new char[1]{c}, StringSplitOptions.TrimEntries));
        // net6 compatible

        // net472 compatible
        foreach(char c in "+-,"){
            string[] newLinesArr = newLines.Split(new char[1]{c});
            
            for(int x=0; x<newLinesArr.Length; x++)
                newLinesArr[x] = newLinesArr[x].Trim();
            
            newLines = String.Join(Convert.ToString(c), newLinesArr);
        }


        // removes all white space around ","
        //newLines = String.Join(", ",newLines.Split(',', StringSplitOptions.TrimEntries)); 
        return newLines;
    }

    public static string replaceAliases(string linesOfCode)
    {
        string[,] Aliases = new string[20, 2]{
            {"jnc ","jaz "},{"jna ","jcz "},{"jnz ","jca "},
            {"je ","jz "},{"jne ","jca "},{"jb ","jc "},
            {"jnb ","jaz "},{"jae ","jaz "},{"jnae ","jc "},
            {"jbe ","jcz "},{"jnbe ","ja "},
            // mul/div by 1 is removed
            {String.Format("(mul|div) {0},{1}1",LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),""},
            // add/sub by 0 is removed
            {String.Format("(add|sub) {0},{1}0",LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),""},
            {String.Format("mov (({0}a{0},{0}a)|({0}b{0},{0}b)|({0}c{0},{0}c)|({0}d{0},{0}d)|({0}e{0},{0}e)|({0}f{0},{0}f)|({0}g{0},{0}g)|({0}sp{0},{0}sp))", LEXICON.SPACE),""},
            //special cases
            // add r, 1 -> inc r
            {String.Format("add {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"inc <REG>"},
            // sub r, 1 -> dec r
            {String.Format("sub {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"dec <REG>"},
            // sh[lr] r, 1
            {String.Format("shl {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"shl <REG>"},
            {String.Format("shr {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"shr <REG>"},
            //special case : i17
            {"(0x([0-9]|[a-f]){1,2})","<HEX>"},
            //special case : i18
            {"(0b([01]{1,8}))","<BIN>"}
        };

        string newLines = "";
        string[] linesOfCodeArr = linesOfCode.Split('\n');
            
            for (int l=0; l<linesOfCodeArr.Length; l++)
            {
                string line = linesOfCodeArr[l];
                // removes the comments
                string newLine = Common.replace(line, ";.*","").Trim();
                for (int i = 0; i < 20; i++)
                {
                    if (Common.match(newLine, Aliases[i, 0]))
                    {
                        newLine = Common.replace(newLine, Aliases[i, 0], Aliases[i, 1]);
                        if(i>18){
                            string binary = Common.getMatch(line, " (0b([01]{1,8}))").Value.Trim();
                            int dec = 0;
                            byte mul = 0b1;
                            for(int b = binary.Length-1; b>1; b--){
                                dec += mul*(System.Convert.ToByte(binary[b]) - 48);
                                mul = System.Convert.ToByte(mul<<1);
                            }
                            newLine = Common.replace(newLine, "<BIN>", System.Convert.ToString(dec));
                        }
                        else if(i>17){ // hex alias to decimal
                            int convertHexToDec(char s){
                                int r = System.Convert.ToInt16(s);
                                if (r>96 && r<103) return r - 87;
                                else if (r>47 && r<58) return r - 48;
                                else return -1;
                            }
                            string hex = Common.getMatch(line, " (0x([0-9]|[a-f]){1,2})").Value.Trim();
                            newLine = Common.replace(newLine, "<HEX>", System.Convert.ToString(
                                convertHexToDec(hex[2])*16 + convertHexToDec(hex[3])
                            ));
                        }
                        else if (i >= 14){
                            string reg = Common.getMatch(line.Trim(), " ("+LEXICON.SYNTAX.ARGUEMENTS.R+"( )*,)").Value.Trim().Replace(",","");
                            newLine = Common.replace(newLine, "<REG>", reg );
                            //newLine = Common.replace(newLine, "<REG>", Common.getMatch(line, " (a|b|c|d|e|f|g|sp)(,|( ){1,})").Value.Trim() );
                        }
                        break;
                    }
                }
                if(newLine.Length>0) newLines = string.Concat(newLines,newLine.Trim()+"\n");
            }
        
        return newLines.Trim();
    }

    /** at this point:
    * - each label should have it's separate line already
    * - all label declarations are already correct
    * - all label references have been declared
    * - all aliases are replaced
    */
    private static string replaceLabels(string linesOfCode)
    {
        int getCostOfLine(string line){
            const string cost1 = "^((clf|ret|hlt)|"+ 
                "((not|shl|shr|inc|dec|call|pop|push|jmp|jca?z?|jc?az?|jc?a?z) "+
                LEXICON.SYNTAX.ARGUEMENTS.R + ")|(db "+LEXICON.SYNTAX.ARGUEMENTS.C+"))$";
            const string cost2 = "(^(mov|jmp|jca?z?|jc?az?|jc?a?z|push|pop|call))|"+
                                    "(^(cmp|xor|not|and|or|shl|shr|div|mul|sub|add) "+
                                        LEXICON.SYNTAX.ARGUEMENTS.R+","+LEXICON.SYNTAX.ARGUEMENTS.R+")";
            const string constX = "^(db \".*\")&";

            if(Common.match(line, cost1)) return 1;
            else if(Common.match(line, cost2)) return 2;
            else if(Common.match(line, constX))
                return line.Substring(line.IndexOf('\"')).Length - 2;
            else return 3;
        }

        // Scan-Labels
        string labels = "";
        string labelCoords = "";

        
        // gather labels and coordinates
        {
            int coordCounter = 0;
            string[] linesOfCodeArr = linesOfCode.Split('\n');
            for (int i=0; i<linesOfCodeArr.Length; i++)
            {
                string line = linesOfCodeArr[i];
                if (line.Contains(':')) { // if label, record it's coordinate
                    labels = string.Concat(labels, (labels.Length>0?"|":"") + line.Split(':')[0] );
                    labelCoords = string.Concat(labelCoords, (labelCoords.Length>0?",":"") + System.Convert.ToString(coordCounter) );
                }else{ // else, add to the counter depending on the scanned instruction
                    coordCounter += getCostOfLine(line);
                }
            }
        }
        
        string[] labelsArray = labels.Split(new char[1]{'|'}, StringSplitOptions.RemoveEmptyEntries);
        string[] coordsArray = labelCoords.Split(new char[1]{','},StringSplitOptions.RemoveEmptyEntries);
        string newLines = linesOfCode;
    
        // remove all ":"
        newLines = Common.replace(newLines,".*:",""); 

        // replace every label with their corresponding coordinate
        for(int i = 0; i<labelsArray.Length; i++)
            newLines = Common.replace(newLines,"\\b"+labelsArray[i]+"\\b", coordsArray[i]);
        return newLines.Trim();
    }

    public static string translateAlias(string linesOfCode)
    {
        // lowercase the entire coded
        linesOfCode = linesOfCode.ToLower(); 

        // separate label declarations into different lines
        linesOfCode = linesOfCode.Replace(":",":\n"); 

        try{
            linesOfCode = replaceAliases(linesOfCode); // System.TypeLoadException
            linesOfCode = replaceLabels(linesOfCode);
            linesOfCode = removeExcessWhitespace(linesOfCode);
        }catch(System.TypeLoadException e){ // catch System.TypeLoadException from Converts
            return "";
        }
        
        return linesOfCode;
    }
}

}