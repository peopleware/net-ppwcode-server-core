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
using System.Net.Http;
using System.Security.Principal;
using System.Threading;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.RequestContext.Implementations
{
    /// <inheritdoc cref="IRequestContext" />
    [UsedImplicitly]
    public class WebApiRequestContext : AbstractRequestContext
    {
        private static readonly ISet<string> _safeHttpMethods =
            new HashSet<string>(
                new[]
                {
                    HttpMethod.Head.ToString(),
                    HttpMethod.Get.ToString(),
                    HttpMethod.Options.ToString(),
                    HttpMethod.Trace.ToString()
                },
                StringComparer.OrdinalIgnoreCase);

        [CanBeNull]
        private IPrincipal _principal;

        [CanBeNull]
        private string _traceIdentifier;

        public WebApiRequestContext(
            [NotNull] ITimeProvider timeProvider,
            [NotNull] ControllerContext controllerContext)
            : base(timeProvider)
        {
            ControllerContext = controllerContext;
        }

        [NotNull]
        protected ControllerContext ControllerContext { get; }

        /// <inheritdoc />
        public override IPrincipal User
            => _principal = _principal ?? ControllerContext.HttpContext.User;

        /// <inheritdoc />
        public override string TraceIdentifier
            => _traceIdentifier = _traceIdentifier ?? ControllerContext.HttpContext.TraceIdentifier;

        /// <inheritdoc />
        public override CancellationToken RequestAborted
            => ControllerContext.HttpContext.RequestAborted;

        /// <inheritdoc />
        public override bool IsReadOnly
            => _safeHttpMethods.Contains(ControllerContext.HttpContext.Request.Method);

        /// <inheritdoc />
        public override string Link(string routeName, IDictionary<string, object> values)
        {
            HttpContext httpContext = ControllerContext.HttpContext;
            IServiceProvider services = httpContext.RequestServices;
            IUrlHelper urlHelper =
                services
                    .GetRequiredService<IUrlHelperFactory>()
                    .GetUrlHelper(ControllerContext);
            string url = urlHelper.Link(routeName, values);
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException(@"No route matches the supplied values, don't forget 'version' if you are using a versioned API.");
            }

            return url;
        }
    }
}
