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

using PPWCode.API.Core;
using PPWCode.API.Core.Exceptions;
using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Implementations
{
    /// <inheritdoc cref="IToDtoPersistentObjectMapper{TModel,TIdentity,TDto,TContext}" />
    public abstract class ToDtoPersistentObjectMapper<TModel, TIdentity, TDto, TContext>
        : PersistentMapper<TModel, TIdentity, TDto, TContext>
        where TModel : class, IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TDto : class, IPersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
        protected ToDtoPersistentObjectMapper(
            [NotNull] IRequestContext requestContext,
            [NotNull] ISessionProviderAsync sessionProvider,
            [NotNull] IRepositoryAsync<TModel, TIdentity> repository)
            : base(requestContext, sessionProvider, repository)
        {
        }

        /// <inheritdoc />
        [ContractAnnotation("=> halt")]
        protected sealed override Task OnMapAsync(
            TDto source,
            TModel destination,
            TContext context,
            CancellationToken cancellationToken)
            => throw new InternalProgrammingError($"Mapping from {typeof(TDto).FullName} to {typeof(TModel).FullName} isn't supported.");

        /// <inheritdoc />
        [ContractAnnotation("=> halt")]
        protected sealed override Task<TModel> FetchOrCreateModelAsync(TDto dto, TContext context, CancellationToken cancellationToken)
            => throw new InternalProgrammingError($"Creating a model of type {typeof(TModel).FullName} from {typeof(TDto).FullName} isn't supported.");

        /// <inheritdoc />
        [ContractAnnotation("=> halt")]
        protected sealed override TModel CreateModel(TDto dto, TContext context)
            => throw new InternalProgrammingError($"Creating a model of type {typeof(TModel).FullName} from {typeof(TDto).FullName} isn't supported.");
    }
}
