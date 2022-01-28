namespace Assembler;
using System.Text.RegularExpressions;

/// <summary> Contains common stuff </summary>
public static class Common
{
    /// <summary> get matches from line that satisfy the grammar pattern </summary>
    public static Match getMatch(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    }

    /// <summary> returns true if line satisfy the grammar pattern </summary>
    public static bool match(string line, string pattern, bool exact = false)
    {
        return getMatch(line, pattern, exact).Success;
    }
}
