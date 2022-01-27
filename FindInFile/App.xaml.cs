using System;
using System.Windows;
using FindInFile.Ui.View;
using Serilog;

namespace FindInFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Occurs when the application is started
        /// </summary>
        /// <param name="sender">The <see cref="App"/></param>
        /// <param name="e">The event arguments</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Init the logger
            const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration().WriteTo
                .File("log/log_.log", rollingInterval: RollingInterval.Day, outputTemplate: template) // Ausgabe in Datei
                .CreateLogger();

            try
            {
                // Start the program
                new MainWindow().Show();

                // Set the color theme
                Helper.SetColorTheme();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error has occurred in the method '{method}'", nameof(App_OnStartup));
            }
        }
    }
}
