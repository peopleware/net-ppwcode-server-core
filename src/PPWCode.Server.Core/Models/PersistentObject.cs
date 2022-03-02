// Copyright 2020-2022 by PeopleWare n.v..
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using NHibernate.Classic;
using NHibernate.Proxy;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Server.Core.Models
{
    /// <inheritdoc cref="IPersistentObject" />
    [SuppressMessage("Design", "CA1033", Justification = "Reviewed: Explicit interface implementation on purpose.")]
    [DataContract(IsReference = true)]
    public abstract class PersistentObject
        : PersistentObject<long>,
          IPersistentObject,
          IEquatable<PersistentObject>
    {
        private int? _oldHashCode;

        protected PersistentObject(long id)
            : base(id)
        {
        }

        protected PersistentObject()
        {
        }

        /// <inheritdoc />
        public virtual bool Equals(PersistentObject other)
            => IsSame(other);

        void IValidatable.Validate()
        {
            ThrowIfNotCivilized();
        }

        public override bool IsSame(IIdentity<long> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (IsTransient || other.IsTransient)
            {
                return false;
            }

            if (!EqualityComparer<long>.Default.Equals(Id, other.Id))
            {
                return false;
            }

            Type otherType = NHibernateProxyHelper.GetClassWithoutInitializingProxy(other);
            Type thisType = NHibernateProxyHelper.GetClassWithoutInitializingProxy(this);
            return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
        }

        public override bool Equals(object obj)
            => IsSame(obj as PersistentObject);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Reviewed")]
        [SuppressMessage("ReSharper", "BaseObjectGetHashCodeCallInGetHashCode", Justification = "Reviewed")]
        public override int GetHashCode()
        {
            // Once we have a hash code we'll never change it
            if (_oldHashCode != null)
            {
                return _oldHashCode.Value;
            }

            // When this instance is transient, we use the base GetHashCode()
            // and remember it, so an instance can NEVER change its hash code.
            if (IsTransient)
            {
                _oldHashCode = base.GetHashCode();
                return _oldHashCode.Value;
            }

            return Id.GetHashCode();
        }

        public static bool operator ==(PersistentObject x, PersistentObject y)
            => Equals(x, y);

        public static bool operator !=(PersistentObject x, PersistentObject y)
            => !Equals(x, y);
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "NHibernate Mapper")]
    public abstract class PersistentObjectMapper<T> : PersistentObjectMapper<T, long>
        where T : PersistentObject
    {
    }
}
