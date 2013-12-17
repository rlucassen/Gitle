namespace Gitle.Model
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NHibernate.Proxy;
    using NHibernate.Validator.Engine;

    #endregion

    public class ModelBase : IEquatable<ModelBase>
    {
        private readonly Guid guid = Guid.NewGuid();
// ReSharper disable UnassignedField.Local
        private long id;
// ReSharper restore UnassignedField.Local

        public virtual long Id
        {
            get { return id; }
        }

        public virtual Guid Guid
        {
            get { return guid; }
        }

        private bool isActive = true;

        public virtual bool IsActive
        {
            get { return isActive; }
        }

        public virtual void Deactivate()
        {
            isActive = false;
        }

        public virtual void Activate()
        {
            isActive = true;
        }

        public virtual bool IsValid()
        {
            var validator = NHibernate.Validator.Cfg.Environment.SharedEngineProvider.GetEngine();
            return validator.IsValid(this);
        }

        public virtual Dictionary<string, string> InvalidValues()
        {
            var validator = NHibernate.Validator.Cfg.Environment.SharedEngineProvider.GetEngine();
            return validator.Validate(this).ToDictionary(e => e.PropertyPath, e => e.Message);
        }

        #region Implementation of IEquatable<ModelBase>

        /// <summary>
        ///     Equalses the specified model base.
        /// </summary>
        /// <param name="modelBase">The model base.</param>
        /// <returns></returns>
        public virtual bool Equals(ModelBase modelBase)
        {
            if (modelBase == null) return false;
            return Id == modelBase.Id && Equals(Guid, modelBase.Guid);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.
        /// </param>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        ///     The <paramref name="obj" /> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || Equals(obj as ModelBase);
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ModelBase x, ModelBase y)
        {
            return Equals(x, y);
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ModelBase x, ModelBase y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Guid.GetHashCode();
                result = (result*397) ^ Id.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}