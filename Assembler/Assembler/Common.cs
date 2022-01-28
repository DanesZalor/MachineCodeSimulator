namespace Assembler;
using System.Text.RegularExpressions;

public static class Common
{
    public static Match getMatch(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }
    public static bool match(string line, string pattern, bool exact = false)
    {
        return getMatch(line, pattern, exact).Success;
    }
}
