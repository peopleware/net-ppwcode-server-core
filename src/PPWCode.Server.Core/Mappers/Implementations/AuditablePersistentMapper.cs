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

using JetBrains.Annotations;

using PPWCode.API.Core;
using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Server.Core.RequestContext.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.IV;

using IAuditable = PPWCode.Vernacular.Persistence.IV.IAuditable;

namespace PPWCode.Server.Core.Mappers.Implementations
{
    /// <summary>
    ///     <inheritdoc cref="PPWCode.API.Core.IAuditable" />
    ///     <para>Extra functionality is added to map all member(s) of <see cref="PPWCode.API.Core.IAuditable" />.</para>
    /// </summary>
    /// <typeparam name="TModel">A Model of <see cref="Vernacular.Persistence.IV.IPersistentObject{T}" />.</typeparam>
    /// <typeparam name="TIdentity">Type of identity used by <typeparamref name="TModel" /></typeparam>
    /// <typeparam name="TDto">A data transfer object of <see cref="PPWCode.API.Core.PersistentDto{TIdentity}" /></typeparam>
    /// <typeparam name="TContext">Type of an optional context</typeparam>
    [UsedImplicitly]
    public abstract class AuditablePersistentMapper<TModel, TIdentity, TDto, TContext> : InsertAuditablePersistentMapper<TModel, TIdentity, TDto, TContext>
        where TModel : class, IPersistentObject<TIdentity>, IAuditable
        where TIdentity : struct, IEquatable<TIdentity>
        where TDto : AuditablePersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
        /// <inheritdoc />
        protected AuditablePersistentMapper(
            [NotNull] IRequestContext requestContext,
            [NotNull] ISessionProviderAsync sessionProvider,
            [NotNull] IRepositoryAsync<TModel, TIdentity> repository,
            [NotNull] IInsertAuditableMapper<TModel, TDto> auditableMapper)
            : base(requestContext, sessionProvider, repository, auditableMapper)
        {
        }
    }
}
