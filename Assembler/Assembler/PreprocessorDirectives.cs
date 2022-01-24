using System.Text.RegularExpressions;

namespace Assembler;

/// <summary> Pretranslation that happens before compilation. In this stage, comments are removed, aliases (such as Jxxx) are translated, 
/// and labels are converted to constants. Before this phase, the custom written program should already be syntactically correct </summary>
public static class PreprocessorDirectives
{
    private static Match getMatch(string line, string pattern, bool exact = false, bool inverse = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        if (inverse) pattern = "\\[^" + pattern + "\\]";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }
    private static bool match(string line, string pattern, bool exact = false) { return getMatch(line, pattern, exact).Success; }

    private static string removeExcessWhitespace(string linesOfCode){
        // replace all consecutive spaces with a single space
        string newLines = linesOfCode;
        
        // replace all consecutive (2 or more) new lines with a single new line 
        newLines = new string(Regex.Replace(newLines,"(\n){2,}","\n"));
        
        // remove all consecutive (2 or more) spaces with a single space
        newLines = new string(Regex.Replace(newLines, "( ){2,}"," "));

        newLines = new string(Regex.Replace(newLines, " ,", ", "));

        // do it again cuz of the possibility of the previous replacement
        newLines = new string(Regex.Replace(newLines, "( ){2,}"," "));
        
        return newLines;
    }

    private static string replaceAliases(string linesOfCode)
    {
        string[,] Aliases = new string[19, 2]{
            {"jnc ","jaz "},{"jna ","jcz "},{"jnz ","jca "},
            {"je ","jz "},{"jne ","jca "},{"jb ","jc "},
            {"jnb ","jaz "},{"jae ","jaz "},{"jnae ","jc "},
            {"jbe ","jcz "},{"jnbe ","ja "},
            // mul/div by 1 is removed
            {String.Format("(mul|div) {0},{1}1",LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),""},
            // add/sub by 0 is removed
            {String.Format("(add|sub) {0},{1}0",LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),""},
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

        using (var reader = new StringReader(linesOfCode))
        {
            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                string newLine = new string(line.Split(';')[0].Trim());
                for (int i = 0; i < 19; i++)
                {
                    if (match(newLine, Aliases[i, 0]))
                    {
                        newLine = Regex.Replace(newLine, Aliases[i, 0], Aliases[i, 1]);
                        if(i>17){
                            string binary = getMatch(line, " (0b([01]{1,8}))").Value.Trim();
                            int dec = 0;
                            byte mul = 0b1;
                            for(int b = binary.Length-1; b>1; b--){
                                dec += mul*(Convert.ToByte(binary[b]) - 48);
                                mul = Convert.ToByte(mul<<1);
                            }
                            newLine = Regex.Replace(newLine, "<BIN>", Convert.ToString(dec));
                        }
                        else if(i>16){ // hex alias to decimal
                            int convertHexToDec(char s){
                                int r = Convert.ToInt16(s);
                                if (r>96 && r<103) return r - 87;
                                else if (r>47 && r<58) return r - 48;
                                else return -1;
                            }
                            string hex = getMatch(line, " (0x([0-9]|[a-f]){1,2})").Value.Trim();
                            newLine = Regex.Replace(newLine, "<HEX>", Convert.ToString(
                                convertHexToDec(hex[2])*16 + convertHexToDec(hex[3])
                            ));
                        }
                        else if (i >= 13)
                            newLine = Regex.Replace(newLine, "<REG>", getMatch(line, " "+LEXICON.TOKENS.REGISTER).Value.Trim());
                        

                        break;
                    }
                }
                if(newLine.Length>0) newLines = new string(string.Concat(newLines,newLine.Trim()+"\n"));
            }
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
        // Scan-Labels
        string labels = "";
        string labelCoords = "";

        // gather labels and coordinates
        using (var reader = new StringReader(linesOfCode))
        {
            const string cost1 = "^((clf|ret)|"+ 
                "((not|shl|shr|inc|dec|call|pop|push|jmp|jca?z?|jc?az?|jc?a?z) "+
                LEXICON.SYNTAX.ARGUEMENTS.R + ")|(db "+LEXICON.SYNTAX.ARGUEMENTS.C+"))$";
            const string cost3 = "^(cmp|xor|not|and|or|shl|shr|div|mul|sub|add)";
            const string constX = "^(db \".*\")&";
            int coordCounter = 0;

            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (line.Contains(':')) { // if label, record it's coordinate
                    labels = new string(string.Concat(labels, (labels.Length>0?"|":"") + line.Split(':')[0] ));
                    labelCoords = new string(string.Concat(labelCoords, (labelCoords.Length>0?",":"") + Convert.ToString(coordCounter) ));
                }else{ // else, add to the counter depending on the scanned instruction
                    if(match(line, cost1)) coordCounter += 1;
                    else if(match(line, cost3)) coordCounter += 3;
                    else if(match(line, constX))
                        coordCounter += line.Substring(line.IndexOf('\"')).Length - 2;
                    else coordCounter += 2;
                }
            }
        }
        string[] labelsArray = labels.Split('|', StringSplitOptions.RemoveEmptyEntries);
        string[] coordsArray = labelCoords.Split(',',StringSplitOptions.RemoveEmptyEntries);
        string newLines = linesOfCode;
    
        // remove all ":"
        newLines = new string(Regex.Replace(newLines,".*:","")); 

        // replace every label with their corresponding coordinate
        for(int i = 0; i<labelsArray.Length; i++)
            newLines = Regex.Replace(newLines,"\\b"+labelsArray[i]+"\\b", coordsArray[i]);
        return newLines.Trim();
    }

    public static string translateAlias(string linesOfCode)
    {
        // lowercase the entire code
        linesOfCode = new string(linesOfCode.ToLower()); 
        // separate label declarations into different lines
        linesOfCode = new string(linesOfCode.Replace(":",":\n")); 

        linesOfCode = replaceAliases(linesOfCode);
        linesOfCode = replaceLabels(linesOfCode);
        linesOfCode = removeExcessWhitespace(linesOfCode);
        return linesOfCode;
    }
}