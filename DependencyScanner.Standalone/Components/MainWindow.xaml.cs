﻿using DependencyScanner.Standalone.ViewModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DependencyScanner.Standalone
{
    public partial class MainWindow : MetroWindow
    {
        public List<AppThemeMenuData> AppThemes { get; set; }
        public List<AccentColorMenuData> AccentColors { get; set; }

        public MainWindow()
        {
            // create accent color menu items for the demo
            AccentColors = ThemeManager.Accents
                .Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                .ToList();

            // create metro theme color menu items for the demo
            AppThemes = ThemeManager.AppThemes
                .Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                .ToList();

            InitializeComponent();

#if !DEBUG
            Task.Factory.StartNew(async () =>
            {
                var updater = new ChocoUpdater();

                if (await updater.IsNewVersionAvailable())
                {
                    var mySettings = new MetroDialogSettings()
                    {
                        DefaultButtonFocus = MessageDialogResult.Affirmative,
                        AffirmativeButtonText = "Update",
                        NegativeButtonText = "Do not update",
                        FirstAuxiliaryButtonText = "Cancel"
                    };

                    var result = await this.ShowMessageAsync("Newer version was detected!", "Do you want to update dependency-scanner?",
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);

                    if (result == MessageDialogResult.Affirmative)
                    {
                        updater.Update();

                        await DispatcherHelper.RunAsync(() =>
                        {
                            Application.Current.Shutdown();
                        }).Task;
                    }
                }
            }, default(CancellationToken), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
#endif
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            settingsFlyout.IsOpen = true;
        }
    }
}
