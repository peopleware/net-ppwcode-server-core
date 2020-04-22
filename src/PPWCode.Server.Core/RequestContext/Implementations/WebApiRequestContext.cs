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

using Microsoft.AspNetCore.Mvc;

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

        [NotNull]
        private readonly ControllerContext _controllerContext;

        [CanBeNull]
        private IPrincipal _principal;

        [CanBeNull]
        private string _traceIdentifier;

        public WebApiRequestContext(
            [NotNull] ITimeProvider timeProvider,
            [NotNull] ControllerContext controllerContext)
            : base(timeProvider)
        {
            _controllerContext = controllerContext;
        }

        /// <inheritdoc />
        public override IPrincipal User
            => _principal = _principal ?? _controllerContext.HttpContext.User;

        /// <inheritdoc />
        public override string TraceIdentifier
            => _traceIdentifier = _traceIdentifier ?? _controllerContext.HttpContext.TraceIdentifier;

        /// <inheritdoc />
        public override CancellationToken RequestAborted
            => _controllerContext.HttpContext.RequestAborted;

        /// <inheritdoc />
        public override bool IsReadOnly
            => _safeHttpMethods.Contains(_controllerContext.HttpContext.Request.Method);

        /// <inheritdoc />
        public override string Link(string route, IDictionary<string, object> parameters)
            /*try
            {
                IDictionary<string, object> routeValues = new HttpRouteValueDictionary(parameters);
                routeValues.Add(HttpRoute.HttpRouteKey, true);
                IHttpRoute httpRoute = _httpConfiguration.Routes[route];
                IHttpVirtualPathData virtualPath = httpRoute.GetVirtualPath(_httpRequestMessage, routeValues);
                string relativePath = virtualPath.VirtualPath;
                return $"{HostConfig.ExternalBaseUrl}/{relativePath}";
            }
            catch (Exception exc)
            {
                throw new InternalProgrammingError("Cannot determine href", exc);
            }*/
            => null;
    }
}
