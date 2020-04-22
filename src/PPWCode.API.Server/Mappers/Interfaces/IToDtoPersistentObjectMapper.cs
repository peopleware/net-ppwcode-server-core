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

using PPWCode.API.Core;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Interfaces
{
    /// <summary>
    ///     All functionality needed to convert persistent models of type <typeparamref name="TModel" /> to persistent
    ///     dto's of type <typeparamref name="TDto" />.
    /// </summary>
    /// <typeparam name="TModel">Type of our model.</typeparam>
    /// <typeparam name="TIdentity">Type of identity used by <typeparamref name="TModel" />.</typeparam>
    /// <typeparam name="TDto">Type of our dto.</typeparam>
    /// <typeparam name="TContext">Type of an optional context.</typeparam>
    /// <remarks>
    ///     <para>A persistent object (=entity) is an object that implements <see cref="IPersistentObject{T}" />.</para>
    ///     <para>
    ///         A <b>D</b>ata <b>T</b>ransfer <b>O</b>bject is an object that derives from
    ///         <see cref="PersistentDto{TIdentity}" />.
    ///     </para>
    /// </remarks>
    public interface IToDtoPersistentObjectMapper<in TModel, TIdentity, TDto, in TContext>
        : IToDtoComponentMapper<TModel, TDto, TContext>
        where TModel : IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TDto : PersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
        /// <summary>
        ///     Convert models of type <typeparamref name="TModel" /> to an array of type
        ///     <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="models">Models to be converted.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The converted models of type <typeparamref name="TDto" />.</returns>
        [NotNull]
        Task<TDto[]> MapAsync([NotNull] [ItemNotNull] IEnumerable<TModel> models, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Convert models of type <typeparamref name="TModel" /> to an array of type
        ///     <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="models">Models to be converted.</param>
        /// <param name="context">Context that can be used while mapping.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The converted models of type <typeparamref name="TDto" />.</returns>
        [NotNull]
        Task<TDto[]> MapAsync([NotNull] [ItemNotNull] IEnumerable<TModel> models, [NotNull] TContext context, CancellationToken cancellationToken = default);
    }
}
