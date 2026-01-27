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

using System.Collections.Generic;

using JetBrains.Annotations;

using PPWCode.API.Core;
using PPWCode.Server.Core.Managers.Interfaces;
using PPWCode.Server.Core.RequestContext.Interfaces;

namespace PPWCode.Server.Core.Managers.Implementations;

/// <inheritdoc cref="ILinksManager{TSource,TLinksDto,TContext}" />
public abstract class SimpleLinksManager<TLinksDto, TContext>
    : LinksManager<TLinksDto, TLinksDto, TContext>,
      ILinksManager<TLinksDto, TContext>
    where TLinksDto : ILinksDto
    where TContext : LinksContext, new()
{
    protected SimpleLinksManager([NotNull] IRequestContext requestContext)
        : base(requestContext)
    {
    }

    /// <inheritdoc />
    public void Initialize(TLinksDto dto)
        => Initialize(dto, dto);

    /// <inheritdoc />
    public void Initialize(TLinksDto dto, TContext context)
        => Initialize(dto, dto, context);

    /// <inheritdoc />
    public void Initialize(IEnumerable<TLinksDto> dtos)
        => Initialize(dtos, dtos);

    /// <inheritdoc />
    public void Initialize(IEnumerable<TLinksDto> dtos, TContext context)
        => Initialize(dtos, dtos, context);
}
