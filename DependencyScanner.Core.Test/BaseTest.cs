﻿using DependencyScanner.Core.GitClient;
using DependencyScanner.Core.Interfaces;
using DependencyScanner.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System.Threading;

namespace DependencyScanner.Core.Test
{
    [TestClass]
    abstract public class TestBase
    {
        [TestInitialize]
        public void Init()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = logger;
        }

        public static ICancelableProgress<ProgressMessage> Progress => new DefaultProgress { Token = default(CancellationToken) };

        public static GitEngine Git => new GitEngine();

        public static FileScanner Scanner => new FileScanner(Git);
    }
}
