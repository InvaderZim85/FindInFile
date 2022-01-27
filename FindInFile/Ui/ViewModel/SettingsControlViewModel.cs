using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ControlzEx.Theming;
using FindInFile.DataObjects;
using ZimLabs.WpfBase.NetCore;

namespace FindInFile.Ui.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.SettingsControl"/>
    /// </summary>
    internal class SettingsControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Contains the value which indicates if the control was initialized
        /// </summary>
        private bool _init = true;

        /// <summary>
        /// Backing field for <see cref="BaseColorList"/>
        /// </summary>
        private ObservableCollection<string> _baseColorList = new();

        /// <summary>
        /// Gets or sets the list with the base colors
        /// </summary>
        public ObservableCollection<string> BaseColorList
        {
            get => _baseColorList;
            private set => SetField(ref _baseColorList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedBaseColor"/>
        /// </summary>
        private string _selectedBaseColor = "Dark";

        /// <summary>
        /// Gets or sets the selected base color (light / dark)
        /// </summary>
        public string SelectedBaseColor
        {
            get => _selectedBaseColor;
            set
            {
                if (SetField(ref _selectedBaseColor, value))
                    ChangeTheme();
            }
        }

        /// <summary>
        /// Backing field for <see cref="ColorThemeList"/>
        /// </summary>
        private ObservableCollection<string> _colorThemeList = new();

        /// <summary>
        /// Gets or sets the list with the color themes
        /// </summary>
        public ObservableCollection<string> ColorThemeList
        {
            get => _colorThemeList;
            set => SetField(ref _colorThemeList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedColorTheme"/>
        /// </summary>
        private string _selectedColorTheme = "Blue";

        /// <summary>
        /// Gets or sets the selected color theme
        /// </summary>
        public string SelectedColorTheme
        {
            get => _selectedColorTheme;
            set
            {
                if (SetField(ref _selectedColorTheme, value))
                    ChangeTheme();
            }
        }

        /// <summary>
        /// Backing field for <see cref="SaveLastSearch"/>
        /// </summary>
        private bool _saveLastSearch;

        /// <summary>
        /// Gets or sets the value which indicates if the last search should be saved
        /// </summary>
        public bool SaveLastSearch
        {
            get => _saveLastSearch;
            set => SetField(ref _saveLastSearch, value);
        }

        /// <summary>
        /// Init the view model and loads the data
        /// </summary>
        public void InitViewModel()
        {
            LoadSettings();
        }

        /// <summary>
        /// The command to save the settings
        /// </summary>
        public ICommand SaveSettingsCommand => new DelegateCommand(SaveSettings);

        /// <summary>
        /// Changes the current theme
        /// </summary>
        private void ChangeTheme()
        {
            if (_init)
                return;

            if (string.IsNullOrEmpty(SelectedBaseColor) || string.IsNullOrEmpty(SelectedColorTheme))
                return;

            Helper.SetColorTheme(SelectedBaseColor, SelectedColorTheme);
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        private async void LoadSettings()
        {
            try
            {
                BaseColorList = new ObservableCollection<string>(ThemeManager.Current.BaseColors);
                ColorThemeList = new ObservableCollection<string>(ThemeManager.Current.ColorSchemes);

                SelectedBaseColor = Helper.Settings.BaseColor;
                SelectedColorTheme = Helper.Settings.ThemeColor;
                SaveLastSearch = Helper.Settings.SaveLastSearch;

                _init = false;
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        private async void SaveSettings()
        {
            var controller = await ShowProgress("Please wait", "Please wait while saving the settings...");

            try
            {
                Helper.Settings.ThemeColor = SelectedColorTheme;
                Helper.Settings.BaseColor = SelectedBaseColor;
                Helper.Settings.SaveLastSearch = SaveLastSearch;

                Helper.SaveSettings();
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await controller.CloseAsync();
            }
        }
    }
}
