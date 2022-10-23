using System;
using System.Diagnostics;
using System.IO;

namespace Rayman1Randomizer;

public static class ProcessHelpers
{
    public static string GetStringAsPathArg(string filePath) => $"\"{filePath.Replace('/', '\\')}\"";

    public static void RunProcess(
        string filePath, 
        string[] args, 
        string? workingDir = null, 
        bool waitForExit = true, 
        bool logInfo = true)
    {
        // Create the process and dispose when finished
        using Process p = new();

        // Set the start info
        p.StartInfo = new ProcessStartInfo(filePath, String.Join(" ", args))
        {
            UseShellExecute = !logInfo,
            RedirectStandardOutput = logInfo,
            WorkingDirectory = workingDir ?? Path.GetDirectoryName(filePath) ?? String.Empty
        };

        p.Start();

        if (logInfo) {
           string output = p.StandardOutput.ReadToEnd();
           Debug.WriteLine($"Process output:");
           Debug.WriteLine(output);
        }
        
        if (waitForExit)
            p.WaitForExit();
    }
}