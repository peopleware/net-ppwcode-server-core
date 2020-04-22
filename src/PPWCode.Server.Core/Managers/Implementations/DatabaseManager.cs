// Copyright 2020 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Castle.Core.Logging;

using HibernatingRhinos.Profiler.Appender;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Server.Core.Utils;
using PPWCode.Vernacular.NHibernate.III;

namespace PPWCode.Server.Core.Managers.Implementations
{
    [UsedImplicitly]
    public abstract class DatabaseManager : IDatabaseManager
    {
        private ILogger _logger = NullLogger.Instance;

        protected DatabaseManager(
            [NotNull] INHibernateSessionFactory nHibernateSessionFactory,
            [NotNull] INhConfiguration nhConfiguration,
            [NotNull] IPpwHbmMapping ppwHbmMapping)
        {
            NHibernateSessionFactory = nHibernateSessionFactory;
            NhConfiguration = nhConfiguration;
            PPWHbmMapping = ppwHbmMapping;
        }

        [NotNull]
        public INHibernateSessionFactory NHibernateSessionFactory { get; }

        [NotNull]
        public INhConfiguration NhConfiguration { get; }

        [NotNull]
        public IPpwHbmMapping PPWHbmMapping { get; }

        [UsedImplicitly]
        public ILogger Logger
        {
            get => _logger;
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value != null)
                {
                    _logger = value;
                }
            }
        }

        public virtual void ExecuteScript(string script)
        {
            using (ISession session = NHibernateSessionFactory.SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction(IsolationLevel.Serializable))
                {
                    IEnumerable<string> commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    foreach (string cmd in commands.Where(c => !string.IsNullOrWhiteSpace(c)))
                    {
                        session
                            .CreateSQLQuery(cmd)
                            .ExecuteUpdate();
                    }

                    transaction.Commit();
                }
            }
        }

        public virtual void Create(
            bool canCreateDatabase,
            bool canAskAcknowledge,
            bool useNHibernateProfiler,
            bool suppressProfilingWhileCreatingSchema,
            bool useNHibernateSchemaExport)
        {
            if (canCreateDatabase)
            {
                if (canAskAcknowledge)
                {
                    int timeoutSeconds = 10;
                    Console.WriteLine($" --- Answer the next questions in {timeoutSeconds} seconds ---");
                    Console.WriteLine(" --- Otherwise startup will continue assuming \'N\'          ---");
                    Console.Write("Recreate database (includes the core data)? (Y/N) : ");
                    try
                    {
                        if (TimeOutConsoleReader.ReadLine(timeoutSeconds * 1000).ToUpper() == "Y")
                        {
                            Console.WriteLine();
                            Console.WriteLine();

                            CreateInternal(
                                useNHibernateProfiler,
                                suppressProfilingWhileCreatingSchema,
                                useNHibernateSchemaExport);
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Waited too long. Database is not recreated.");
                    }
                }
                else
                {
                    CreateInternal(
                        useNHibernateProfiler,
                        suppressProfilingWhileCreatingSchema,
                        useNHibernateSchemaExport);
                }
            }
        }

        protected virtual void CreateInternal(
            bool useNHibernateProfiler,
            bool suppressProfilingWhileCreatingSchema,
            bool useNHibernateSchemaExport)
        {
            if (useNHibernateProfiler
                && suppressProfilingWhileCreatingSchema)
            {
                using (ProfilerIntegration.IgnoreAll())
                {
                    if (useNHibernateSchemaExport)
                    {
                        CreateUsingHbm2Ddl();
                    }
                    else
                    {
                        CreateUsingFluentMigrator();
                    }
                }
            }
            else
            {
                if (useNHibernateSchemaExport)
                {
                    CreateUsingHbm2Ddl();
                }
                else
                {
                    CreateUsingFluentMigrator();
                }
            }
        }

        protected virtual void CreateUsingHbm2Ddl()
        {
            string dropAllScript = GetDropAllScript();
            if (dropAllScript != null)
            {
                ExecuteScript(dropAllScript);
            }

            ShowMappings(PPWHbmMapping.HbmMapping);
            SchemaExport schemaExport = new SchemaExport(NhConfiguration.GetConfiguration());
            schemaExport.Create(false, true);
        }

        protected virtual void CreateUsingFluentMigrator()
        {
        }

        [CanBeNull]
        protected virtual string GetDropAllScript()
        {
            Assembly assembly = typeof(DatabaseManager).Assembly;
            string resource = $"{typeof(DatabaseManager).Namespace}.DropAll.sql";
            Stream stream = assembly.GetManifestResourceStream(resource);
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return null;
        }

        protected virtual void ShowMappings(HbmMapping hbmMapping)
        {
            string xmlMapping =
                new StringBuilder()
                    .AppendLine()
                    .AppendLine(hbmMapping.AsString())
                    .ToString();
            Logger.Info(xmlMapping);
        }
    }
}
