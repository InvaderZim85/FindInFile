using System.Collections.Generic;
using System.Linq;

namespace FindInFile.DataObjects
{
    /// <summary>
    /// Represents a single file
    /// </summary>
    internal class FileEntry
    {
        /// <summary>
        /// Gets or sets the name of the file
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the path of the file
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Gets or sets the list with the line numbers in which the search text occurs.
        /// </summary>
        public List<OccurenceEntry> Occurence { get; set; } = new();

        /// <summary>
        /// Gets the list with the lines
        /// </summary>
        public string Lines => string.Join(", ", Occurence.Select(s => s.Line));

        /// <summary>
        /// Gets or sets the content of the file
        /// </summary>
        public string Content { get; set; } = "";
    }
}
