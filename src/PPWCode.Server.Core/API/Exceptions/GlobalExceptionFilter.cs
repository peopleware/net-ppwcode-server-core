// Copyright 2026 by PeopleWare n.v..
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

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace PPWCode.Server.Core.API.Exceptions
{
    public class GlobalExceptionFilter
        : IAsyncExceptionFilter,
          IOrderedFilter
    {
        [CanBeNull]
        private ILogger _logger;

        public GlobalExceptionFilter(int order)
        {
            Order = order;
        }

        [NotNull]
        public ILogger Logger
            => _logger ??= PPWLogging.GetLogger(GetType());

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
                Logger.LogError(context.Exception.Message, context.Exception);
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
