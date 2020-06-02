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
        where TDto : PersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly IDictionary<string, object> EmptyRouteParameters =
            new Dictionary<string, object>();

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

        /// <summary>
        ///     The <see cref="Route" /> together with the member <see cref="GetRouteParameters" /> is being used to calculate a
        ///     unique <see cref="Uri" /> to our resource of type <typeparamref name="TModel" />.
        /// </summary>
        [NotNull]
        protected abstract string Route { get; }

        /// <inheritdoc />
        public sealed override Task MapAsync(
            TModel source,
            TDto destination,
            TContext context,
            CancellationToken cancellationToken)
            => ExecuteAsync(can => OnMapAsync(source, destination, context, can), cancellationToken);

        /// <inheritdoc />
        public sealed override Task MapAsync(
            TDto source,
            TModel destination,
            TContext context,
            CancellationToken cancellationToken)
            => OnMapAsync(source, destination, context, cancellationToken);

        /// <inheritdoc />
        public sealed override async Task<TModel> MapAsync(TDto source, TContext context, CancellationToken cancellationToken)
        {
            TModel model =
                source == default(TDto)
                    ? default
                    : await FetchOrCreateModelAsync(source, context, cancellationToken).ConfigureAwait(false);
            if (model != null)
            {
                await MapAsync(source, model, context, cancellationToken).ConfigureAwait(false);
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
        protected virtual Task OnMapAsync(
            [NotNull] TModel source,
            [NotNull] TDto destination,
            TContext context,
            CancellationToken cancellationToken)
        {
            destination.Id = source.Id;
            destination.HRef = GetHref(source, context);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="MapAsync(TDto,TModel,TContext,System.Threading.CancellationToken)" />
        protected virtual Task OnMapAsync(
            [NotNull] TDto source,
            [NotNull] TModel destination,
            TContext context,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        ///     Calculates a unique <see cref="Uri" />, for the <paramref name="source" />
        /// </summary>
        /// <param name="source">The model for which we have to calculate a unique <see cref="Uri" /></param>
        /// <param name="context">Context that can be used while mapping</param>
        /// <returns>
        ///     A unique <see cref="Uri" /> that identifies the <paramref name="source" />
        /// </returns>
        protected virtual string GetHref([NotNull] TModel source, TContext context)
        {
            IDictionary<string, object> routeParameters = GetRouteParameters(source, context);
            if (context is MapperVersionContext versionContext)
            {
                versionContext.AddVersionToRouteParameters(routeParameters);
            }

            return RequestContext.Link(Route, routeParameters);
        }

        /// <summary>
        ///     Returns all identifiers that are necessary to calculate a unique <see cref="Uri" /> to our resource of type
        ///     <typeparamref name="TModel" />.
        /// </summary>
        /// <param name="source">Model where we extract our information</param>
        /// <param name="context">Context that can be used while mapping</param>
        /// <returns>
        ///     All identifiers necessary to calculate a unique <see cref="Uri" /> to our resource of type
        ///     <typeparamref name="TModel" />.
        /// </returns>
        [NotNull]
        protected abstract IDictionary<string, object> GetRouteParameters([NotNull] TModel source, TContext context);

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
        protected virtual async Task<TModel> FetchOrCreateModelAsync([NotNull] TDto dto, [CanBeNull] TContext context, CancellationToken cancellationToken)
            => !dto.IsTransient
                   ? await Repository.GetByIdAsync(dto.Id.GetValueOrDefault(), cancellationToken).ConfigureAwait(false)
                     ?? throw new SemanticException($"Unable to find a persistent model of type {typeof(TModel).FullName}, identified by {dto.Id}.")
                   : CreateModel(dto, context);
    }
}
