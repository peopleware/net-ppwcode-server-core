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

using JetBrains.Annotations;

using PPWCode.Server.Core.Mappers.Interfaces;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Implementations
{
    /// <inheritdoc cref="IAuditableMapper{TModel,TDto}" />
    [UsedImplicitly]
    public class AuditableMapper<TModel, TDto>
        : InsertAuditableMapper<TModel, TDto>,
          IAuditableMapper<TModel, TDto>
        where TModel : IAuditable
        where TDto : PPWCode.API.Core.IAuditable
    {

        /// <inheritdoc />
        public override void Map(TModel source, TDto destination)
        {
            base.Map(source, destination);

            destination.LastModifiedAt = source.LastModifiedAt;
            destination.LastModifiedBy = source.LastModifiedBy;
        }
    }
}
