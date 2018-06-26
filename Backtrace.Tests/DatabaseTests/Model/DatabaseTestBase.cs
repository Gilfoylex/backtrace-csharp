﻿using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Interfaces.Database;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests.Model
{
    /// <summary>
    /// Database tests base class
    /// </summary>
    public class DatabaseTestBase
    {
        /// <summary>
        /// Database
        /// </summary>
        protected BacktraceDatabase _database;

        [SetUp]
        public virtual void Setup()
        {
            //get project path
            string projectPath = Environment.CurrentDirectory;

            //mock api
            var mockApi = new Mock<IBacktraceApi>();
            mockApi.Setup(n => n.Send(It.IsAny<BacktraceData>()))
                .Returns(new BacktraceResult());

            //mock file context
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext>();
            mockFileContext.Setup(n => n.GetEntries())
                .Returns(new List<FileInfo>());

            //mock cache
            var mockCacheContext = new Mock<IBacktraceDatabaseContext>();
            mockFileContext.Setup(n => n.RemoveOrphaned(It.IsAny<IEnumerable<BacktraceDatabaseEntry>>()));

            var settings = new BacktraceDatabaseSettings(projectPath)
            {
                AutoSendMode = false, //we don't want to test timers
                MaxRecordCount = 100,
                RetryLimit = 3 
            };
            _database = new BacktraceDatabase(settings)
            {
                BacktraceDatabaseContext = new MockBacktraceDatabaseContext(projectPath, 3, RetryOrder.Stack),
                BacktraceDatabaseFileContext = mockFileContext.Object,
                BacktraceApi = mockApi.Object
            };
        }

        /// <summary>
        /// Dispose all entries in memory cache. Use this method only for testing purpose!
        /// </summary>
        protected void DisposeEntries()
        {
            ((MockBacktraceDatabaseContext)_database.BacktraceDatabaseContext).DisposeUsedFiles();
        }

        protected void ChangeRetryOrder(RetryOrder @newOrder)
        {
            ((BacktraceDatabaseContext)_database.BacktraceDatabaseContext).RetryOrder = newOrder;
        }

        /// <summary>
        /// Get new database entry 
        /// </summary>
        /// <returns>Database entry mock</returns>
        protected BacktraceDatabaseEntry GetEntry()
        {
            //mock single entry
            var mockEntry = new Mock<BacktraceDatabaseEntry>();
            mockEntry.Setup(n => n.Delete());
            mockEntry.Setup(n => n.BacktraceData)
                .Returns(new BacktraceData(It.IsAny<BacktraceReportBase>(), It.IsAny<Dictionary<string, object>>()));
            var entry = new BacktraceData(null, new Dictionary<string, object>());
            mockEntry.SetupProperty(n => n.Entry, entry);

            mockEntry.Object.EntryWriter = new MockBacktraceDatabaseWriter();
            return mockEntry.Object;
        }
    }
}