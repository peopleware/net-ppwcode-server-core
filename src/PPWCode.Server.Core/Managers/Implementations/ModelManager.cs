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
using System.Threading;
using System.Threading.Tasks;

using Castle.Core.Logging;

using JetBrains.Annotations;

using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Implementations
{
    /// <inheritdoc cref="IModelManager{TModel,TIdentity}" />
    public abstract class ModelManager<TModel, TIdentity>
        : Manager,
          IModelManager<TModel, TIdentity>
        where TModel : class, IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
    {
        private ILogger _logger = NullLogger.Instance;

        protected ModelManager([NotNull] IRepositoryAsync<TModel, TIdentity> repository)
        {
            Repository = repository;
        }

        [NotNull]
        public IRepositoryAsync<TModel, TIdentity> Repository { get; }

        [NotNull]
        [UsedImplicitly]
        public ILogger Logger
        {
            get => _logger;
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value != null)
                {
                    _logger = value;
                }
            }
        }

        /// <inheritdoc />
        public virtual Task<TModel> GetByIdAsync(TIdentity id, CancellationToken cancellationToken)
            => Repository.GetByIdAsync(id, cancellationToken);

        /// <inheritdoc />
        public async Task<ISet<TModel>> FindByIdsAsync(ISet<TIdentity> ids, CancellationToken cancellationToken)
            => (await Repository.FindByIdsAsync(ids, cancellationToken)).ToHashSet();

        /// <inheritdoc />
        public virtual async Task<ISet<TModel>> FindAllAsync(CancellationToken cancellationToken)
            => (await Repository.FindAllAsync(cancellationToken)).ToHashSet();

        /// <inheritdoc />
        public virtual Task SaveAsync(TModel model, CancellationToken cancellationToken)
            => Repository.SaveOrUpdateAsync(model, cancellationToken);

        /// <inheritdoc />
        public virtual Task DeleteAsync(TModel model, CancellationToken cancellationToken)
            => Repository.DeleteAsync(model, cancellationToken);
    }
}
