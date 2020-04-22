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
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Interfaces
{
    /// <summary>
    ///     Model manager that is responsible to implement all necessary business code to handle
    ///     objects of type <typeparamref name="TModel" />.
    /// </summary>
    /// <typeparam name="TModel">Type of our model.</typeparam>
    /// <typeparam name="TIdentity">The identity type</typeparam>
    public interface IModelManager<TModel, TIdentity> : IManager
        where TIdentity : struct, IEquatable<TIdentity>
        where TModel : class, IPersistentObject<TIdentity>
    {
        /// <summary>
        ///     All business code necessary to retrieve a model of type <typeparamref name="TModel" /> based on the
        ///     identification <paramref name="id" />.
        /// </summary>
        /// <param name="id">Id of the retrieved model.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <returns>
        ///     A model of type <typeparamref name="TModel" />. If there isn't a corresponding model in our persistent store
        ///     based on <paramref name="id" />, <c>null</c> will be returned.
        /// </returns>
        [NotNull]
        [ItemCanBeNull]
        Task<TModel> GetByIdAsync(TIdentity id, CancellationToken cancellationToken);

        /// <summary>Gets an list of entities by their ids.</summary>
        /// <param name="ids">The given primary keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <returns>
        ///     The entities with the given ids which could be found, if not found no entity with the id is returned.
        /// </returns>
        [NotNull]
        [ItemNotNull]
        Task<ISet<TModel>> FindByIdsAsync([NotNull] ISet<TIdentity> ids, CancellationToken cancellationToken);

        /// <summary>
        ///     All business code necessary to retrieve all models of type <typeparamref name="TModel" />.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <returns>
        ///     A set of type <typeparamref name="TModel" /> that contains all models of type <typeparamref name="TModel" />.
        /// </returns>
        [NotNull]
        [ItemNotNull]
        Task<ISet<TModel>> FindAllAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     All business code necessary to create/update a model of type <typeparamref name="TModel" /> in our persistent
        ///     store.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <param name="model">
        ///     The model to be created/updated. This model should be a transient <see cref="IPersistentObject{T}" /> when
        ///     creating.
        /// </param>
        Task SaveAsync([NotNull] TModel model, CancellationToken cancellationToken);

        /// <summary>
        ///     All business code necessary to delete a model of type <typeparamref name="TModel" /> of our persistent store.
        /// </summary>
        /// <param name="model">The model to be deleted. This model is a none-transient <see cref="IPersistentObject{T}" />.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        Task DeleteAsync([NotNull] TModel model, CancellationToken cancellationToken);
    }
}
