﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScanner.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand<string> OpenSolutionCommand { get; }

        public BrowseViewModel BrowseVM { get; }
        public ConsolidateSolutionsViewModel ConsolidateSolutionsVM { get; }

        public MainViewModel(BrowseViewModel browseViewModel, ConsolidateSolutionsViewModel consolidateSolutionsViewModel)
        {
            BrowseVM = browseViewModel;
            ConsolidateSolutionsVM = consolidateSolutionsViewModel;

            OpenSolutionCommand = new RelayCommand<string>(a =>
            {
                try
                {
                    Process.Start(a);
                }
                catch
                {
                }
            });
        }
    }
}
