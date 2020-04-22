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

using PPWCode.API.Core.Exceptions;
using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Interfaces
{
    /// <summary>
    ///     Implements validations of a model of type <typeparamref name="TModel" />
    ///     that cross the boundaries of the model itself. This includes all validations
    ///     that touch relations with other objects.
    /// </summary>
    /// <remarks>
    ///     Validations that are limited to properties inside the model object itself
    ///     are checked using the method <see cref="ICivilizedObject.WildExceptions" />.
    /// </remarks>
    /// <typeparam name="TModel">the type of the model</typeparam>
    /// <typeparam name="TIdentity">The identity type</typeparam>
    public interface IModelValidator<in TModel, TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
        where TModel : class, IPersistentObject<TIdentity>
    {
        /// <summary>
        ///     Validates the given <paramref name="model" />.
        /// </summary>
        /// <param name="model">a model object</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <exception cref="CompoundSemanticException">
        ///     If there are validation problems, the method will throw an exception of
        ///     type <see cref="CompoundSemanticException" />. This exception will contain
        ///     a list of <see cref="SemanticException" /> instances that represent all validations
        ///     that are violated.
        /// </exception>
        /// <exception cref="InternalProgrammingError">
        ///     An exception of type <see cref="InternalProgrammingError" /> is thrown when
        ///     a problem inside the back end itself is detected.  This is typically used when some
        ///     post conditions or invariants are violated.
        /// </exception>
        [NotNull]
        Task ValidateAsync([NotNull] TModel model, CancellationToken cancellationToken);

        /// <summary>
        ///     Validates the given <paramref name="models" />.
        /// </summary>
        /// <param name="models">Models collection</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <exception cref="CompoundSemanticException">
        ///     If there are validation problems, the method will throw an exception of
        ///     type <see cref="CompoundSemanticException" />. This exception will contain
        ///     a list of <see cref="SemanticException" /> instances that represent all validations
        ///     that are violated.
        /// </exception>
        /// <exception cref="InternalProgrammingError">
        ///     An exception of type <see cref="InternalProgrammingError" /> is thrown when
        ///     a problem inside the back end itself is detected.  This is typically used when some
        ///     post conditions or invariants are violated.
        /// </exception>
        [NotNull]
        Task ValidateAsync([NotNull] [ItemNotNull] TModel[] models, CancellationToken cancellationToken);
    }
}
