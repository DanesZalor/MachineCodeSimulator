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

    private static string removeComments(string[] lines)
    {
        string removedComments = "";
        foreach (string line in lines) removedComments += line.Split(';')[0].Trim() + "\n";
        return removedComments;
    }

    private static string replaceAliases(string[] lines)
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
            {String.Format("dec {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"dec <REG>"},
            // sh[lr] r, 1
            {String.Format("shl {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"shl <REG>"},
            {String.Format("shr {0},{1}1", LEXICON.SYNTAX.ARGUEMENTS.R, LEXICON.SPACE),"shr <REG>"},
        };
        string newLines = "";
        foreach (string line in lines)
        {
            string newLine = line.Trim();
            for (int i = 0; i < 17; i++)
            {
                if (match(newLine, Aliases[i, 0]))
                {
                    newLine = Regex.Replace(newLine, Aliases[i, 0], Aliases[i, 1]);
                    if (i >= 13)
                        newLine = Regex.Replace(newLine, "<REG>", getMatch(line, LEXICON.SYNTAX.ARGUEMENTS.R).Value);
                    break;
                }
            }
            newLines += newLine + "\n";
        }
        return newLines.Trim();
    }

    public static string replaceLabels(string[] lines)
    {
        // Scan-Labels
        string labels = "";
        string labelCoords = "";
        foreach (string line in lines)
        {
            if (match(line, "([a-z](\\w)*)")) { }
        }
        return "";
    }

    public static string translateAlias(string linesOfCode)
    {
        string[] lines = linesOfCode.ToLower().Replace(":", ":\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string removedComments = removeComments(lines);
        string replacedAliases = replaceAliases(removedComments.Split('\n'));
        return replacedAliases;
    }
}