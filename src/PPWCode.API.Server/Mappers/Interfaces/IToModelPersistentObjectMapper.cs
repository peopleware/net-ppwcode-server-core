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

using PPWCode.API.Core;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Interfaces
{
    /// <summary>
    ///     All functionality needed to convert persistent dtos of type <typeparamref name="TDto" /> to persistent
    ///     models of type <typeparamref name="TModel" />.
    /// </summary>
    /// <typeparam name="TModel">Type of our model.</typeparam>
    /// <typeparam name="TIdentity">Type of identity used by <typeparamref name="TModel" /></typeparam>
    /// <typeparam name="TDto">Type of our Data Transfer object</typeparam>
    /// <typeparam name="TContext">Type of an optional context</typeparam>
    /// <remarks>
    ///     <para>A persistent object (=entity) is an object that implements <see cref="IPersistentObject{T}" />.</para>
    ///     <para>
    ///         A <b>D</b>ata <b>T</b>ransfer <b>O</b>bject is an object that derives from
    ///         <see cref="PersistentDto{TIdentity}" />.
    ///     </para>
    /// </remarks>
    public interface IToModelPersistentObjectMapper<TModel, TIdentity, in TDto, in TContext>
        : IToModelComponentMapper<TModel, TDto, TContext>
        where TModel : IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TDto : PersistentDto<TIdentity>
        where TContext : MapperContext, new()
    {
    }
}
