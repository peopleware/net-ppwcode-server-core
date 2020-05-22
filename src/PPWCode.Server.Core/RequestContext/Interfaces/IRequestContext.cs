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

namespace PPWCode.Server.Core.RequestContext.Interfaces
{
    /// <summary>
    ///     Makes context from the HttpRequest.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>only make things available on an as-needed basis</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 the context has a scoped lifestyle and is linked to the <see cref="HttpRequestMessage" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public interface IRequestContext
    {
        /// <summary>
        ///     Returns the User linked to the current HttpRequest.
        /// </summary>
        [NotNull]
        IPrincipal User { get; }

        /// <summary>
        ///     A unique Id is generated for each request.
        /// </summary>
        [NotNull]
        string TraceIdentifier { get; }

        /// <summary>
        ///     A moment in time that is stable during the current HttpRequest.
        /// </summary>
        DateTime RequestTimestamp { get; }

        /// <summary>
        ///     Indicates that the current request (HttpRequest) doesn't modify data in the store.
        /// </summary>
        bool IsReadOnly { get; }

        // <summary>
        /// Gets the cancellation token for the request.
        /// </summary>
        /// <returns>The cancellation token for the request.</returns>
        CancellationToken RequestAborted { get; }

        /// <summary>
        ///     There will be a counter for each current HttpRequest, at the moment this counter is being used as
        ///     reference counter inside the mapping classes.
        /// </summary>
        /// <returns>
        ///     Current reference added with 1.
        /// </returns>
        int IncrementAndGetMapperReferenceCounter();

        /// <summary>
        ///     There will be a counter for each current HttpRequest, at the moment this counter is being used as
        ///     reference counter inside the mapping classes.
        /// </summary>
        /// <returns>
        ///     Current reference subtracted with 1.
        /// </returns>
        int DecrementAndGetMapperReferenceCounter();

        /// <summary>
        ///     Build route using the given parameters.
        /// </summary>
        /// <param name="routeName">name of rest api route</param>
        /// <param name="values">Available route-parameters as key/value pair</param>
        /// <returns>A url based on <paramref name="routeName" /> and <paramref name="values" /></returns>
        [CanBeNull]
        string Link([NotNull] string routeName, [NotNull] IDictionary<string, object> values);
    }
}
