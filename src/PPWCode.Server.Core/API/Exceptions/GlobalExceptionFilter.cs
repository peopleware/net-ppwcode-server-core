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

using System.Threading.Tasks;

using Castle.Core.Logging;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PPWCode.Server.Core.API.Exceptions
{
    public class GlobalExceptionFilter
        : IAsyncExceptionFilter,
          IOrderedFilter
    {
        private ILogger _logger = NullLogger.Instance;

        public GlobalExceptionFilter(int order)
        {
            Order = order;
        }

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
        [CanBeNull]
        public IExceptionHandler ExceptionHandler { get; set; }

        /// <inheritdoc />
        public Task OnExceptionAsync(ExceptionContext context)
        {
            bool handled =
                (ExceptionHandler != null)
                && ExceptionHandler.Process(context);
            if (!handled)
            {
                Logger.Error(context.Exception.Message, context.Exception);
                context.Result =
                    new ObjectResult(context.Exception)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
            }

            context.ExceptionHandled = true;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public int Order { get; }
    }
}
