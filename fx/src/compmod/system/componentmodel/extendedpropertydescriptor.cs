//------------------------------------------------------------------------------
// <copyright file="ExtendedPropertyDescriptor.cs" company="Microsoft">
//     
//      Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//     
//      The use and distribution terms for this software are contained in the file
//      named license.txt, which can be found in the root of this distribution.
//      By using this software in any fashion, you are agreeing to be bound by the
//      terms of this license.
//     
//      You must not remove this notice, or any other, from this software.
//     
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {

    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Security.Permissions;
    
    /// <internalonly/>
    /// <devdoc>
    ///    <para>
    ///       This class wraps an PropertyDescriptor with something that looks like a property. It
    ///       allows you to treat extended properties the same as regular properties.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    internal sealed class ExtendedPropertyDescriptor : PropertyDescriptor {

        private readonly ReflectPropertyDescriptor      extenderInfo;       // the extender property
        private readonly IExtenderProvider provider;           // the guy providing it

        /// <devdoc>
        ///     Creates a new extended property info.  Callers can then treat this as
        ///     a standard property.
        /// </devdoc>
        public ExtendedPropertyDescriptor(ReflectPropertyDescriptor extenderInfo, Type receiverType, IExtenderProvider provider, Attribute[] attributes)
            : base(extenderInfo, attributes) {

            Debug.Assert(extenderInfo != null, "ExtendedPropertyDescriptor must have extenderInfo");
            Debug.Assert(provider != null, "ExtendedPropertyDescriptor must have provider");

            ArrayList attrList = new ArrayList(AttributeArray);
            attrList.Add(ExtenderProvidedPropertyAttribute.Create(extenderInfo, receiverType, provider));
            if (extenderInfo.IsReadOnly) {
                attrList.Add(ReadOnlyAttribute.Yes);
            }
            
            Attribute[] temp = new Attribute[attrList.Count];
            attrList.CopyTo(temp, 0);
            AttributeArray = temp;

            this.extenderInfo = extenderInfo;
            this.provider = provider;
        }

        public ExtendedPropertyDescriptor(PropertyDescriptor extender,  Attribute[] attributes) : base(extender, attributes) {
            Debug.Assert(extender != null, "The original PropertyDescriptor must be non-null");
            
            ExtenderProvidedPropertyAttribute attr = extender.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;

            Debug.Assert(attr != null, "The original PropertyDescriptor does not have an ExtenderProvidedPropertyAttribute");

            
            ReflectPropertyDescriptor reflectDesc = attr.ExtenderProperty as ReflectPropertyDescriptor;

            Debug.Assert(reflectDesc != null, "The original PropertyDescriptor has an invalid ExtenderProperty");

            this.extenderInfo = reflectDesc;
            this.provider = attr.Provider;
        }

        /// <devdoc>
        ///     Determines if the the component will allow its value to be reset.
        /// </devdoc>
        public override bool CanResetValue(object comp) {
            return extenderInfo.ExtenderCanResetValue(provider, comp);
        }

        /// <devdoc>
        ///     Retrieves the type of the component this PropertyDescriptor is bound to.
        /// </devdoc>
        public override Type ComponentType {
            get {
                return extenderInfo.ComponentType;
            }
        }
        
        /// <devdoc>
        ///     Determines if the property can be written to.
        /// </devdoc>
        public override bool IsReadOnly {
            get {
                return Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
            }
        }

        /// <devdoc>
        ///     Retrieves the data type of the property.
        /// </devdoc>
        public override Type PropertyType {
            get {
                return extenderInfo.ExtenderGetType(provider);
            }
        }

        /// <devdoc>
        ///     Retrieves the display name of the property.  This is the name that will
        ///     be displayed in a properties window.  This will be the same as the property
        ///     name for most properties.
        /// </devdoc>
        public override string DisplayName {
            get {
                string name = base.DisplayName;

                DisplayNameAttribute displayNameAttr = Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if (displayNameAttr == null || displayNameAttr.IsDefaultAttribute()) {
                    ISite site = GetSite(provider);
                    if (site != null) {
                        string providerName = site.Name;
                        if (providerName != null && providerName.Length > 0) {
                            name = SR.GetString(SR.MetaExtenderName, name, providerName);
                        }
                    }
                }   
                return name;
            }
        }

        /// <devdoc>
        ///     Retrieves the value of the property for the given component.  This will
        ///     throw an exception if the component does not have this property.
        /// </devdoc>
        public override object GetValue(object comp) {
            return extenderInfo.ExtenderGetValue(provider, comp);
        }

        /// <devdoc>
        ///     Resets the value of this property on comp to the default value.
        /// </devdoc>
        public override void ResetValue(object comp) {
            extenderInfo.ExtenderResetValue(provider, comp, this);
        }

        /// <devdoc>
        ///     Sets the value of this property on the given component.
        /// </devdoc>
        public override void SetValue(object component, object value) {
            extenderInfo.ExtenderSetValue(provider, component, value, this);
        }

        /// <devdoc>
        ///     Determines if this property should be persisted.  A property is
        ///     to be persisted if it is marked as persistable through a
        ///     PersistableAttribute, and if the property contains something other
        ///     than the default value.  Note, however, that this method will
        ///     return true for design time properties as well, so callers
        ///     should also check to see if a property is design time only before
        ///     persisting to runtime storage.
        /// </devdoc>
        public override bool ShouldSerializeValue(object comp) {
            return extenderInfo.ExtenderShouldSerializeValue(provider, comp);
        }



        /* 
           The following code has been removed to fix FXCOP violations.  The code
           is left here incase it needs to be resurrected in the future.

        /// <devdoc>
        ///     Creates a new extended property info.  Callers can then treat this as
        ///     a standard property.
        /// </devdoc>
        public ExtendedPropertyDescriptor(ReflectPropertyDescriptor extenderInfo, Type receiverType, IExtenderProvider provider) : this(extenderInfo, receiverType, provider, null) {
        }

        /// <devdoc>
        ///     Retrieves the object that is providing this extending property.
        /// </devdoc>
        public IExtenderProvider Provider {
            get {
                return provider;
            }
        }
        */

    }
}
