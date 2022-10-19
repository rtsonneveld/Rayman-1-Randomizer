using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rayman1Randomizer {
    static class FileUtil {

        /// <summary>
        /// Reads all lines but with fileshare.readwrite permission
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>All lines of the file</returns>
        public static string[] ReadAllLines(string path)
        {
            using var files = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(files);

            var lines = new List<string>();
            while (!sr.EndOfStream) {
                lines.Add(sr.ReadLine());
            }

            return lines.ToArray();
        }

        public static void CloneDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root)) {
                string dirName = Path.GetFileName(directory);
                if (!Directory.Exists(Path.Combine(dest, dirName))) {
                    Directory.CreateDirectory(Path.Combine(dest, dirName));
                }
                CloneDirectory(directory, Path.Combine(dest, dirName));
            }

            foreach (var file in Directory.GetFiles(root)) {

                var destFile = Path.Combine(dest, Path.GetFileName(file));
                if (File.Exists(destFile)) {
                    File.Delete(destFile);
                }
                File.Copy(file, destFile);
            }
        }
    }
}
