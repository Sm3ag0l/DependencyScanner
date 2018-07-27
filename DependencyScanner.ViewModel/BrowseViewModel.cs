﻿using DependencyScanner.Core.Interfaces;
using DependencyScanner.Core.Model;
using DependencyScanner.ViewModel.Events;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using NuGet;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Threading;
using DependencyScanner.ViewModel.Tools;
using static DependencyScanner.ViewModel.Tools.DispatcherTools;
using System.Collections.Specialized;
using System.Linq;

namespace DependencyScanner.ViewModel
{
    public class BrowseViewModel : SolutionBaseViewModel<SolutionResult>
    {
        private readonly IScanner _scanner;
        private readonly IMessenger _messenger;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _isScanning;
        public bool IsScanning { get => _isScanning; set => Set(ref _isScanning, value); }

        private double _progress;
        public double Progress { get => _progress; set => Set(ref _progress, value); }

        private string _progressMessage = "";
        public string ProgressMessage { get => _progressMessage; set => Set(ref _progressMessage, value); }

        public RelayCommand PickWorkingDirectoryCommand { get; private set; }
        public RelayCommand ScanCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        private ObservableCollection<string> _workingDirectories;
        public ObservableCollection<string> WorkingDirectories { get => _workingDirectories; set => Set( ref _workingDirectories, value); }

        private string _workingDirectory;
        public string WorkingDirectory { get => _workingDirectory; set => Set(ref _workingDirectory, value); }

        public BrowseViewModel()
        {
            if (!IsInDesignMode)
            {
                throw new Exception("Allowed only at design time");
            }

            IsScanning = true;
            Progress = 50D;
            ProgressMessage = "Scanning in designer";

            FileInfo FakeInfo() => new FileInfo(@"F:\Projects\_GitHub\DependencyScanner\DependencyScanner.sln");

            var reference1 = new PackageReference("Nuget", new SemanticVersion(new Version(1, 1, 1, 2)), null, new System.Runtime.Versioning.FrameworkName("dot.Net", new Version(1, 2)), false);
            var project1 = new ProjectResult(FakeInfo(), FakeInfo());
            project1.References.Add(reference1);
            var solution1 = new SolutionResult(FakeInfo());
            solution1.Projects.Add(project1);

            ScanResult = new ObservableCollection<SolutionResult>()
            {
                solution1
            };
        }

        public BrowseViewModel(IScanner scanner, IMessenger messenger)
        {
            _scanner = scanner;
            _messenger = messenger;

            PickWorkingDirectoryCommand = new RelayCommand(() =>
            {
                DispacherInvoke(() =>
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        DialogResult result = dialog.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                        {
                            WorkingDirectory = dialog.SelectedPath;

                            //ScanResult?.Clear();

                            _messenger.Send<ClearResultEvent>(new ClearResultEvent());

                            var allWorkingdirectories = AppSettings.Instance.WorkingDirectories;

                            if (!allWorkingdirectories.Contains(WorkingDirectory))
                            {
                                allWorkingdirectories.Add(WorkingDirectory);
                            }

                            Properties.Settings.Default.WorkingDirectory = WorkingDirectory;

                            Properties.Settings.Default.Save();

                            if (AppSettings.Instance.AutoScanAfterPickingDirectory)
                            {
                                ScanCommand.Execute(null);
                            }
                        }
                    }
                });
            });

            ScanCommand = new RelayCommand(async () =>
            {
                try
                {
                    ProgressMessage = "Init scan";

                    Progress = 0D;

                    IsScanning = true;

                    _cancellationTokenSource = new CancellationTokenSource();

                    await Scan(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    IsScanning = false;

                    _cancellationTokenSource = null;
                }
            });

            CancelCommand = new RelayCommand(() =>
            {
                _cancellationTokenSource?.Cancel();
            });

            WorkingDirectories = new ObservableCollection<string>(Properties.Settings.Default.WorkingDirectories.OfType<string>());

            WorkingDirectories.CollectionChanged += (s, e) =>
            {
                var collection = new StringCollection();

                foreach (var item in WorkingDirectories)
                {
                    collection.Add(item);
                }

                Properties.Settings.Default.WorkingDirectories = collection;

                Properties.Settings.Default.Save();
            };

            if (!string.IsNullOrEmpty(Properties.Settings.Default.WorkingDirectory))
            {
                WorkingDirectory = Properties.Settings.Default.WorkingDirectory;

                if (AppSettings.Instance.ExecuteScanOnInit)
                {
                    ScanCommand.Execute(null);
                }
            }
        }

        private Task Scan(CancellationToken Token)
        {
            return Task.Run(() =>
            {
                var progress = new DefaultProgress
                {
                    Token = Token
                };

                progress.ReportAction = a =>
                {
                    Progress = a.Value;
                    ProgressMessage = a.Message;
                };

                var scanResult = _scanner.ScanSolutions(WorkingDirectory, progress);

                ScanResult = new ObservableCollection<SolutionResult>(scanResult);

                FilterScanResult = CollectionViewSource.GetDefaultView(ScanResult);

                FilterScanResult.Filter = FilterJob;

                _messenger.Send<IEnumerable<SolutionResult>>(ScanResult);
            });
        }

        protected override bool FilterJob(object value)
        {
            if (value is SolutionResult input && !string.IsNullOrEmpty(SolutionFilter))
            {
                return input.Info.Name.IndexOf(SolutionFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return true;
        }
    }
}
