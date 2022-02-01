using System;
using System.IO;
namespace AssemblerTest;

public class Common{
    /// <summary> reads a file assuming ../../../TestCases/filename exists </summary>
    public static string readFile(string filename){
        string path = Path.Combine(
            System.IO.Directory.GetCurrentDirectory(), 
            ".." + Path.DirectorySeparatorChar + 
            ".." + Path.DirectorySeparatorChar + 
            ".." + Path.DirectorySeparatorChar + 
            "TestCases/" + filename
        );
        return new string(System.IO.File.ReadAllText(path));
    }
}