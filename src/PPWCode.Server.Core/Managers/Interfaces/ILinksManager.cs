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

using PPWCode.API.Core;
using PPWCode.Server.Core.Managers.Implementations;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Interfaces
{
    /// <summary>
    ///     Initialize the <see cref="ILinksDto.Links" /> and <see cref="ILinksDto.HRef" />
    ///     members in a given Dto of type <typeparamref name="TDto" />.
    /// </summary>
    /// <typeparam name="TModel">A model of <see cref="IPersistentObject{T}" />.</typeparam>
    /// <typeparam name="TDto">A data transfer object of <see cref="ILinksDto" /></typeparam>
    /// <typeparam name="TContext">Type of an optional context</typeparam>
    public interface ILinksManager<in TModel, in TDto, in TContext> : IManager
        where TModel : class
        where TDto : class, ILinksDto
        where TContext : LinksContext, new()
    {
        /// <summary>
        ///     Initialize the <see cref="ILinksDto.Links" /> and <see cref="ILinksDto.HRef" />
        ///     members of the <paramref name="dto" /> input.
        /// </summary>
        /// <param name="model">The model that can be used as data provider to buildup the links.</param>
        /// <param name="dto">The Dto that will be initialized.</param>
        /// <remarks>
        ///     After the initialization, <see cref="ILinksDto.Links" /> and/or
        ///     <see cref="ILinksDto.HRef" /> can be <c>null</c>.
        /// </remarks>
        void Initialize([CanBeNull] TModel model, [CanBeNull] TDto dto);

        /// <inheritdoc cref="Initialize(TModel,TDto)" />
        /// <param name="context">Optional context that can be used as additional data provider.</param>
        void Initialize([CanBeNull] TModel model, [CanBeNull] TDto dto, [NotNull] TContext context);

        /// <summary>
        ///     Initialize the <see cref="ILinksDto.Links" /> and <see cref="ILinksDto.HRef" />
        ///     members of the <paramref name="dtos" /> input.
        /// </summary>
        /// <param name="models">The models that can be used as data provider to buildup the links.</param>
        /// <param name="dtos">The Dto's that will be initialized.</param>
        /// <remarks>
        ///     <para>
        ///         After the initialization, <see cref="ILinksDto.Links" /> and/or
        ///         <see cref="ILinksDto.HRef" /> can be <c>null</c>.
        ///     </para>
        ///     <para>
        ///         The <paramref name="dtos" /> and <paramref name="models" /> should be aligned.
        ///     </para>
        /// </remarks>
        void Initialize([NotNull] [ItemNotNull] IEnumerable<TModel> models, [NotNull] [ItemNotNull] IEnumerable<TDto> dtos);

        /// <inheritdoc cref="Initialize(System.Collections.Generic.IEnumerable{TModel},System.Collections.Generic.IEnumerable{TDto})" />
        /// <param name="context">Optional context that can be used as additional data provider.</param>
        void Initialize([NotNull] [ItemNotNull] IEnumerable<TModel> models, [NotNull] [ItemNotNull] IEnumerable<TDto> dtos, [NotNull] TContext context);
    }
}
