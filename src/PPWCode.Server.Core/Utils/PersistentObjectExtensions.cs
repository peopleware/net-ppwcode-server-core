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

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Utils
{
    public static class PersistentObjectExtensions
    {
        public static TParent GetParentWithId<TChild, TParent, TIdentity>(this TChild child, Func<TChild, TParent> getParent, TIdentity parentId)
            where TChild : class, IPersistentObject<TIdentity>
            where TParent : class, IPersistentObject<TIdentity>
            where TIdentity : struct, IEquatable<TIdentity>
        {
            if (child == null)
            {
                return default;
            }

            TParent parent = getParent(child);
            if (parent == null)
            {
                return default;
            }

            return
                EqualityComparer<TIdentity>.Default.Equals(parent.Id, parentId)
                    ? parent
                    : default;
        }
    }
}
