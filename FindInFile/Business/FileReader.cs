using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindInFile.DataObjects;
using Serilog;

namespace FindInFile.Business
{
    /// <summary>
    /// Provides helper functions for the interaction with files
    /// </summary>
    internal static class FileReader
    {
        /// <summary>
        /// The delegate of the progress event
        /// </summary>
        /// <param name="maxSteps">The max steps</param>
        /// <param name="currentStep">The current step</param>
        /// <param name="message">The message</param>
        public delegate void ProgressEvent(int maxSteps, int currentStep, string message);

        /// <summary>
        /// Occurs when a progress was made
        /// </summary>
        public static event ProgressEvent? Progress;

        /// <summary>
        /// Loads all files of the specified directories and checks if any of the file files contains the given search text
        /// </summary>
        /// <param name="directories">The list with the directories</param>
        /// <param name="fileSearchPattern">The file search pattern</param>
        /// <param name="searchText">The search text</param>
        /// <returns>The list with the files, which were found</returns>
        public static async Task<List<FileEntry>> Search(List<string> directories, string fileSearchPattern, string searchText)
        {
            static void PrintProgress(int maxCount, int count, string file)
            {
                var maxLength = maxCount.ToString().Length;

                var message = $"{count.ToString().PadLeft(maxLength, '0')} of {maxCount} - {Path.GetFileName(file)}";
                Progress?.Invoke(maxCount, count, message);
            }

            var files = LoadFiles(directories, fileSearchPattern);

            var result = new List<FileEntry>();

            var count = 1;
            var tmpFiles = files.Distinct().ToList();
            foreach (var file in files.Distinct())
            {
                var fileResult = await CheckFile(file, searchText);
                if (fileResult != null)
                    result.Add(fileResult);

                PrintProgress(tmpFiles.Count, count++, file);
            }

            return result.OrderBy(o => o.Path).ThenBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Loads all affected files of the specified directories
        /// </summary>
        /// <param name="directories">The list with the directories</param>
        /// <param name="fileSearchPattern">The search pattern</param>
        /// <returns>The list with the files</returns>
        private static List<string> LoadFiles(List<string> directories, string fileSearchPattern)
        {
            var patterns = fileSearchPattern.Split(new[] {";", ","}, StringSplitOptions.TrimEntries);

            var result = new List<string>();

            foreach (var pattern in patterns)
            {
                foreach (var directory in directories)
                {
                    result.AddRange(Directory.GetFiles(directory, pattern, SearchOption.AllDirectories));
                }
            }

            return result;
        }

        /// <summary>
        /// Checks the specified file
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="searchText">The search text</param>
        /// <returns>The file which contains the search text</returns>
        private static async Task<FileEntry?> CheckFile(string filePath, string searchText)
        {
            try
            {
                var result = new FileEntry
                {
                    Name = Path.GetFileName(filePath),
                    Path = filePath
                };

                var content = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);

                var lineCount = 1;
                foreach (var entry in content)
                {
                    if (entry.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        result.Occurence.Add(new OccurenceEntry
                        {
                            Line = lineCount,
                            SelectionStart = entry.IndexOf(searchText, StringComparison.OrdinalIgnoreCase)
                        });

                    lineCount++;
                }

                if (!result.Occurence.Any()) 
                    return null;

                result.Content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                return result;
            }
            catch
            {
                Log.Warning("Can't load / check file '{path}'", filePath);
                return null;
            }
        }
    }
}
