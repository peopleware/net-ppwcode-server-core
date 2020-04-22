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

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Interfaces
{
    /// <inheritdoc cref="IModelManager{TModel,TIdentity}" />
    /// <summary>
    ///     <para>
    ///         Model manager that is responsible to implement all necessary business code to handle
    ///         objects of type <typeparamref name="TModel" />.
    ///     </para>
    ///     <para>
    ///         Implements validations of a model of type <typeparamref name="TModel" />
    ///         that cross the boundaries of the model itself. This includes all validations
    ///         that touch relations with other objects. These type of validations are mostly on the aggregate models.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Validations that are limited to properties inside the model object itself
    ///     are checked using the method <see cref="ICivilizedObject.WildExceptions" />.
    /// </remarks>
    public interface IModelManagerWithValidation<TModel, TIdentity>
        : IModelManager<TModel, TIdentity>,
          IModelValidator<TModel, TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TModel : class, IPersistentObject<TIdentity>
    {
        /// <inheritdoc cref="IModelManager{TModel,TIdentity}.SaveAsync" />
        [NotNull]
        Task SaveWithoutValidationAsync([NotNull] TModel model, CancellationToken cancellationToken);
    }
}
