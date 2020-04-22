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
using System.Security.Principal;
using System.Threading;

using Castle.Core.Logging;

using JetBrains.Annotations;

using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.RequestContext.Implementations
{
    /// <inheritdoc cref="IRequestContext" />
    public abstract class AbstractRequestContext : IRequestContext
    {
        private ILogger _logger = NullLogger.Instance;
        private int _referenceCounter;
        private DateTime? _requestTimestamp;

        protected AbstractRequestContext(
            [NotNull] ITimeProvider timeProvider)
        {
            TimeProvider = timeProvider;
        }

        [UsedImplicitly]
        public ILogger Logger
        {
            get => _logger;
            set
            {
                if (value != null)
                {
                    _logger = value;
                }
            }
        }

        [NotNull]
        public ITimeProvider TimeProvider { get; }

        /// <inheritdoc />
        public virtual DateTime RequestTimestamp
            => (_requestTimestamp = _requestTimestamp ?? TimeProvider.UtcNow).Value;

        /// <inheritdoc />
        public int IncrementAndGetMapperReferenceCounter()
            => Interlocked.Increment(ref _referenceCounter);

        /// <inheritdoc />
        public int DecrementAndGetMapperReferenceCounter()
            => Interlocked.Decrement(ref _referenceCounter);

        /// <inheritdoc />
        public abstract CancellationToken RequestAborted { get; }

        /// <inheritdoc />
        public abstract bool IsReadOnly { get; }

        /// <inheritdoc />
        public abstract IPrincipal User { get; }

        /// <inheritdoc />
        public abstract string TraceIdentifier { get; }

        /// <inheritdoc />
        public abstract string Link(string route, IDictionary<string, object> parameters);
    }
}
