using System;
using System.IO;
namespace AssemblerTest;

public class Common{
    private static string folderpath = Path.Combine(
            System.IO.Directory.GetCurrentDirectory(), 
            ".." + Path.DirectorySeparatorChar + 
            ".." + Path.DirectorySeparatorChar + 
            ".." + Path.DirectorySeparatorChar + 
            "TestCases" + Path.DirectorySeparatorChar
        );

    /// <summary> reads a file assuming ../../../TestCases/filename exists </summary>
    public static string readFile(string filename){
        string path = Path.Combine(folderpath, filename);
        return new string(System.IO.File.ReadAllText(path));
    }

    public static byte[] readFileBytes(string filename){
        string path = Path.Combine(folderpath, filename);
        return System.IO.File.ReadAllBytes(path);
    }
}