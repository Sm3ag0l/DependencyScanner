﻿using DependencyScanner.Core.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScanner.ViewModel
{
    public class SolutionProjectCompareResult : ViewModelBase
    {
        private SolutionResult _result;
        public SolutionResult Result { get => _result; set => Set(ref _result, value); }

        private IEnumerable<ConsolidateProject> _projectResult;
        public IEnumerable<ConsolidateProject> ProjectResult
        {
            get => _projectResult;
            set
            {
                Set(ref _projectResult, value);
                RaisePropertyChanged(nameof(HasConsolidates));
            }
        }

        public bool HasConsolidates
        {
            get
            {
                if (ProjectResult == null) return false;

                return ProjectResult.Any(a => a.References.Count() > 0);
            }
        }
    }
}
