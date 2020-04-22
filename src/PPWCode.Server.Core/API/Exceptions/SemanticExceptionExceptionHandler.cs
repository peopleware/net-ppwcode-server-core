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
using System.Linq;
using System.Net;
using System.Text;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

using PPWCode.API.Core;
using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.API.Exceptions
{
    [UsedImplicitly]
    public class SemanticExceptionExceptionHandler : BaseExceptionHandler
    {
        private static readonly ISet<string> _environmentsConsideredAsDevelopment =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Environments.Development,
                "DEV",
                "LOCAL"
            };

        public SemanticExceptionExceptionHandler(
            [NotNull] IHostEnvironment hostEnvironment)
        {
            HostEnvironment = hostEnvironment;
        }

        [NotNull]
        public IHostEnvironment HostEnvironment { get; }

        protected override bool OnProcess(ExceptionContext context)
        {
            if (context.Exception is SemanticException e)
            {
                Logger.Info(e.Message, e);
                HttpStatusCode httpStatusCode = DetermineHttpStatusCode((dynamic)e);
                context.Result =
                    httpStatusCode == HttpStatusCode.BadRequest
                        ? new ObjectResult(BuildMessageList(context, e)) { StatusCode = (int)httpStatusCode }
                        : new ObjectResult(FlattenExceptions(e)) { StatusCode = StatusCodes.Status500InternalServerError };

                return true;
            }

            return false;
        }

        protected virtual HttpStatusCode DetermineHttpStatusCode([NotNull] SemanticException se)
            => HttpStatusCode.BadRequest;

        protected virtual HttpStatusCode DetermineHttpStatusCode([NotNull] RepositorySqlException rse)
            => HttpStatusCode.InternalServerError;

        protected virtual HttpStatusCode DetermineHttpStatusCode([NotNull] DbConstraintException ce)
            => HttpStatusCode.BadRequest;

        [NotNull]
        protected virtual MessageList BuildMessageList([NotNull] ExceptionContext context, [NotNull] SemanticException se)
        {
            IEnumerable<Message> result = Translate(context, (dynamic)se);
            return new MessageList { Messages = result.ToArray() };
        }

        [NotNull]
        protected virtual IEnumerable<Message> Translate([NotNull] ExceptionContext context, [NotNull] PropertyException pe)
        {
            yield return BuildMessage(pe.ToString());
        }

        [NotNull]
        protected virtual IEnumerable<Message> Translate([NotNull] ExceptionContext context, [NotNull] SemanticException se)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(se.Message);

            if (_environmentsConsideredAsDevelopment.Contains(HostEnvironment.EnvironmentName))
            {
                Exception e = se.InnerException;
                while (e != null)
                {
                    sb.AppendLine(e.Message);
                    e = e.InnerException;
                }
            }

            yield return BuildMessage(sb.ToString());
        }

        [NotNull]
        protected virtual IEnumerable<Message> Translate([NotNull] ExceptionContext context, [NotNull] CompoundSemanticException cse)
        {
            foreach (SemanticException exc in cse.Elements)
            {
                foreach (Message message in Translate(context, (dynamic)exc))
                {
                    yield return message;
                }
            }
        }

        [NotNull]
        protected virtual IEnumerable<Message> Translate([NotNull] ExceptionContext context, [NotNull] ValidationViolationException exc)
        {
            yield return BuildMessage(exc.Message);
        }

        [NotNull]
        private IEnumerable<Message> Translate([NotNull] ExceptionContext context, [NotNull] DbConstraintException exc)
        {
            string[] parameters =
                new List<string>
                    {
                        exc.ConstraintName ?? "UNKNOWN_CONSTRAINT_NAME",
                        exc.EntityName ?? "UNKNOWN_ENTITY_NAME",
                        exc.EntityId?.ToString() ?? "UNKNOWN_ENTITY_ID",
                        exc.Message ?? "UNKNOWN_MESSAGE"
                    }
                    .ToArray();

            switch (exc.ConstraintType)
            {
                case DbConstraintTypeEnum.PRIMARY_KEY:
                    yield return BuildMessage("DB_PK_CONSTRAINT_VIOLATION", parameters);
                    break;
                case DbConstraintTypeEnum.FOREIGN_KEY:
                    yield return BuildMessage("DB_FK_CONSTRAINT_VIOLATION", parameters);
                    break;
                case DbConstraintTypeEnum.NOT_NULL:
                    yield return BuildMessage("DB_NN_CONSTRAINT_VIOLATION", parameters);
                    break;
                case DbConstraintTypeEnum.UNIQUE:
                    yield return BuildMessage("DB_UQ_CONSTRAINT_VIOLATION", parameters);
                    break;
                case DbConstraintTypeEnum.CHECK:
                    yield return BuildMessage("DB_CK_CONSTRAINT_VIOLATION", parameters);
                    break;
                default:
                    yield return BuildMessage("DB_GENERIC_CONSTRAINT_VIOLATION", parameters);
                    break;
            }
        }

        [NotNull]
        protected virtual Message BuildMessage([NotNull] string text, string[] parameters = null)
            => Message.CreateUntranslatedMessage(InfoLevelEnum.ERROR, text, parameters);
    }
}
