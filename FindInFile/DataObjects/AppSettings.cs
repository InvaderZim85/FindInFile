namespace FindInFile.DataObjects;

/// <summary>
/// Provides the application settings
/// </summary>
internal class AppSettings
{
    /// <summary>
    /// Gets or sets the base color
    /// </summary>
    public string BaseColor { get; set; } = "Dark";

    /// <summary>
    /// Gets or sets the theme color
    /// </summary>
    public string ThemeColor { get; set; } = "Blue";

    /// <summary>
    /// Gets or set the value which indicates if the last search should be saved
    /// </summary>
    public bool SaveLastSearch { get; set; } = true;
}