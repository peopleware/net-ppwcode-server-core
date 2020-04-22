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

using PPWCode.API.Core;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Mappers.Interfaces
{
    /// <summary>
    ///     All functionality needed to convert components of type <typeparamref name="TComponent" />  to
    ///     <b>D</b>ata <b>T</b>ransfer <b>O</b>bject of type <typeparamref name="TDto" /> and vice versa.
    /// </summary>
    /// <typeparam name="TComponent">A component of <see cref="ICivilizedObject" />.</typeparam>
    /// <typeparam name="TDto">A data transfer object of <see cref="Dto" /></typeparam>
    /// <typeparam name="TContext">Type of an optional context</typeparam>
    /// <remarks>
    ///     <para>A components is an object that implements <see cref="ICivilizedObject" />.</para>
    ///     <para>A <b>D</b>ata <b>T</b>ransfer <b>O</b>bject is an object that derives from <see cref="Dto" />.</para>
    /// </remarks>
    public interface IBiDirectionalComponentMapper<TComponent, TDto, in TContext>
        : IToDtoComponentMapper<TComponent, TDto, TContext>,
          IToModelComponentMapper<TComponent, TDto, TContext>
        where TComponent : ICivilizedObject
        where TDto : Dto
        where TContext : MapperContext, new()
    {
    }
}
