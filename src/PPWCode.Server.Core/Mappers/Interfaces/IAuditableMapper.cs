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

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Interfaces
{
    /// <summary>
    ///     All functionality needed to convert the audit information of type <typeparamref name="TModel" />  to
    ///     <typeparamref name="TDto" /> and vice versa.
    /// </summary>
    /// <typeparam name="TModel">Type of our model.</typeparam>
    /// <typeparam name="TDto">Type of our Data Transfer object</typeparam>
    public interface IAuditableMapper<in TModel, in TDto> : IInsertAuditableMapper<TModel, TDto>
        where TModel : IAuditable
        where TDto : PPWCode.API.Core.IAuditable
    {
    }
}
