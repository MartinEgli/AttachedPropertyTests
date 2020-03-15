// -----------------------------------------------------------------------
// <copyright file="ParentNotifiers.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Markup.Primitives;

    /// <summary>
    ///     A memory safe dictionary storage for <see cref="ParentChangedNotifier" /> instances.
    /// </summary>
    public class ParentNotifiers
    {
        /// <summary>
        ///     The inner
        /// </summary>
        private readonly Dictionary<WeakReference<DependencyObject>, WeakReference<ParentChangedNotifier>> inner =
            new Dictionary<WeakReference<DependencyObject>, WeakReference<ParentChangedNotifier>>();

        /// <summary>
        ///     Check, if it contains the key.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>True, if the key exists.</returns>
        public bool ContainsKey(DependencyObject target) =>
            this.inner.Keys.Any(x => x.TryGetTarget(out var t) && ReferenceEquals(t, target));

        /// <summary>
        ///     Removes the entry.
        /// </summary>
        /// <param name="target">The target object.</param>
        public void Remove(DependencyObject target)
        {
            var singleOrDefault = this.inner.Keys.SingleOrDefault(
                x =>
                    {
                        x.TryGetTarget(out var t);
                        return ReferenceEquals(t, target);
                    });

            if (singleOrDefault == null)
            {
                return;
            }

            {
                if (this.inner[singleOrDefault].TryGetTarget(out var t))
                {
                    t.Dispose();
                }
            }
            this.inner.Remove(singleOrDefault);
        }

        /// <summary>
        ///     Adds the key-value-pair.
        /// </summary>
        /// <param name="target">The target key object.</param>
        /// <param name="parentChangedNotifier">The notifier.</param>
        public void Add(DependencyObject target, ParentChangedNotifier parentChangedNotifier) =>
            this.inner.Add(
                new WeakReference<DependencyObject>(target),
                new WeakReference<ParentChangedNotifier>(parentChangedNotifier));
    }

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
            this DependencyObject sourceObject,
            DependencyProperty property,
            Action<T> valueChangedAction)
        {
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
        /// <param name="propName">Name of the property.</param>
        /// <returns></returns>
        public static DependencyProperty FindDependencyProperty(this DependencyObject target, string propName)
        {
            var info = target.GetType()
                .GetField(propName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            if (info == null)
            {
                return null;
            }

            return (DependencyProperty)info.GetValue(null);
        }

        public static List<DependencyProperty> GetDependencyProperties(object element)
        {
            var properties = new List<DependencyProperty>();
            var markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (var mp in markupObject.Properties)
                {
                    if (mp.DependencyProperty != null)
                    {
                        properties.Add(mp.DependencyProperty);
                    }
                }
            }

            return properties;
        }

        public static IEnumerable<DependencyProperty> GetAttachedProperties(object element)
        {
            var markupObject = MarkupWriter.GetMarkupObjectFor(element);

            return markupObject.Properties.Where(mp => mp.IsAttached).Select(mp => mp.DependencyProperty);
        }

        public static DependencyProperty FindDependencyProperty(
            this DependencyObject target,
            DependencyProperty property)
        {
            var info = target.GetType()
                .GetField(property.Name, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            if (info == null)
            {
                return null;
            }

            return (DependencyProperty)info.GetValue(null);
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