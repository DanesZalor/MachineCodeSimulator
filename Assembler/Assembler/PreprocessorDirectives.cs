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

    private static string replaceAliases(string linesOfCode)
    {
        string[,] Aliases = new string[17, 2]{
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
        };
        string newLines = "";

        using (var reader = new StringReader(linesOfCode))
        {
            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                string newLine = new string(line.Split(';')[0].Trim());
                for (int i = 0; i < 17; i++)
                {
                    if (match(newLine, Aliases[i, 0]))
                    {
                        newLine = Regex.Replace(newLine, Aliases[i, 0], Aliases[i, 1]);
                        if (i >= 13)
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
    */
    private static string replaceLabels(string linesOfCode)
    {
        // Scan-Labels
        string labels = "";
        string labelCoords = "";

        // gather labels and coordinates
        using (var reader = new StringReader(linesOfCode))
        {
            string cost1 = "^((clf|ret)|"+
                String.Format(
                    "((not|shl|shr|inc|dec|call|pop|push|jmp|jca?z?|jc?az?|jc?a?z) {0})|",LEXICON.SYNTAX.ARGUEMENTS.R
                ) + String.Format(
                    "(db {0})|", LEXICON.SYNTAX.ARGUEMENTS.C
                )+
            ")$";
            string cost3 = "^(cmp|xor|not|and|or|shl|shr|div|mul|sub|add)";
            string constX = "^(db \".*\")&";
            int coordCounter = 0;

            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (line.Contains(':')) { 
                    labels = new string(string.Concat(labels, (labels.Length>0?"|":"") + line.Split(':')[0] ));
                    labelCoords = new string(string.Concat(labelCoords, (labelCoords.Length>0?",":"") + Convert.ToString(coordCounter) ));
                }else{
                    if(match(line, cost1)) coordCounter += 1;
                    else if(match(line, cost3)) coordCounter += 3;
                    else if(match(line, constX))
                        coordCounter += line.Substring(line.IndexOf('\"')).Length - 2;
                    else coordCounter += 2;
                }
            }
        }
        //Console.WriteLine("labels "+labels);Console.WriteLine("coords "+labelCoords);
        string[] labelsArray = labels.Split('|', StringSplitOptions.RemoveEmptyEntries);
        string[] coordsArray = labelCoords.Split(',',StringSplitOptions.RemoveEmptyEntries);
        string newLines = linesOfCode;
    
        newLines = new string(Regex.Replace(newLines,".*:",""));
        newLines = new string(Regex.Replace(newLines,"(\n){2,}","\n"));
        for(int i = 0; i<labelsArray.Length; i++)
            newLines = Regex.Replace(newLines,"\\b"+labelsArray[i]+"\\b", coordsArray[i]);
        return newLines.Trim();
    }

    public static string translateAlias(string linesOfCode)
    {
        linesOfCode = new string(linesOfCode.ToLower()); // lowercase the entire code
        linesOfCode = new string(linesOfCode.Replace(":",":\n")); // separate label declarations into different lines
        //Console.WriteLine("OG:\n----------\n"+linesOfCode);
        string replacedAliases = replaceAliases(linesOfCode);
        //Console.WriteLine("replaceAliases:\n----------\n"+replacedAliases);
        string replacedLabels = replaceLabels(replacedAliases);
        //Console.WriteLine("replaceLabels:\n----------\n"+replacedLabels);
        return replacedLabels;
    }
}