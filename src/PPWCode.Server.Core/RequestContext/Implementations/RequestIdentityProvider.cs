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

using PPWCode.Vernacular.NHibernate.III;

namespace PPWCode.Server.Core.RequestContext.Implementations
{
    /// <summary>
    ///     Specific ASP.Net Core implementation of <see cref="IIdentityProvider" />.
    /// </summary>
    /// <remarks>
    ///     Be sure to register the <see cref="IHttpContextAccessor" />
    ///     <code>services.AddHttpContextAccessor();</code>
    /// </remarks>
    public class RequestIdentityProvider : IIdentityProvider
    {
        public RequestIdentityProvider(
            [NotNull] IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        [NotNull]
        public IHttpContextAccessor HttpContextAccessor { get; }

        /// <inheritdoc />
        public string IdentityName
            => HttpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
                   ? HttpContextAccessor.HttpContext.User.Identity.Name ?? "Authenticated"
                   : "Not Authenticated";
    }
}
