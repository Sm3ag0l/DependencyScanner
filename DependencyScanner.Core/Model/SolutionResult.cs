﻿using NuGet;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DependencyScanner.Core.Model
{
    [DebuggerDisplay("{Info.Name}")]
    public class SolutionResult
    {
        public FileInfo Info { get; }
        public ICollection<ProjectResult> Projects { get; } = new List<ProjectResult>();

        public SolutionResult(FileInfo info)
        {
            Info = info;
        }

        public IEnumerable<PackageReference> GetSolutionReferences()
        {
            return Projects.SelectMany(a => a.References);
        }
    }
}