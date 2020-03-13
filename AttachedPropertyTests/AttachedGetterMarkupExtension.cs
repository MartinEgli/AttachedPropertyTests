// -----------------------------------------------------------------------
// <copyright file="AttachedGetterMarkupExtension.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;
    using System.Windows.Markup;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TAttachedObject">The type of the attached object.</typeparam>
    /// <seealso cref="System.Windows.Markup.MarkupExtension" />
    public class AttachedGetterMarkupExtension<T, TAttachedObject> : MarkupExtension
        where TAttachedObject : AttachedObject<T>, new()
    {
        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if(provideValueTarget.TargetObject is DependencyObject dependencyObject)
            {
                return new TAttachedObject().GetValue(dependencyObject);
            }
            return null;
        }
    }
}