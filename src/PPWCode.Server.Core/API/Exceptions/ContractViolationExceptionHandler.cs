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

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using PPWCode.API.Core.Contracts;

namespace PPWCode.Server.Core.API.Exceptions
{
    [UsedImplicitly]
    public class ContractViolationExceptionHandler : BaseExceptionHandler
    {
        protected override bool OnProcess(ExceptionContext context)
        {
            if (context.Exception is ContractViolation e)
            {
                Logger.Error(e.Message, e);
                context.Result =
                    new ObjectResult(e.Message)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                return true;
            }

            return false;
        }
    }
}
