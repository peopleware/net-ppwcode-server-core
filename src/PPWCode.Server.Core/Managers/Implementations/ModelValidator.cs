// Copyright 2026 by PeopleWare n.v..
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
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Server.Core.Utils;
using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Implementations
{
    /// <inheritdoc cref="IModelValidator{TModel,TIdentity}" />
    public abstract class ModelValidator<TModel, TIdentity>
        : IModelValidator<TModel, TIdentity>
        where TModel : class, IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
    {
        [CanBeNull]
        private ILogger _logger;

        [NotNull]
        public ILogger Logger
            => _logger ??= PPWLogging.GetLogger(GetType());

        /// <inheritdoc />
        public virtual async Task ValidateAsync(TModel model, CancellationToken cancellationToken)
        {
            IEnumerable<SemanticException> semanticExceptions =
                await OnValidateAsync(model, cancellationToken).ConfigureAwait(false);
            CompoundSemanticException validationErrors =
                semanticExceptions
                    .Aggregate(
                        model.WildExceptions(),
                        (cse, be) =>
                        {
                            if (be != null)
                            {
                                cse.AddElement(be);
                            }

                            return cse;
                        });

            if ((validationErrors != null) && !validationErrors.IsEmpty)
            {
                throw validationErrors;
            }

            await OnInvariantsAsync(model, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ValidateAsync(TModel[] models, CancellationToken cancellationToken)
        {
            IList<CompoundSemanticException> compoundSemanticExceptions =
                new List<CompoundSemanticException>();

            foreach (TModel model in models)
            {
                try
                {
                    await ValidateAsync(model, cancellationToken).ConfigureAwait(false);
                }
                catch (CompoundSemanticException cse)
                {
                    compoundSemanticExceptions.Add(cse);
                }
            }

            if (compoundSemanticExceptions.Any())
            {
                CompoundSemanticException cse = SemanticExceptionHelpers.Compact(compoundSemanticExceptions);
                if (cse != null)
                {
                    throw cse;
                }
            }
        }

        /// <summary>
        ///     Validates the given <paramref name="model" />.
        /// </summary>
        /// <param name="model">The model to be validated.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <returns>
        ///     A list of <see cref="SemanticException" /> instances that represent all validations
        ///     that are violated.
        /// </returns>
        [NotNull]
        [ItemNotNull]
        protected abstract Task<IEnumerable<SemanticException>> OnValidateAsync([NotNull] TModel model, CancellationToken cancellationToken);

        /// <summary>
        ///     Checks the given <paramref name="model" /> for finding defects inside our back end.
        /// </summary>
        /// <param name="model">The model to be check for post conditions / invariants.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
        /// <exception cref="InternalProgrammingError">
        ///     An exception of type <see cref="InternalProgrammingError" /> is thrown when
        ///     a problem inside the back end itself is detected.  This is typically used when some
        ///     post conditions or invariants are violated.
        /// </exception>
        /// <returns>A task</returns>
        [NotNull]
        protected abstract Task OnInvariantsAsync([NotNull] TModel model, CancellationToken cancellationToken);
    }
}
