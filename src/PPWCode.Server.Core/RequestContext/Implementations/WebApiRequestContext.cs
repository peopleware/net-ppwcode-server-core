// Copyright 2024 by PeopleWare n.v..
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
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.RequestContext.Implementations
{
    /// <inheritdoc cref="IRequestContext" />
    [UsedImplicitly]
    public class WebApiRequestContext(
        [NotNull] ITimeProvider timeProvider,
        [NotNull] IHttpContextAccessor httpContextAccessor)
        : AbstractRequestContext(timeProvider)
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

        [NotNull]
        private readonly object _locker = new object();

        [CanBeNull]
        private IPrincipal _principal;

        [CanBeNull]
        private string _traceIdentifier;

        [CanBeNull]
        private IUrlHelper _urlHelper;

        [NotNull]
        protected HttpContext HttpContext { get; } = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("A http context is required!");

        /// <inheritdoc />
        public override IPrincipal User
            => _principal = _principal ?? HttpContext.User;

        /// <inheritdoc />
        public override string TraceIdentifier
            => _traceIdentifier = _traceIdentifier ?? HttpContext.TraceIdentifier;

        /// <inheritdoc />
        public override CancellationToken RequestAborted
            => HttpContext.RequestAborted;

        /// <inheritdoc />
        public override bool IsReadOnly
            => _safeHttpMethods.Contains(HttpContext.Request.Method);

        /// <inheritdoc />
        public override string Link(string routeName, IDictionary<string, object> values)
        {
            if (string.IsNullOrWhiteSpace(routeName))
            {
                return null;
            }

            if (_urlHelper == null)
            {
                lock (_locker)
                {
                    if (_urlHelper == null)
                    {
                        Endpoint endpoint = HttpContext.GetEndpoint();
                        if (endpoint != null)
                        {
                            IDataTokensMetadata dataTokens = endpoint.Metadata.GetMetadata<IDataTokensMetadata>();

                            RouteData routeData = new RouteData();
                            routeData.PushState(router: null, HttpContext.Request.RouteValues, new RouteValueDictionary(dataTokens?.DataTokens));

                            ActionDescriptor action = endpoint.Metadata.GetMetadata<ActionDescriptor>();
                            if (action != null)
                            {
                                ActionContext actionContext = new ActionContext(HttpContext, routeData, action);
                                IServiceProvider services = HttpContext.RequestServices;
                                _urlHelper =
                                    services
                                        .GetRequiredService<IUrlHelperFactory>()
                                        .GetUrlHelper(actionContext);
                            }
                        }
                    }
                }
            }

            return _urlHelper?.Link(routeName, values);
        }
    }
}
