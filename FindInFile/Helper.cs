﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;
using FindInFile.DataObjects;
using FindInFile.Properties;
using Newtonsoft.Json;

namespace FindInFile
{
    /// <summary>
    /// Provides several helper functions
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Contains the path of the settings file
        /// </summary>
        private static readonly string SettingsFile = Path.Combine(GetBaseFolder(), "Settings.json");

        /// <summary>
        /// Gets the path of the base folder
        /// </summary>
        /// <returns>The path of the base folder</returns>
        private static string GetBaseFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        /// <returns>The settings</returns>
        public static AppSettings LoadSettings()
        {
            if (!File.Exists(SettingsFile))
                return new AppSettings();

            var content = File.ReadAllText(SettingsFile, Encoding.UTF8);

            return JsonConvert.DeserializeObject<AppSettings>(content) ?? new AppSettings();
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        /// <param name="data">The settings</param>
        /// <returns>true when successful, otherwise false</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool SaveSettings(AppSettings data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var content = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(SettingsFile, content, Encoding.UTF8);

            return File.Exists(SettingsFile);
        }

        /// <summary>
        /// Sets the color theme of the application
        /// </summary>
        /// <param name="baseColor">The name of the base color (optional, if empty, the saved base color will be used)</param>
        /// <param name="colorTheme">The name of the color theme (optional, if empty, the saved color theme will be used)</param>
        public static void SetColorTheme(string baseColor = "", string colorTheme = "")
        {
            var settings = LoadSettings();
            if (string.IsNullOrEmpty(baseColor))
                baseColor = settings.BaseColor;

            if (string.IsNullOrEmpty(colorTheme))
                colorTheme = settings.ThemeColor;

            ThemeManager.Current.ChangeTheme(Application.Current, baseColor, colorTheme);

            ExecuteAction("SetTheme");
        }

        #region Mediator

        /// <summary>
        /// The list with the actions which should be executed when the specified key is selected
        /// </summary>
        private static readonly SortedList<string, List<Action>> Actions = new();

        /// <summary>
        /// Adds an action
        /// </summary>
        /// <param name="key">The key of the action</param>
        /// <param name="action">The action which should be executed</param>
        public static void AddAction(string key, Action action)
        {
            if (Actions.ContainsKey(key))
            {
                Actions[key].Add(action);
            }
            else
            {
                Actions.Add(key, new List<Action>
                {
                    action
                });
            }
        }

        /// <summary>
        /// Executes the action which of the desired key
        /// </summary>
        /// <param name="key">The key of the action</param>
        public static void ExecuteAction(string key)
        {
            if (!Actions.ContainsKey(key))
                return;

            var actions = Actions[key];
            foreach (var action in actions)
            {
                // Execute the action
                action();
            }
        }

        /// <summary>
        /// Removes all actions with the specified key
        /// </summary>
        /// <param name="key">The key of the action</param>
        public static void RemoveAction(string key)
        {
            if (Actions.ContainsKey(key))
            {
                Actions.Remove(key);
            }
        }

        /// <summary>
        /// Removes all actions
        /// </summary>
        public static void RemoveAllActions()
        {
            Actions.Clear();
        }
        #endregion
    }
}