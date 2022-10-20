using System;
using System.IO;

namespace Rayman1Randomizer;

/// <summary>
/// A temporary file
/// </summary>
public sealed class TempFile : IDisposable
{
    /// <summary>
    /// Creates a new temporary file
    /// </summary>
    public TempFile()
    {
        // Get the temp path and create the file
        TempPath = Path.GetTempFileName();

        // Get the file info
        var info = new FileInfo(TempPath);

        // Set the attribute to temporary
        info.Attributes |= FileAttributes.Temporary;
    }

    /// <summary>
    /// The path of the temporary file
    /// </summary>
    public string TempPath { get; }

    public Stream OpenRead() => File.OpenRead(TempPath);

    /// <summary>
    /// Removes the temporary file
    /// </summary>
    public void Dispose()
    {
        // Delete the temp file
        File.Delete(TempPath);
    }
}