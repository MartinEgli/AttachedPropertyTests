// -----------------------------------------------------------------------
// <copyright file="DependencyObjectHelpers.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Markup.Primitives;

    using JetBrains.Annotations;

    public static class DependencyObjectHelpers
    {
        /// <summary>
        ///     Adds the value changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="property">The property.</param>
        /// <param name="valueChangedAction">The value changed action.</param>
        public static void AddValueChanged<T>(
            [NotNull] this DependencyObject sourceObject,
            [NotNull] DependencyProperty property,
            [NotNull] Action<T> valueChangedAction)
        {
            if (sourceObject == null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (valueChangedAction == null)
            {
                throw new ArgumentNullException(nameof(valueChangedAction));
            }

            void ValueChangedHandler(object sender, EventArgs args) =>
                valueChangedAction(sourceObject.GetValueSync<T>(property));

            //  var desc = DependencyPropertyDescriptor.FromProperty(property, typeof(AttachedFork<T, TOwner>));
            var desc = DependencyPropertyDescriptor.FromProperty(property, property.OwnerType);
            desc?.AddValueChanged(sourceObject, ValueChangedHandler);
        }

        /// <summary>
        ///     Finds the dependency property.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     target
        ///     or
        ///     propertyName
        /// </exception>
        [CanBeNull]
        public static DependencyProperty FindDependencyProperty(
            [NotNull] this DependencyObject target,
            [NotNull] string propertyName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var info = target.GetType()
                .GetField(propertyName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            if (info == null)
            {
                return null;
            }

            return (DependencyProperty)info.GetValue(null);
        }

        /// <summary>
        ///     Gets the dependency properties.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<DependencyProperty> GetDependencyProperties([NotNull] object element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return MarkupWriter.GetMarkupObjectFor(element)
                .Properties.Where(mp => mp.DependencyProperty != null)
                .Select(mp => mp.DependencyProperty)
                .ToList();
        }

        /// <summary>
        /// Gets the attached properties.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">element</exception>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<DependencyProperty> GetAttachedProperties([NotNull] object element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return MarkupWriter.GetMarkupObjectFor(element)
                .Properties.Where(mp => mp.IsAttached)
                .Select(mp => mp.DependencyProperty);
        }

        /// <summary>
        ///     Determines whether [has dependency property] [the specified property name].
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propName">Name of the property.</param>
        /// <returns>
        ///     <c>true</c> if [has dependency property] [the specified property name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDependencyProperty(this DependencyObject target, string propName)
        {
            return FindDependencyProperty(target, propName) != null;
        }

        /// <summary>
        ///     Determines whether [has dependency property] [the specified property].
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        ///     <c>true</c> if [has dependency property] [the specified property]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDependencyProperty(this DependencyObject target, DependencyProperty property)
        {
            var value = target.ReadLocalValue(property);
            return value != DependencyProperty.UnsetValue;
        }
    }
}