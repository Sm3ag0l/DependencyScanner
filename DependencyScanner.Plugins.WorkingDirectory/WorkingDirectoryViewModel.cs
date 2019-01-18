﻿using DependencyScanner.Api.Interfaces;
using DependencyScanner.Api.Model;
using DependencyScanner.Api.Services;
using DependencyScanner.Plugins.Wd.Model;
using DependencyScanner.Plugins.Wd.Services;
using DependencyScanner.Standalone.Events;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dependency.Scanner.Plugins.Wd
{
    public class WorkingDirectoryViewModel : ObservableObject
    {
        public RelayCommand PickWorkingDirectoryCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand<string> RemoveWorkingDirectoryCommand { get; private set; }

        private readonly WorkingDirectorySettingsManager _settingsManager;
        private readonly IMessenger _messenger;
        private readonly ILogger _logger;
        private readonly IRepositoryScanner _scanner;
        private readonly IFolderPicker _folderPicker;
        private readonly Func<IWorkingDirectory> _wdCtor;

        public ObservableCollection<IWorkingDirectory> Directories { get; set; }

        public WorkingDirectoryViewModel(WorkingDirectorySettingsManager settingsManager,
            IMessenger messenger,
            ILogger logger,
            IRepositoryScanner scanner,
            IFolderPicker folderPicker,
            Func<IWorkingDirectory> wdCtor
            )
        {
            _settingsManager = settingsManager;
            _messenger = messenger;
            _logger = logger;
            _scanner = scanner;
            _folderPicker = folderPicker;
            _wdCtor = wdCtor;

            Directories = new ObservableCollection<IWorkingDirectory>(_settingsManager.RestoreWorkingDirectories());

            InitCommands();

            foreach (var dir in Directories)
            {
                _messenger.Send<AddWorkindDirectory>(new AddWorkindDirectory(dir));
            }
        }

        private void InitCommands()
        {
            PickWorkingDirectoryCommand = new RelayCommand(async () =>
            {
                var folder = _folderPicker.PickFolder();

                var progress = new DefaultProgress();

                var newRepos = await _scanner.ScanForGitRepositories(folder, progress);

                var newWorkinDir = _wdCtor();

                newWorkinDir.Path = folder;

                newWorkinDir.Repositories = new ObservableCollection<IRepository>(newRepos.Select(a => new Repository(a)));

                Directories.Add(newWorkinDir);

                _settingsManager.SyncSettings(Directories);

                _messenger.Send<AddWorkindDirectory>(new AddWorkindDirectory(newWorkinDir));
            });

            RemoveWorkingDirectoryCommand = new RelayCommand<string>(a =>
            {
                var wd = Directories.FirstOrDefault(b => b.Path == a);

                if (wd != null)
                {
                    Directories.Remove(wd);

                    _settingsManager.SyncSettings(Directories);

                    _messenger.Send<RemoveWorkinbDirectory>(new RemoveWorkinbDirectory(wd));
                }
            });
        }
    }
}