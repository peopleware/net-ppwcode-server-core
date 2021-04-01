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

using JetBrains.Annotations;

using PPWCode.API.Core;
using PPWCode.API.Core.Exceptions;
using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Server.Core.RequestContext.Interfaces;

namespace PPWCode.Server.Core.Managers.Implementations
{
    /// <inheritdoc cref="ILinksManager{TSource,TLinksDto,TContext}" />
    public abstract class LinksManager<TSource, TLinksDto, TContext>
        : Manager,
          ILinksManager<TSource, TLinksDto, TContext>
        where TLinksDto : ILinksDto
        where TContext : LinksContext, new()
    {
        protected LinksManager([NotNull] IRequestContext requestContext)
        {
            RequestContext = requestContext;
        }

        [NotNull]
        public IRequestContext RequestContext { get; }

        /// <inheritdoc />
        public void Initialize(TSource source, TLinksDto dto)
            => Initialize(source, dto, new TContext());

        /// <inheritdoc />
        public void Initialize(TSource source, TLinksDto dto, TContext context)
        {
            if ((source == null) || (dto == null))
            {
                return;
            }

            if (dto.Links == null)
            {
                dto.Links = new Dictionary<string, IDictionary<string, object>>();
            }

            Uri href = GetHref(source, context);
            if (href != null)
            {
                AddLink(dto, SelfKey, new Dictionary<string, object> { { HRefKey, href } });
                dto.HRef = href;
            }

            foreach (KeyValuePair<string, IDictionary<string, object>> additionalLink in
                GetAdditionalLinks(source, context)
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && (kv.Value != null)))
            {
                AddLink(dto, additionalLink.Key, additionalLink.Value);
            }
        }

        /// <inheritdoc />
        public void Initialize(IEnumerable<TSource> sources, IEnumerable<TLinksDto> dtos)
            => Initialize(sources, dtos, new TContext());

        /// <inheritdoc />
        public void Initialize(IEnumerable<TSource> sources, IEnumerable<TLinksDto> dtos, TContext context)
        {
            using (IEnumerator<TSource> sourceEnumerator = sources.GetEnumerator())
            {
                foreach (TLinksDto dto in dtos)
                {
                    sourceEnumerator.MoveNext();
                    if (sourceEnumerator.Current == null)
                    {
                        throw new InternalProgrammingError($"Expected that {nameof(dtos)} and {nameof(sources)} are aligned.");
                    }

                    Initialize(sourceEnumerator.Current, dto, context);
                }
            }
        }

        /// <summary>
        ///     Name of self for entry in Links.
        /// </summary>
        protected virtual string SelfKey
            => "self";

        /// <summary>
        ///     Name of href for entry in Links.
        /// </summary>
        protected virtual string HRefKey
            => "href";

        /// <summary>
        ///     The <see cref="SelfRoute" /> together with the member <see cref="GetSelfRouteParameters" /> is being used to calculate a
        ///     unique <see cref="Uri" /> to our resource of type <typeparamref name="TSource" />.
        /// </summary>
        [CanBeNull]
        protected virtual string SelfRoute
            => null;

        /// <summary>
        ///     Returns all identifiers that are necessary to calculate a unique <see cref="Uri" /> to our resource of type
        ///     <typeparamref name="TSource" />.
        /// </summary>
        /// <param name="source">The source where we extract our information</param>
        /// <param name="context">Context that can be used while mapping</param>
        /// <returns>
        ///     All identifiers necessary to calculate a unique <see cref="Uri" /> to our resource of type
        ///     <typeparamref name="TSource" />.
        /// </returns>
        [CanBeNull]
        protected virtual IDictionary<string, object> GetSelfRouteParameters([NotNull] TSource source, [NotNull] TContext context)
            => null;

        /// <summary>
        ///     Calculates a unique <see cref="Uri" />, for the <paramref name="source" />
        /// </summary>
        /// <param name="source">The source where we extract our information</param>
        /// <param name="context">Context that can be used while mapping</param>
        /// <param name="route">Optional route, if route is <c>null</c>, <see cref="SelfRoute" /> will be taken</param>
        /// <param name="routeParameters">
        ///     Optional route-parameters, if route-parameters is <c>null</c>,
        ///     <see cref="GetSelfRouteParameters" /> will be taken
        /// </param>
        /// <returns>
        ///     A unique <see cref="Uri" />, based on <paramref name="route" /> and <paramref name="routeParameters" />.
        /// </returns>
        [CanBeNull]
        protected Uri GetHref(
            [NotNull] TSource source,
            [NotNull] TContext context,
            [CanBeNull] string route = null,
            [CanBeNull] IDictionary<string, object> routeParameters = null)
        {
            routeParameters = routeParameters ?? GetSelfRouteParameters(source, context);
            route = route ?? SelfRoute;

            if ((route != null) && (routeParameters != null))
            {
                context.AddVersionToRouteParameters(routeParameters);
                string link = RequestContext.Link(route, routeParameters);
                return link != null ? new Uri(link) : null;
            }

            return null;
        }

        /// <summary>
        ///     Add a new link to <see cref="ILinksDto.Links" />.
        /// </summary>
        /// <param name="dto">Dto that contains a member <see cref="ILinksDto.Links" /></param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <param name="key">Key of the link</param>
        /// <param name="href">Link itself</param>
        /// <remarks>A key can be added, if the key is not already exists as link and the reference is not null.</remarks>
        /// <result>If they is added, it will return true.</result>
        protected virtual bool AddLink(
            [NotNull] TLinksDto dto,
            [NotNull] string key,
            [NotNull] IDictionary<string, object> value)
        {
            if ((dto.Links == null) || !dto.Links.ContainsKey(key))
            {
                if (dto.Links == null)
                {
                    dto.Links = new Dictionary<string, IDictionary<string, object>>();
                }

                dto.Links.Add(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     The possibility to enrich the <see cref="ILinksDto.Links" /> list.
        /// </summary>
        /// <param name="source">The source where we extract our information</param>
        /// <param name="context">Context that can be used while mapping.</param>
        /// <returns>List of key / href pairs, to be added to our Links dictionary.</returns>
        [NotNull]
        protected virtual IEnumerable<KeyValuePair<string, IDictionary<string, object>>> GetAdditionalLinks(
            [NotNull] TSource source,
            [NotNull] TContext context)
        {
            yield break;
        }
    }

    /// <inheritdoc cref="ILinksManager{TSource,TLinksDto,TContext}" />
    public abstract class SimpleLinksManager<TLinksDto, TContext>
        : LinksManager<TLinksDto, TLinksDto, TContext>,
          ILinksManager<TLinksDto, TContext>
        where TLinksDto : ILinksDto
        where TContext : LinksContext, new()
    {
        protected SimpleLinksManager([NotNull] IRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <inheritdoc />
        public void Initialize(TLinksDto dto)
            => Initialize(dto, dto);

        /// <inheritdoc />
        public void Initialize(TLinksDto dto, TContext context)
            => Initialize(dto, dto, context);

        /// <inheritdoc />
        public void Initialize(IEnumerable<TLinksDto> dtos)
            => Initialize(dtos, dtos);

        /// <inheritdoc />
        public void Initialize(IEnumerable<TLinksDto> dtos, TContext context)
            => Initialize(dtos, dtos, context);
    }
}
