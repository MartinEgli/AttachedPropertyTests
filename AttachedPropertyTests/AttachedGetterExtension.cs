// -----------------------------------------------------------------------
// <copyright file="AttachedGetterExtension.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;
    using System.Windows.Markup;

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOwner"></typeparam>
    /// <seealso cref="System.Windows.Markup.MarkupExtension" />
    public abstract class AttachedGetterExtension<T, TOwner> : UpdatableMarkupExtension
    {
        /// <summary>
        ///     When implemented in a derived class, returns an object that is provided as the value of the target property for
        ///     this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        ///     The object value to set on the property where the extension is applied.
        /// </returns>
        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (provideValueTarget.TargetObject is DependencyObject dependencyObject)

            {
                ((FrameworkElement)dependencyObject).Loaded += this.OnLoaded;

                var value = AttachedFork<T, TOwner>.GetValueOrRegisterParentChanged(dependencyObject, this.Update);
                return value;
            }

            return null;
        }

        /// <summary>
        ///     Updates the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        private void Update(T value) => this.UpdateValue(value);

        /// <summary>
        ///     Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject dependencyObject)

            {
                this.Update(
                    AttachedFork<T, TOwner>.GetValueOrRegisterParentChanged(
                        dependencyObject,
                        i => this.UpdateValue(i)));
            }
        }
    }
}