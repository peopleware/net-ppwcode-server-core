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
using System.Threading;
using System.Threading.Tasks;

using Castle.Core.Logging;

using JetBrains.Annotations;

using PPWCode.API.Core.Exceptions;
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

        /// <inheritdoc />
        public virtual async Task ValidateAsync(TModel model, CancellationToken cancellationToken)
        {
            IEnumerable<SemanticException> semanticExceptions =
                await OnValidateAsync(model, cancellationToken);
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

            await OnInvariantsAsync(model, cancellationToken);
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
                    await ValidateAsync(model, cancellationToken);
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
        [NotNull]
        protected abstract Task OnInvariantsAsync([NotNull] TModel model, CancellationToken cancellationToken);
    }
}
