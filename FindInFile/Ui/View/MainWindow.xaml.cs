using System.Windows;
using FindInFile.DataObjects;
using FindInFile.Ui.ViewModel;
using MahApps.Metro.Controls;

namespace FindInFile.Ui.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    /// <summary>
    /// Contains the length of the search text
    /// </summary>
    private int _searchTextLength;

    /// <summary>
    /// Creates a new instance of the <see cref="MainWindow"/>
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the text of the editor
    /// </summary>
    /// <param name="text">The text which should be set</param>
    /// <param name="searchText">The search text</param>
    private void SetText(string text, string searchText)
    {
        TextEditor.Document.Text = text;
        _searchTextLength = searchText.Length;
    }

    /// <summary>
    /// Selected the desired line
    /// </summary>
    /// <param name="occurence">The entry which should be selected</param>
    private void SelectLine(OccurenceEntry occurence)
    {
        // Set the focus
        TextEditor.Focus();

        // Scroll to the desired line
        TextEditor.ScrollToLine(occurence.Line);

        // Get the offset of the line
        var document = TextEditor.Document;
        var line = document.GetLineByNumber(occurence.Line);
        var offset = line.Offset;

        // Add the offset to the selection start
        TextEditor.Select(offset + occurence.SelectionStart, _searchTextLength);
    }

    /// <summary>
    /// Init the flyout
    /// </summary>
    private void InitFlyOut()
    {
        SettingsControl.InitControl();
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="MainWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Init the editor
        TextEditor.Options.HighlightCurrentLine = true;

        if (DataContext is MainWindowViewModel viewModel)
            viewModel.InitViewModel(SetText, SelectLine, InitFlyOut);
    }

    /// <summary>
    /// Occurs when the fly out was closed
    /// </summary>
    /// <param name="sender">The settings fly out</param>
    /// <param name="e">The event arguments</param>
    private void Flyout_OnClosingFinished(object sender, RoutedEventArgs e)
    {
        Helper.SetColorTheme();
    }
}