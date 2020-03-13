// -----------------------------------------------------------------------
// <copyright file="UpdatableMarkupExtension.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace AttachedPropertyTests
{
    /// <summary>
    /// </summary>
    /// <seealso cref="System.Windows.Markup.MarkupExtension" />
    public abstract class UpdatableMarkupExtension : MarkupExtension
    {
        /// <summary>
        /// Gets the target object.
        /// </summary>
        /// <value>
        /// The target object.
        /// </value>
        protected object TargetObject { get; private set; }

        /// <summary>
        /// Gets the target property.
        /// </summary>
        /// <value>
        /// The target property.
        /// </value>
        protected object TargetProperty { get; private set; }

        /// <summary>
        /// Provides the value.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target)
            {
                this.TargetObject = target.TargetObject;
                this.TargetProperty = target.TargetProperty;
            }

            return this.ProvideValueInternal(serviceProvider);
        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void UpdateValue(object value)
        {
            if (this.TargetObject == null)
            {
                return;
            }

            if (this.TargetProperty is DependencyProperty property)
            {
                var targetObject = this.TargetObject as DependencyObject;

                void UpdateAction() => targetObject.SetValue(property, value);

                // Check whether the target object can be accessed from the
                // current thread, and use Dispatcher.Invoke if it can't

                if (targetObject.CheckAccess())
                {
                    UpdateAction();
                }
                else
                {
                    targetObject.Dispatcher.Invoke((Action)UpdateAction);
                }
            }
            else // _targetProperty is PropertyInfo
            {
                var propertyInfo = this.TargetProperty as PropertyInfo;
                propertyInfo?.SetValue(this.TargetObject, value, null);
            }
        }

        /// <summary>
        /// Provides the value internal.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        protected abstract object ProvideValueInternal(IServiceProvider serviceProvider);
    }
}