using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using FindInFile.Business;
using FindInFile.DataObjects;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using ZimLabs.WpfBase.NetCore;
using Help = System.Windows.Forms.Help;

namespace FindInFile.Ui.ViewModel;

/// <summary>
/// Provides the logic for the <see cref="View.MainWindow"/>
/// </summary>
internal sealed class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The action 
    /// </summary>
    private Action? _initFlyOut;

    /// <summary>
    /// Contains the index of the current occurence
    /// </summary>
    private int _currentOccurence;

    /// <summary>
    /// The action to set the text of the editor
    /// </summary>
    private Action<string, string>? _setText;

    /// <summary>
    /// The action to select the specified line
    /// </summary>
    private Action<OccurenceEntry>? _selectLine;

    /// <summary>
    /// Backing field for <see cref="DirectoryList"/>
    /// </summary>
    private ObservableCollection<string> _directoryList = new();

    /// <summary>
    /// Gets or sets the list with the directories
    /// </summary>
    public ObservableCollection<string> DirectoryList
    {
        get => _directoryList;
        set => SetField(ref _directoryList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedDirectory"/>
    /// </summary>
    private string _selectedDirectory = "";

    /// <summary>
    /// Gets or sets the selected directory
    /// </summary>
    public string SelectedDirectory
    {
        get => _selectedDirectory;
        set => SetField(ref _selectedDirectory, value);
    }

    /// <summary>
    /// Backing field for <see cref="FilePattern"/>
    /// </summary>
    private string _filePattern = "*.*";

    /// <summary>
    /// Gets or sets the file pattern (needed for the search)
    /// </summary>
    public string FilePattern
    {
        get => _filePattern;
        set => SetField(ref _filePattern, value);
    }

    /// <summary>
    /// Backing field for <see cref="SearchText"/>
    /// </summary>
    private string _searchText = "";

    /// <summary>
    /// Gets or sets the search text
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    /// <summary>
    /// Backing field for <see cref="SearchResult"/>
    /// </summary>
    private ObservableCollection<FileEntry> _searchResult = new();

    /// <summary>
    /// Gets or sets the list with the found values
    /// </summary>
    public ObservableCollection<FileEntry> SearchResult
    {
        get => _searchResult;
        set => SetField(ref _searchResult, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedEntry"/>
    /// </summary>
    private FileEntry? _selectedEntry;

    /// <summary>
    /// Gets or sets the selected file entry
    /// </summary>
    public FileEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (!SetField(ref _selectedEntry, value)) 
                return;

            // Set the text and jump to the first occurrence
            _setText?.Invoke(value?.Content ?? "", SearchText);
            _selectLine?.Invoke(value?.Occurence.FirstOrDefault() ?? new OccurenceEntry());
            _currentOccurence = 0;
        }
    }

    /// <summary>
    /// Backing field for <see cref="ResultInfo"/>
    /// </summary>
    private string _resultInfo = "Result";

    /// <summary>
    /// Gets or sets the info text
    /// </summary>
    public string ResultInfo
    {
        get => _resultInfo;
        set => SetField(ref _resultInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="AppInfo"/>
    /// </summary>
    private string _appInfo = "FindInFile";

    /// <summary>
    /// Gets or sets the application info
    /// </summary>
    public string AppInfo
    {
        get => _appInfo;
        set => SetField(ref _appInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="SettingsOpen"/>
    /// </summary>
    private bool _settingsOpen;

    /// <summary>
    /// Gets or sets the value which indicates if the settings fly out is open
    /// </summary>
    public bool SettingsOpen
    {
        get => _settingsOpen;
        set
        {
            SetField(ref _settingsOpen, value);

            switch (value)
            {
                case true:
                    _initFlyOut?.Invoke();
                    break;
            }
        }
    }

    /// <summary>
    /// Backing field for <see cref="IncludeFileName"/>
    /// </summary>
    private bool _includeFileName;

    /// <summary>
    /// Gets or sets the value which indicates if the file name should be included into the search
    /// </summary>
    public bool IncludeFileName
    {
        get => _includeFileName;
        set => SetField(ref _includeFileName, value);
    }

    /// <summary>
    /// The command to add a new directory
    /// </summary>
    public ICommand AddCommand => new DelegateCommand(AddDirectory);

    /// <summary>
    /// The command to remove the selected directory from the list
    /// </summary>
    public ICommand RemoveCommand => new DelegateCommand(RemoveDirectory);

    /// <summary>
    /// The command to start the search
    /// </summary>
    public ICommand SearchCommand => new DelegateCommand(Search);

    /// <summary>
    /// The command to move to the next occurrence
    /// </summary>
    public ICommand MoveNextCommand => new DelegateCommand(() => Move(true));

    /// <summary>
    /// The command to move to the previous occurence
    /// </summary>
    public ICommand MovePreviousCommand => new DelegateCommand(() => Move(false));

    /// <summary>
    /// The command to copy the file content
    /// </summary>
    public ICommand CopyContentCommand => new DelegateCommand(CopyFileContent);

    /// <summary>
    /// The command to show the settings window
    /// </summary>
    public ICommand SettingsCommand => new DelegateCommand(() =>
    {
        SettingsOpen = !SettingsOpen;
    });

    /// <summary>
    /// The command to reveal the selected file in the explorer
    /// </summary>
    public ICommand RevealInExplorerCommand => new DelegateCommand(() =>
    {
        if (SelectedEntry == null)
            return;

        Helper.OpenInExplorer(SelectedEntry.Path);
    });

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="setText">The action to set the text</param>
    /// <param name="selectLine">The action to selected the occurrence</param>
    /// <param name="initFlyOut">The action to init the fly out</param>
    public void InitViewModel(Action<string, string> setText, Action<OccurenceEntry> selectLine, Action initFlyOut)
    {
        _setText = setText;
        _selectLine = selectLine;
        _initFlyOut = initFlyOut;
        LoadSettings();

        AppInfo = $"FindInFile v{Assembly.GetExecutingAssembly().GetName().Version}";
    }

    /// <summary>
    /// Adds a new directory
    /// </summary>
    private void AddDirectory()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Multiselect = true,
            EnsurePathExists = true,
            Title = "Select the desired directory"
        };

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            return;

        foreach (var entry in dialog.FileNames)
        {
            if (DirectoryList.Any(a => a.Equals(entry, StringComparison.OrdinalIgnoreCase)))
                continue;

            DirectoryList.Add(entry);
        }
    }

    /// <summary>
    /// Removes the selected directory
    /// </summary>
    private async void RemoveDirectory()
    {
        if (string.IsNullOrEmpty(SelectedDirectory))
            return;

        if (await ShowQuestion("Remove", $"Remove directory '{SelectedDirectory}'?", "Yes", "No") !=
            MessageDialogResult.Affirmative)
            return;

        DirectoryList.Remove(SelectedDirectory);
        SelectedDirectory = "";
    }

    /// <summary>
    /// Starts the search
    /// </summary>
    private async void Search()
    {
        if (!DirectoryList.Any() || string.IsNullOrEmpty(SearchText))
            return;

        string FormatTimeSpan(TimeSpan value)
        {
            return $"{value.Minutes:00}:{value.Seconds:00}";
        }

        var startTime = DateTime.Now;
        SaveSettings();

        var cancellationTokenSource = new CancellationTokenSource();
        var progress = await ShowProgress("Please wait", "Please wait while searching...", isCancelable: true);
        progress.Canceled += (_, _) => cancellationTokenSource.Cancel();
        progress.Closed += (_, _) => cancellationTokenSource.Dispose();

        try
        {
            FileReader.Progress += (max, count, msg) =>
            {
                var elapsed = DateTime.Now - startTime;
                progress.Maximum = max;
                progress.Minimum = 0;
                progress.SetProgress(count);
                progress.SetMessage($"{msg}{Environment.NewLine}Elapsed: {FormatTimeSpan(elapsed)}");
            };

            var result = await FileReader.Search(DirectoryList.ToList(), FilePattern, SearchText, IncludeFileName,
                cancellationTokenSource.Token);
            SearchResult = new ObservableCollection<FileEntry>(result);

            var duration = DateTime.Now - startTime;
            ResultInfo = $"Result: {SearchResult.Count} file(s)- Duration: {FormatTimeSpan(duration)}";
        }
        catch (Exception ex)
        {
            await ShowError(ex);
        }
        finally
        {
            await progress.CloseAsync();
        }
    }

    /// <summary>
    /// Saves the settings
    /// </summary>
    private void SaveSettings()
    {
        var saveLastSearch = Helper.Settings.SaveLastSearch;
        Properties.Settings.Default.DirList = saveLastSearch ? string.Join(";", DirectoryList) : "";
        Properties.Settings.Default.FilePattern = saveLastSearch ? FilePattern : "*.*";
        Properties.Settings.Default.Save();
    }

    /// <summary>
    /// Loads the settings
    /// </summary>
    private void LoadSettings()
    {
        if (!Helper.Settings.SaveLastSearch)
            return;

        var entries = Properties.Settings.Default.DirList.Split(';').Where(w => !string.IsNullOrEmpty(w)).ToList();
        if (entries.Any())
            DirectoryList = new ObservableCollection<string>(entries);

        FilePattern = Properties.Settings.Default.FilePattern;
        if (string.IsNullOrEmpty(FilePattern))
            FilePattern = "*.*";
    }

    /// <summary>
    /// Moves the occurrence
    /// </summary>
    /// <param name="next">true to move to the next</param>
    private void Move(bool next)
    {
        if (SelectedEntry == null)
            return;

        var maxOccurrence = SelectedEntry.Occurence.Count - 1;
        if (next)
        {
            if (_currentOccurence == maxOccurrence)
                _currentOccurence = 0;
            else
                _currentOccurence++;
        }
        else
        {
            if (_currentOccurence == 0)
                _currentOccurence = maxOccurrence;
            else
                _currentOccurence--;
        }

        // Get the entry
        var occurrence = SelectedEntry.Occurence[_currentOccurence];
        _selectLine?.Invoke(occurrence);
    }

    /// <summary>
    /// Copies the content of the file
    /// </summary>
    private void CopyFileContent()
    {
        if (SelectedEntry == null || string.IsNullOrEmpty(SelectedEntry.Content))
            return;

        CopyToClipboard(SelectedEntry.Content);
    }
}