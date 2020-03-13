// -----------------------------------------------------------------------
// <copyright file="AttachedBindingGetterExtension.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;
    using System.Windows.Markup;

    public abstract class AttachedBindingGetterExtension<T> : UpdatableMarkupExtension
    {
        /// <summary>
        ///     Provides the value internal.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (provideValueTarget.TargetObject is DependencyObject dependencyObject)
            {
                var attachedBinding = new AttachedBinding<T>();
                var value = attachedBinding.GetValueOrRegisterParentChanged(dependencyObject, i => this.UpdateValue(i));
                return value;
            }

            return null;
        }
    }
}