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

using Microsoft.AspNetCore.Http;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.RequestContext.Implementations
{
    /// <summary>
    ///     Specific ASP.Net Core implementation of <see cref="ITimeProvider" />.
    ///     This implementation of <see cref="ITimeProvider" /> has the following characteristic:
    ///     <para>
    ///         Each sub-sequent call to either <see cref="ITimeProvider.Now" /> or <see cref="ITimeProvider.UtcNow" />
    ///         is within the same request the same.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Be sure to register the <see cref="IHttpContextAccessor" />
    ///     <code>services.AddHttpContextAccessor();</code>
    /// </remarks>
    public class RequestTimeProvider : ITimeProvider
    {
        public const string RequestTimeName = "PPW_RequestTimestamp";

        private DateTime _now;
        private DateTime _utcNow;

        public RequestTimeProvider([NotNull] IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        [NotNull]
        public IHttpContextAccessor HttpContextAccessor { get; }

        /// <inheritdoc />
        public DateTime Now
        {
            get
            {
                EnsureInitialization();
                return _now;
            }
        }

        /// <inheritdoc />
        public DateTime UtcNow
        {
            get
            {
                EnsureInitialization();
                return _utcNow;
            }
        }

        private void EnsureInitialization()
        {
            DateTime utcNow;
            if (!HttpContextAccessor.HttpContext.Items.ContainsKey(RequestTimeName))
            {
                utcNow = DateTime.UtcNow;
                HttpContextAccessor.HttpContext.Items.Add(RequestTimeName, utcNow);
            }
            else
            {
                utcNow = (DateTime)HttpContextAccessor.HttpContext.Items[RequestTimeName];
            }

            _now = utcNow.ToLocalTime();
            _utcNow = utcNow;
        }
    }
}
