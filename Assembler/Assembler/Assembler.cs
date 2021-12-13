using System.Text.RegularExpressions;

namespace Assembler;

public static class Assembler
{

    /// <summary> Regex constants for terms Register, Decimal, Offset, Address. Can be used for Regex.Match() </summary>
    public static class Terms
    {
        public const string Register = "([a-g]{1,2}|sp)";
        public const string Decimal = "(" +
                                        "(1[0-9]{0,2})|" +  // 1, 10-19, 100-199
                                        "([1-9][0-9]?)|" +  // 1-99
                                        "(2[0-4][0-9])|" +  // 200 - 249
                                        "(25[0-5])" +       // 250-255
                                    ")";
        public const string Offset = "(" +
                                        "(\\+([1-9]|(1[1-5])))|" +  // +1 to +15
                                        "(-([1-9]|(1[1-6])))" +     // -1 to -16
                                    ")";
        public const string Address = "\\[(" +
                                        Register + "|" +    // [Register]
                                        Decimal + "|" +     // [Decimal]
                                        "(" +               // [Register+Offset] or [Register-Offset]
                                            Register + Offset +
                                        ")" +
                                    ")\\]";
    }


    public static bool match(string line, string pattern, bool exact = false)
    {
        if (exact) pattern = "^" + pattern + "$";
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase).Success;
    }

    public static byte[] TranslateLine(string line)
    {

        return new byte[1];
    }
    private static string RemoveComments(string code)
    {
        return "";
    }
    public static byte[] Compile(string code)
    {

        return new byte[1];
    }


    public static int test()
    {
        return 1;
    }
}
