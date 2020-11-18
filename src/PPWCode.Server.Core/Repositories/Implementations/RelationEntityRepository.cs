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
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Server.Core.Repositories.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Repositories.Implementations
{
    /// <inheritdoc cref="IRelationEntityRepository{TIdentity}" />
    [UsedImplicitly]
    public class RelationEntityRepository<TIdentity>
        : IRelationEntityRepository<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
    {
        public RelationEntityRepository(
            [NotNull] ISessionProviderAsync sessionProvider)
        {
            SessionProvider = sessionProvider;
        }

        [NotNull]
        private ISessionProviderAsync SessionProvider { get; }

        [NotNull]
        private ISession Session
            => SessionProvider.Session;

        [NotNull]
        private ITransactionProviderAsync TransactionProviderAsync
            => SessionProvider.TransactionProviderAsync;

        [NotNull]
        private ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync
            => SessionProvider.SafeEnvironmentProviderAsync;

        private IsolationLevel IsolationLevel
            => SessionProvider.IsolationLevel;

        /// <inheritdoc />
        public async Task<TModel> GetByIdAsync<TModel>(TIdentity? id, CancellationToken cancellationToken)
            where TModel : class, IPersistentObject<TIdentity>
            => id == null
                   ? default
                   : await ExecuteAsync(
                             nameof(GetByIdAsync),
                             can => GetByIdInternalAsync<TModel>(id.Value, can),
                             cancellationToken)
                         .ConfigureAwait(false);

        [NotNull]
        [ItemCanBeNull]
        protected virtual Task<TResult> ExecuteAsync<TResult>(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken)
        {
            Task<TResult> SafeAsync(CancellationToken can)
                => SafeEnvironmentProviderAsync.RunAsync(requestDescription, lambda, cancellationToken);

            return TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeAsync, cancellationToken);
        }

        /// <inheritdoc cref="IRepositoryAsync{TRoot,TId}.GetByIdAsync" />
        [NotNull]
        [ItemCanBeNull]
        protected virtual Task<TModel> GetByIdInternalAsync<TModel>(TIdentity id, CancellationToken cancellationToken)
            => Session.GetAsync<TModel>(id, cancellationToken);
    }
}
