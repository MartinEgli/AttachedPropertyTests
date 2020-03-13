﻿// -----------------------------------------------------------------------
// <copyright file="AttachedObject.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System.Windows;

    public class AttachedObject<T> : DependencyObject

    {
        /// <summary>
        ///     The setter property
        /// </summary>
        public static readonly DependencyProperty SetterProperty = DependencyProperty.RegisterAttached(
            "Setter",
            typeof(T),
            typeof(AttachedObject<T>),
            new PropertyMetadata(null, SetterChanged));

        /// <summary>
        ///     The parent notifiers
        /// </summary>
        private readonly ParentNotifiers parentNotifiers = new ParentNotifiers();

        /// <summary>
        ///     To use when no assembly is specified.
        /// </summary>
        public T FallbackValue { get; set; }

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
        /// <returns>The assembly name, if available.</returns>
        public T GetValue(DependencyObject target)
        {
            if (target == null)
            {
                return this.FallbackValue;
            }

            var value = target.GetValueOrRegisterParentNotifier<T>(
                SetterProperty,
                this.ParentChangedAction,
                this.parentNotifiers);
            if (value == null)
            {
                return this.FallbackValue;
            }

            return value;
        }

        /// <summary>
        ///     Parents the changed action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ParentChangedAction(DependencyObject obj)
        {
        }
    }
}