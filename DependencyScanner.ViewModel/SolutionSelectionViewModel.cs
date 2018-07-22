﻿using DependencyScanner.Core.Model;
using GalaSoft.MvvmLight;

namespace DependencyScanner.ViewModel
{
    public class SolutionSelectionViewModel : ObservableObject
    {
        public SolutionResult Result { get; set; }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
    }
}
