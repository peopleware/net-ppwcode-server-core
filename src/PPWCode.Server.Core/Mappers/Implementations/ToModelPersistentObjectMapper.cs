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
using PPWCode.API.Core.Exceptions;
using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Implementations
{
    /// <inheritdoc cref="IToModelPersistentObjectMapper{TModel,TIdentity,TDto,TContext}" />
    public abstract class ToModelPersistentObjectMapper<TModel, TIdentity, TDto, TContext>
        : PersistentMapper<TModel, TIdentity, TDto, TContext>
        where TModel : class, IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TDto : class, IPersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
        protected ToModelPersistentObjectMapper(
            [NotNull] IRequestContext requestContext,
            [NotNull] ISessionProviderAsync sessionProvider,
            [NotNull] IRepositoryAsync<TModel, TIdentity> repository)
            : base(requestContext, sessionProvider, repository)
        {
        }

        /// <inheritdoc />
        protected sealed override string Route
        {
            [ContractAnnotation("=> halt")]
            get => throw new InternalProgrammingError($"Mapping from {typeof(TModel).FullName} to {typeof(TDto).FullName} isn't supported.");
        }

        /// <inheritdoc />
        [ContractAnnotation("=> halt")]
        protected sealed override IDictionary<string, object> GetRouteParameters(TModel source, TContext context)
            => throw new InternalProgrammingError($"Mapping from {typeof(TModel).FullName} to {typeof(TDto).FullName} isn't supported.");

        /// <inheritdoc />
        [ContractAnnotation("=> halt")]
        protected sealed override Task OnMapAsync(TModel source, TDto destination, TContext context, CancellationToken cancellationToken)
            => throw new InternalProgrammingError($"Mapping from {typeof(TModel).FullName} to {typeof(TDto).FullName} isn't supported.");

        /// <inheritdoc />
        [ContractAnnotation("=> halt")]
        protected sealed override TDto CreateDto()
            => throw new InternalProgrammingError($"Mapping from {typeof(TModel).FullName} to {typeof(TDto).FullName} isn't supported.");
    }
}
