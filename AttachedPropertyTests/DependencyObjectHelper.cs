// -----------------------------------------------------------------------
// <copyright file="DependencyObjectHelper.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;

    /// <summary>
    ///     Extension methods for dependency objects.
    /// </summary>
    public static class DependencyObjectHelper
    {
        /// <summary>
        ///     Gets the value thread-safe.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value.</returns>
        public static bool TryGetValueSync<T>(this DependencyObject obj, DependencyProperty property, out T value)
        {
            value = default;
            if (obj.HasDependencyProperty(property))
            {
                value = GetValueSync<T>(obj, property);
                if (value != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static T GetValueSync<T>(this DependencyObject obj, DependencyProperty property)
        {
            if (obj.CheckAccess())
            {
                return (T)obj.GetValue(property);
            }

            return (T)obj.Dispatcher.Invoke(() => obj.GetValue(property));
        }

        public static void RegisterDependencyPropertyChanged<T>(this DependencyObject obj, DependencyProperty property)
        {
            var desc = DependencyPropertyDescriptor.FromProperty(property, typeof(T));
            desc.AddValueChanged(obj, Handler);
        }

        private static void Handler(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///     Sets the value thread-safe.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public static void SetValueSync<T>(this DependencyObject obj, DependencyProperty property, T value)
        {
            if (obj.CheckAccess())
            {
                obj.SetValue(property, value);
            }
            else
            {
                obj.Dispatcher.Invoke(() => obj.SetValue(property, value));
            }
        }
    }
}