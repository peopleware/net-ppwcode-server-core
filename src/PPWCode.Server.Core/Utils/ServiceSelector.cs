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
using System.Reflection;

using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

using JetBrains.Annotations;

namespace PPWCode.Server.Core.Utils
{
    public static class CastleExtensions
    {
        public static IWindsorContainer RegisterOpenGenerics(
            [NotNull] this IWindsorContainer container,
            [NotNull] Assembly assembly,
            [NotNull] Type openGenericInterfaceType,
            [NotNull] Type openGenericImplementationTYpe,
            LifestyleType lifestyleType = LifestyleType.Singleton)
        {
            container
                .Register(
                    Classes
                        .FromAssembly(assembly)
                        .BasedOn(openGenericInterfaceType)
                        .WithService.Select((t, b) => ServiceSelector(t, openGenericInterfaceType, openGenericImplementationTYpe))
                        .Configure(r => r.LifeStyle.Is(lifestyleType)));
            return container;
        }

        private static IEnumerable<Type> ServiceSelector(
            [NotNull] Type type,
            [NotNull] Type openGenericInterfaceType,
            [NotNull] Type openGenericImplementationTYpe)
        {
            HashSet<Type> matches = new HashSet<Type>();

            if (type == openGenericImplementationTYpe)
            {
                matches.Add(openGenericInterfaceType);
            }
            else
            {
                AddFromInterface(type, openGenericInterfaceType, matches);
            }

            return matches;
        }

        private static void AddFromInterface([NotNull] Type type, [NotNull] Type implements, [NotNull] ICollection<Type> matches)
        {
            foreach (Type @interface in GetTopLevelInterfaces(type))
            {
                if ((implements.FullName != null)
                    && (@interface.GetTypeInfo().GetInterface(implements.FullName, false) != null))
                {
                    matches.Add(@interface);
                }
            }
        }

        private static IEnumerable<Type> GetTopLevelInterfaces(Type type)
        {
            Type[] interfaces = type.GetInterfaces();
            List<Type> topLevel = new List<Type>(interfaces);

            foreach (Type @interface in interfaces)
            {
                foreach (Type parent in @interface.GetInterfaces())
                {
                    topLevel.Remove(parent);
                }
            }

            return topLevel;
        }
    }
}
