//Created by Alexander Fields
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class Program
{
    static void Main(string[] args)
    {
        string exePath = System.AppDomain.CurrentDomain.BaseDirectory;
        string pathFile = System.IO.Path.Combine(exePath, "path.txt");

        if (!System.IO.File.Exists(pathFile))
        {
            System.Console.WriteLine("path.txt not found in the application directory.");
            return;
        }

        string rootPath = System.IO.File.ReadAllText(pathFile).Trim();

        if (string.IsNullOrEmpty(rootPath) || !System.IO.Directory.Exists(rootPath))
        {
            System.Console.WriteLine("Invalid directory path in path.txt.");
            return;
        }

        List<string> modifiedFiles = new List<string>();

        Parallel.ForEach(System.IO.Directory.GetFiles(rootPath, "*.cs", System.IO.SearchOption.AllDirectories), file =>
        {
            if (ProcessFile(file))
            {
                lock (modifiedFiles)
                {
                    modifiedFiles.Add(file);
                }
            }
        });

        System.Console.WriteLine("Modified files:");
        foreach (string file in modifiedFiles)
        {
            System.Console.WriteLine(Path.GetFileName(file));
        }
    }

    static bool ProcessFile(string filePath)
    {
        string content = System.IO.File.ReadAllText(filePath);

        // Regex pattern to match classes and variables without a summary above them
        // This pattern also captures attributes above the class or variable declaration
        string pattern = @"(?<!/// <summary>[\s\S]*?/// </summary>\s*)((\s*\[.*\]\s*)+)(public|private|protected|internal)(\s+static)?\s+(class|[\w<>]+\s+[\w]+(?=\s*(;|=|\{)))";

        // Replacement pattern to add the summary
        string replacement = @"/// <summary>
    /// 
    /// </summary>
    $1$3$4 $5";

        string newContent = Regex.Replace(content, pattern, replacement);

        if (newContent != content)
        {
            System.IO.File.WriteAllText(filePath, newContent);
            return true;
        }

        return false;
    }
}
