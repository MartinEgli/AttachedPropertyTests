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
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     The value.
        /// </returns>
        public static bool TryGetValueSync<T>(this DependencyObject obj, DependencyProperty property, out T value)
        {
            value = default;
            if (!obj.HasDependencyProperty(property))
            {
                return false;
            }

            value = obj.GetValueSync<T>(property);
            if (value != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the value synchronize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static T GetValueSync<T>(this DependencyObject obj, DependencyProperty property)
        {
            if (obj.CheckAccess())
            {
                return (T)obj.GetValue(property);
            }

            return (T)obj.Dispatcher.Invoke(() => obj.GetValue(property));
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