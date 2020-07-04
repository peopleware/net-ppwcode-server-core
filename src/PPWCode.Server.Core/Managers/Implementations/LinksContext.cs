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

using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;

namespace PPWCode.Server.Core.Managers.Implementations
{
    public abstract class LinksContext
    {
        public const string VersionRouteParameter = "version";
        public const string DefaultApiVersionFormat = "VVV";

        protected LinksContext(
            [NotNull] ApiVersion apiVersion,
            [CanBeNull] string apiVersionFormat = null)
        {
            ApiVersion = apiVersion;
            ApiVersionFormat = apiVersionFormat ?? DefaultApiVersionFormat;
        }

        [NotNull]
        public ApiVersion ApiVersion { get; }

        [NotNull]
        public string ApiVersionFormat { get; }

        public void AddVersionToRouteParameters([NotNull] IDictionary<string, object> routeValues)
        {
            if (!routeValues.ContainsKey(VersionRouteParameter))
            {
                routeValues.Add(VersionRouteParameter, ApiVersion.ToString(ApiVersionFormat));
            }
        }
    }
}
