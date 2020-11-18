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

using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Managers.Implementations
{
    /// <inheritdoc cref="IModelManagerWithValidation{TModel,TIdentity}" />
    public abstract class ModelManagerWithValidation<TModel, TIdentity>
        : ModelManager<TModel, TIdentity>,
          IModelManagerWithValidation<TModel, TIdentity>
        where TModel : class, IPersistentObject<TIdentity>
        where TIdentity : struct, IEquatable<TIdentity>
    {
        /// <inheritdoc />
        protected ModelManagerWithValidation(
            [NotNull] IRepositoryAsync<TModel, TIdentity> repository,
            [NotNull] IModelValidator<TModel, TIdentity> modelValidator)
            : base(repository)
        {
            ModelValidator = modelValidator;
        }

        [NotNull]
        public IModelValidator<TModel, TIdentity> ModelValidator { get; }

        /// <inheritdoc />
        public override async Task SaveAsync(TModel model, CancellationToken cancellationToken)
        {
            await base.SaveAsync(model, cancellationToken);
            await ModelValidator.ValidateAsync(model, cancellationToken);
        }

        /// <inheritdoc />
        public Task SaveWithoutValidationAsync(TModel model, CancellationToken cancellationToken)
            => base.SaveAsync(model, cancellationToken);

        /// <inheritdoc />
        public Task ValidateAsync(TModel model, CancellationToken cancellationToken)
            => ModelValidator.ValidateAsync(model, cancellationToken);

        /// <inheritdoc />
        public Task ValidateAsync(TModel[] models, CancellationToken cancellationToken)
            => ModelValidator.ValidateAsync(models, cancellationToken);
    }
}
