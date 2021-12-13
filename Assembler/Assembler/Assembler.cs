﻿using System.Text.RegularExpressions;

namespace Assembler;

public static class Assembler
{
    public static class Terms
    {
        public const string Register = "([a-g]{1,2}|sp)";
        public const string Decimal = "(1[0-9]{0,2})|" + "(2(([0-4][0-9])|(5[0-6])|([0-9])))|" + "";
        public const string Address = "\\[" + Register + "\\+" + Decimal + "\\]";
    }
    public static bool match(string line, string pattern)
    {
        return Regex.Match(line, pattern, RegexOptions.IgnoreCase).Success;
    }
    public static string evaluate_MOV_R_R(string line)
    {
        if (!match(line.Substring(4), String.Format("^{0},{0}", Terms.Register)))
        {
            return "mov <reg> <reg>\n";
        }

        return "OK!";
    }
    public static byte[] TranslateLine(string line)
    {
        bool match = Regex.Match("mov a, c", "^mov [a-g]|sp, [a-g]|sp").Success;

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
