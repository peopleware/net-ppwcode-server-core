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

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using PPWCode.API.Core;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Interfaces
{
    /// <summary>
    ///     All functionality needed to convert objects of type <typeparamref name="TComponent" />  to
    ///     <typeparamref name="TDto" />.
    /// </summary>
    /// <typeparam name="TComponent">Type of our model.</typeparam>
    /// <typeparam name="TDto">Type of our Data Transfer object</typeparam>
    /// <typeparam name="TContext">Type of an optional context</typeparam>
    /// <remarks>
    ///     <para>A components is an object that implements <see cref="ICivilizedObject" />.</para>
    ///     <para>A <b>D</b>ata <b>T</b>ransfer <b>O</b>bject is an object that derives from <see cref="Dto" />.</para>
    /// </remarks>
    public interface IToDtoComponentMapper<in TComponent, TDto, in TContext>
        where TComponent : ICivilizedObject
        where TDto : class, IDto
        where TContext : MapperContext, new()
    {
        /// <summary>
        ///     Convert an object of type <typeparamref name="TComponent" /> to a new created object of type
        ///     <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="component">Object to be converted</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The converted object of type <typeparamref name="TDto" /></returns>
        [NotNull]
        [ItemCanBeNull]
        Task<TDto> MapAsync([CanBeNull] TComponent component, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Convert an object of type <typeparamref name="TComponent" /> to a new created object of type
        ///     <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="component">Object to be converted</param>
        /// <param name="context">Optional context of type <typeparamref name="TContext" /></param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The converted object of type <typeparamref name="TDto" /></returns>
        [NotNull]
        [ItemCanBeNull]
        Task<TDto> MapAsync([CanBeNull] TComponent component, [NotNull] TContext context, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Convert an object of type <typeparamref name="TComponent" /> into an object of type <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="component">Object to be converted</param>
        /// <param name="dto">Converted object</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        Task MapAsync([NotNull] TComponent component, [NotNull] TDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Convert an object of type <typeparamref name="TComponent" /> into an object of type <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="component">Object to be converted</param>
        /// <param name="dto">Converted object</param>
        /// <param name="context">Context that can be used while mapping</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        Task MapAsync(
            [NotNull] TComponent component,
            [NotNull] TDto dto,
            [NotNull] TContext context,
            CancellationToken cancellationToken = default);
    }
}
