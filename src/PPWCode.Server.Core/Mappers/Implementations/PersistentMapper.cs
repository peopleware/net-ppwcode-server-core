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
using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Implementations
{
    /// <summary>
    ///     All functionality needed to convert persistent objects of type <typeparamref name="TModel" />  to
    ///     <typeparamref name="TDto" /> and vice versa.
    /// </summary>
    /// <typeparam name="TModel">A Model of <see cref="IPersistentObject{T}" />.</typeparam>
    /// <typeparam name="TIdentity">Type of identity used by <typeparamref name="TModel" /></typeparam>
    /// <typeparam name="TDto">A data transfer object of <see cref="PersistentDto{TIdentity}" /></typeparam>
    /// <typeparam name="TContext">Type of an optional context</typeparam>
    public abstract class PersistentMapper<TModel, TIdentity, TDto, TContext>
        : ComponentMapper<TModel, TDto, TContext>,
          IBiDirectionalPersistentObjectMapper<TModel, TIdentity, TDto, TContext>
        where TModel : class, IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TDto : class, IPersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
        protected PersistentMapper(
            [NotNull] IRequestContext requestContext,
            [NotNull] ISessionProviderAsync sessionProvider,
            [NotNull] IRepositoryAsync<TModel, TIdentity> repository)
        {
            RequestContext = requestContext;
            SessionProvider = sessionProvider;
            Repository = repository;
        }

        [NotNull]
        public IRequestContext RequestContext { get; }

        [NotNull]
        public ISessionProviderAsync SessionProvider { get; }

        [NotNull]
        public IRepositoryAsync<TModel, TIdentity> Repository { get; }

        /// <inheritdoc />
        public sealed override Task MapAsync(
            TModel model,
            TDto dto,
            TContext context,
            CancellationToken cancellationToken)
            => ExecuteAsync(can => OnMapAsync(model, dto, context, can), cancellationToken);

        /// <inheritdoc />
        public sealed override Task MapAsync(
            TDto dto,
            TModel model,
            TContext context,
            CancellationToken cancellationToken)
            => OnMapAsync(dto, model, context, cancellationToken);

        /// <inheritdoc />
        public sealed override async Task<TModel> MapAsync(TDto dto, TContext context, CancellationToken cancellationToken)
        {
            TModel model =
                dto == default(TDto)
                    ? default
                    : await FetchOrCreateModelAsync(dto, context, cancellationToken).ConfigureAwait(false);
            if (model != null)
            {
                await MapAsync(dto, model, context, cancellationToken).ConfigureAwait(false);
            }

            return model;
        }

        /// <inheritdoc />
        public Task<TDto[]> MapAsync(IEnumerable<TModel> models, CancellationToken cancellationToken)
            => MapAsync(models, new TContext(), cancellationToken);

        /// <inheritdoc />
        public Task<TDto[]> MapAsync(IEnumerable<TModel> models, TContext context, CancellationToken cancellationToken)
        {
            async Task<TDto[]> MapAllAsync(CancellationToken can)
            {
                List<TDto> result = new List<TDto>();
                foreach (TModel model in models)
                {
                    TDto dto = CreateDto();
                    await OnMapAsync(model, dto, context, can).ConfigureAwait(false);
                    result.Add(dto);
                }

                return result.ToArray();
            }

            return ExecuteAsync(MapAllAsync, cancellationToken);
        }

        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TResult> ExecuteAsync<TResult>(
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken)
        {
            int referenceCounter = RequestContext.IncrementAndGetMapperReferenceCounter();
            try
            {
                if ((referenceCounter == 1) && !RequestContext.IsReadOnly)
                {
                    SessionProvider.Flush();
                }

                return await lambda(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                RequestContext.DecrementAndGetMapperReferenceCounter();
            }
        }

        [NotNull]
        protected virtual Task ExecuteAsync(
            [NotNull] Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken)
        {
            async Task<int> WrapperAsync(CancellationToken can)
            {
                await lambda(can).ConfigureAwait(false);
                return default;
            }

            return ExecuteAsync(WrapperAsync, cancellationToken);
        }


        /// <inheritdoc cref="MapAsync(TModel,TDto,TContext,System.Threading.CancellationToken)" />
        [NotNull]
        protected virtual Task OnMapAsync(
            [NotNull] TModel model,
            [NotNull] TDto dto,
            [NotNull] TContext context,
            CancellationToken cancellationToken)
        {
            dto.Id = model.Id;
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="MapAsync(TDto,TModel,TContext,System.Threading.CancellationToken)" />
        [NotNull]
        protected virtual Task OnMapAsync(
            [NotNull] TDto dto,
            [NotNull] TModel model,
            [NotNull] TContext context,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        ///     Creates a new model of type <typeparamref name="TModel" /> using our Data Transfer Object <paramref name="dto" />
        ///     of type <typeparamref name="TDto" />.
        /// </summary>
        /// <param name="dto">Object that is the source for creating our new model of type <typeparamref name="TModel" /></param>
        /// <param name="context">Optional context of type <typeparamref name="TContext" /></param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The newly created model of type <typeparamref name="TModel" /></returns>
        /// <remarks>
        ///     <para>
        ///         if the
        ///         <typeparamref name="TDto"></typeparamref>
        ///         is <see cref="PersistentDto{TIdentity}.IsTransient" /> then the model will be created using
        ///         <see cref="ComponentMapper{TModel,TDto,TContext}.CreateModel" />,
        ///         otherwise the persistent store will be asked to get a persistent object of type
        ///         <typeparamref name="TModel"></typeparamref>
        ///         , identified by <see cref="PersistentDto{TIdentity}.Id" />
        ///     </para>
        /// </remarks>
        [NotNull]
        [ItemNotNull]
        protected virtual async Task<TModel> FetchOrCreateModelAsync([NotNull] TDto dto, [NotNull] TContext context, CancellationToken cancellationToken)
            => !dto.IsTransient
                   ? await Repository.GetByIdAsync(dto.Id.GetValueOrDefault(), cancellationToken).ConfigureAwait(false)
                     ?? throw new SemanticException($"Unable to find a persistent model of type {typeof(TModel).FullName}, identified by {dto.Id}.")
                   : CreateModel(dto, context);
    }
}
