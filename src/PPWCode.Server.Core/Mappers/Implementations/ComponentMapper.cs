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
using PPWCode.API.Core.Exceptions;
using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Implementations
{
    /// <inheritdoc cref="IBiDirectionalComponentMapper{TComponent,TDto,TContext}" />
    public abstract class ComponentMapper<TModel, TDto, TContext>
        : IBiDirectionalComponentMapper<TModel, TDto, TContext>
        where TModel : class, ICivilizedObject
        where TDto : Dto
        where TContext : MapperContext, new()
    {
        /// <inheritdoc />
        public Task<TDto> MapAsync(TModel source, CancellationToken cancellationToken)
            => MapAsync(source, new TContext(), cancellationToken);

        /// <inheritdoc />
        public async Task<TDto> MapAsync(TModel source, TContext context, CancellationToken cancellationToken)
        {
            if (source == default(TModel))
            {
                return default;
            }

            TDto dto = CreateDto();
            await MapAsync(source, dto, context, cancellationToken).ConfigureAwait(false);
            return dto;
        }

        /// <inheritdoc />
        public Task MapAsync(TModel source, TDto destination, CancellationToken cancellationToken)
            => MapAsync(source, destination, new TContext(), cancellationToken);

        /// <inheritdoc />
        public abstract Task MapAsync(
            TModel source,
            TDto destination,
            TContext context,
            CancellationToken cancellationToken);

        /// <inheritdoc />
        public Task<TModel> MapAsync(TDto source, CancellationToken cancellationToken = default)
            => MapAsync(source, new TContext(), cancellationToken);

        /// <inheritdoc />
        public virtual Task<TModel> MapAsync(TDto source, TContext context, CancellationToken cancellationToken)
            => Task.FromResult(
                source == null
                    ? default
                    : CreateModel(source, context));

        /// <inheritdoc />
        public virtual Task MapAsync(
            TDto source,
            TModel destination,
            TContext context,
            CancellationToken cancellationToken)
            => throw new InternalProgrammingError($"{nameof(ComponentMapper<TModel, TDto, TContext>)} doesn't support mapping from {nameof(TDto)} to {nameof(TModel)}.");

        /// <summary>
        ///     Creates a new model of type <typeparamref name="TModel" /> using our Data Transfer Object <paramref name="dto" />
        ///     of type <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="dto">Object that is the source for creating our new model of type <typeparamref name="TModel" /></param>
        /// <param name="context">Optional context of type <typeparamref name="TContext" /></param>
        /// <returns>The newly created model of type <typeparamref name="TModel" /></returns>
        /// <remarks>
        ///     It is not the intention that this method copies all the properties of the <paramref name="dto" /> to our newly
        ///     created model of type <typeparamref name="TModel" />.
        /// </remarks>
        [NotNull]
        protected abstract TModel CreateModel([NotNull] TDto dto, [CanBeNull] TContext context);

        /// <summary>
        ///     Creates a new Data Transfer Object of type <typeparamref name="TDto" />.
        /// </summary>
        /// <returns>The newly created Data Transfer Object of type <typeparamref name="TDto" /></returns>
        [NotNull]
        protected abstract TDto CreateDto();
    }
}
