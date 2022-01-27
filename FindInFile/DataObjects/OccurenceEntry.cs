namespace FindInFile.DataObjects
{
    /// <summary>
    /// Represents an occurence entry
    /// </summary>
    internal class OccurenceEntry
    {
        /// <summary>
        /// Gets or sets the line number
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the index where the search text starts
        /// </summary>
        public int SelectionStart { get; set; }
    }
}
