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

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PPWCode.Server.Core.API.Exceptions
{
    [UsedImplicitly]
    public class OperationCancelledExceptionHandler : BaseExceptionHandler
    {
        protected override bool OnProcess(ExceptionContext context)
        {
            if (context.Exception is NotImplementedException e)
            {
                Logger.Error(e.Message, e);
                context.Result = new ClientClosedResult();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Represents an <see cref="StatusCodeResult" /> that when
        ///     executed will produce a 499 response, <see href="https://httpstatuses.com/499" />.
        /// </summary>
        [DefaultStatusCode(DefaultStatusCode)]
        private class ClientClosedResult : StatusCodeResult
        {
            private const int DefaultStatusCode = 499;

            /// <summary>
            ///     Creates a new <see cref="ClientClosedResult" /> instance.
            /// </summary>
            public ClientClosedResult()
                : base(DefaultStatusCode)
            {
            }
        }
    }
}
