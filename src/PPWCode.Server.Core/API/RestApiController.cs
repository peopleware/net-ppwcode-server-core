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
using System.Threading.Tasks;

using Castle.Core.Logging;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;

using PPWCode.API.Core;
using PPWCode.Server.Core.Managers.Implementations;
using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Server.Core.Mappers;
using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.API
{
    [ApiController]
    public abstract class RestApiController
        : ControllerBase,
          IRestApiController
    {
        private ILogger _logger = NullLogger.Instance;

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

        /// <summary>
        ///     Converts a <see cref="IPagedList{TModel}" />, where <c>T</c> is equal to <typeparamref name="TModel" />, to a
        ///     <see cref="PagedList{TDto}" /> using the mapper <paramref name="itemMapper" />, where <c>T</c> is equal to
        ///     <typeparamref name="TDto" />.
        /// </summary>
        /// <typeparam name="TModel">Type of our model.</typeparam>
        /// <typeparam name="TIdentity">Type of identity used by <typeparamref name="TModel" />.</typeparam>
        /// <typeparam name="TDto">Type of our dto.</typeparam>
        /// <typeparam name="TContext">Type of an optional context while mapping.</typeparam>
        /// <param name="pagedModels">A paged list of <typeparamref name="TModel"></typeparamref>.</param>
        /// <param name="itemMapper">A mapper that can convert a model to dto.</param>
        /// <param name="context">Optional mapping context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <result>
        ///     A <see cref="IPagedList{TModel}" />, where <c>T</c> is equal to <typeparamref name="TModel" />.
        /// </result>
        [NotNull]
        [ItemNotNull]
        protected virtual async Task<PagedList<TDto>> MapPagedListAsync<TModel, TIdentity, TDto, TContext>(
            [NotNull] IPagedList<TModel> pagedModels,
            [NotNull] IToDtoPersistentObjectMapper<TModel, TIdentity, TDto, TContext> itemMapper,
            [CanBeNull] TContext context = null)
            where TModel : IPersistentObject<TIdentity>
            where TIdentity : struct, IEquatable<TIdentity>
            where TDto : class, IPersistentDto<TIdentity>
            where TContext : MapperContext, new()
            => new PagedList<TDto>(
                await itemMapper.MapAsync(pagedModels.Items, context ?? new TContext(), HttpContext.RequestAborted),
                pagedModels.PageIndex,
                pagedModels.PageSize,
                pagedModels.TotalCount);

        /// <summary>
        ///     Converts a <see cref="IPagedList{TModel}" />, where <c>T</c> is equal to <typeparamref name="TModel" />, to a
        ///     <see cref="PagedList{TDto}" /> using the mapper <paramref name="itemMapper" />, where <c>T</c> is equal to
        ///     <typeparamref name="TDto" />.
        /// </summary>
        /// <typeparam name="TModel">Type of our model.</typeparam>
        /// <typeparam name="TIdentity">Type of identity used by <typeparamref name="TModel" />.</typeparam>
        /// <typeparam name="TDto">Type of our dto.</typeparam>
        /// <typeparam name="TMapperContext">Type of an optional context while mapping.</typeparam>
        /// <typeparam name="TLinksContext">Type of an optional context while initializing links.</typeparam>
        /// <param name="pagedModels">A paged list of <typeparamref name="TModel"></typeparamref>.</param>
        /// <param name="itemMapper">A mapper that can convert a model to dto.</param>
        /// <param name="context">Optional mapping context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <result>
        ///     A <see cref="IPagedList{TModel}" />, where <c>T</c> is equal to <typeparamref name="TModel" />.
        /// </result>
        /// <remarks>
        ///     The links are also initialized while converting, using the <paramref name="linksManager" />.
        /// </remarks>
        [NotNull]
        [ItemNotNull]
        protected virtual async Task<PagedList<TDto>> MapPagedListAsync<TModel, TIdentity, TDto, TMapperContext, TLinksContext>(
            [NotNull] IPagedList<TModel> pagedModels,
            [NotNull] IToDtoPersistentObjectMapper<TModel, TIdentity, TDto, TMapperContext> itemMapper,
            [NotNull] ILinksManager<TModel, TDto, TLinksContext> linksManager,
            [CanBeNull] TMapperContext mapperContext = null,
            [CanBeNull] TLinksContext linksContext = null)
            where TModel : class, IPersistentObject<TIdentity>
            where TIdentity : struct, IEquatable<TIdentity>
            where TDto : class, ILinksDto, IPersistentDto<TIdentity>
            where TMapperContext : MapperContext, new()
            where TLinksContext : LinksContext, new()
        {
            TDto[] dtos = await itemMapper.MapAsync(pagedModels.Items, mapperContext ?? new TMapperContext(), HttpContext.RequestAborted);
            linksManager.Initialize(pagedModels.Items, dtos, linksContext ?? new TLinksContext());
            return new PagedList<TDto>(
                dtos,
                pagedModels.PageIndex,
                pagedModels.PageSize,
                pagedModels.TotalCount);
        }
    }
}
