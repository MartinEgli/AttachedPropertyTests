// -----------------------------------------------------------------------
// <copyright file="AttachedFork.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;

    public class AttachedFork<T, TOwner> : DependencyObject

    {
        /// <summary>
        ///     The setter property
        /// </summary>
        public static readonly DependencyProperty SetterProperty = DependencyProperty.RegisterAttached(
            "Setter",
            typeof(T),
            typeof(AttachedFork<T, TOwner>),
            new PropertyMetadata(SetterChanged));

        /// <summary>
        ///     Setters the changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void SetterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //Instance.FallbackAssembly = e.NewValue?.ToString();
            //Instance.OnProviderChanged(obj);
        }

        /// <summary>
        ///     Getter of <see cref="DependencyProperty" /> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static T GetSetter(DependencyObject obj) => obj.GetValueSync<T>(SetterProperty);

        /// <summary>
        ///     Setter of <see cref="DependencyProperty" /> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetSetter(DependencyObject obj, T value) => obj.SetValueSync(SetterProperty, value);

        /// <summary>
        ///     Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="sourceChanged">The parent changed.</param>
        /// <param name="fallbackValue">The fallback value.</param>
        /// <returns>
        ///     The assembly name, if available.
        /// </returns>
        public static T GetValueOrRegisterParentChanged(
            DependencyObject target,
            Action<T> sourceChanged,
            T fallbackValue)
        {
            if (target == null)
            {
                return fallbackValue;
            }

            var value = GetValueOrRegisterParentChanged(target, sourceChanged);
            if (value == null)
            {
                return fallbackValue;
            }

            return value;
        }

        /// <summary>
        ///     Gets the value or register parent changed.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="sourceChanged">The parent changed.</param>
        /// <returns></returns>
        public static T GetValueOrRegisterParentChanged(DependencyObject target, Action<T> sourceChanged)
        {
            var value = target.GetValueOrRegisterParentNotifierX<T, TOwner>(
                SetterProperty,
                ParentChangedAction,
                sourceChanged,
                new ParentNotifiers());

            return value;
        }

        /// <summary>
        ///     Parents the changed action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static void ParentChangedAction(DependencyObject obj)
        {
        }
    }
}