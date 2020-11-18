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
using System.Text;

using Castle.Core.Logging;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Filters;

namespace PPWCode.Server.Core.API.Exceptions
{
    public abstract class BaseExceptionHandler : IExceptionHandler
    {
        private ILogger _logger = NullLogger.Instance;

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

        [UsedImplicitly]
        public IExceptionHandler Next { get; set; }

        public bool Process(ExceptionContext context)
        {
            bool handled = OnProcess(context);
            if (!handled && (Next != null))
            {
                handled = Next.Process(context);
            }

            return handled;
        }

        protected abstract bool OnProcess([NotNull] ExceptionContext context);

        protected string FlattenExceptions([NotNull] Exception e)
        {
            StringBuilder sb = new StringBuilder();
            do
            {
                if (!string.IsNullOrWhiteSpace(e.Message))
                {
                    sb
                        .Append(e.Message)
                        .AppendLine();
                }

                e = e.InnerException;
            }
            while (e != null);

            return sb.ToString();
        }
    }
}
