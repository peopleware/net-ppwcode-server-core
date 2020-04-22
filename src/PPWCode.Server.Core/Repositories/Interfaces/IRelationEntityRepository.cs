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
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Repositories.Interfaces
{
    public interface IRelationEntityRepository<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
    {
        /// <summary>
        ///     Retrieve a entity type <typeparamref name="TModel" /> with a given identifier
        ///     <paramref name="id" /> which is used in a relationship to another entity.
        /// </summary>
        /// <typeparam name="TModel">Entity type to retrieve</typeparam>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>
        ///     The requested entity if the identifier is not null, null if no identifier is being supplied.
        /// </returns>
        /// <remarks>Entity retrieval will be done via a Stateless session.</remarks>
        [NotNull]
        [ItemCanBeNull]
        Task<TModel> GetByIdAsync<TModel>(TIdentity? id, CancellationToken cancellationToken = default)
            where TModel : class, IPersistentObject<TIdentity>;
    }
}
