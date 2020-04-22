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

using Castle.Core;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;

using PPWCode.Server.Core.RequestContext.Implementations;
using PPWCode.Server.Core.RequestContext.Interfaces;

namespace PPWCode.Server.Core.RequestContext.Resolvers
{
    public class WebApiRequestContextResolver : ISubDependencyResolver
    {
        public WebApiRequestContextResolver([NotNull] IKernel kernel)
        {
            Kernel = kernel;
        }

        public IKernel Kernel { get; }

        public bool CanResolve(
            [NotNull] CreationContext context,
            [NotNull] ISubDependencyResolver contextHandlerResolver,
            [NotNull] ComponentModel model,
            [NotNull] DependencyModel dependency)
        {
            const string ControllerContext = "controllerContext";

            return dependency.TargetType.Is<IRequestContext>()
                   && context.AdditionalArguments.Contains(ControllerContext)
                   && context.AdditionalArguments[ControllerContext] is ControllerContext;
        }

        public object Resolve(
            [NotNull] CreationContext context,
            [NotNull] ISubDependencyResolver contextHandlerResolver,
            [NotNull] ComponentModel model,
            [NotNull] DependencyModel dependency)
            => Kernel.Resolve<WebApiRequestContext>(context.AdditionalArguments);
    }
}
