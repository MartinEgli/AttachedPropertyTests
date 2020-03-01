// -----------------------------------------------------------------------
// <copyright file="TestClass.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Reflection;
    using System.Windows;

    public class TestClass : DependencyObject

    {
        public static readonly DependencyProperty TestProperty = DependencyProperty.RegisterAttached("Test", typeof(string), typeof(TestClass));

        public static void SetTest(UIElement element, string value)

        {
            element.SetValue(TestProperty, value);
        }

        public static string GetTest(UIElement element)

        {
            return (string)element.GetValue(TestProperty);
        }

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaultAssembly",
                typeof(string),
                typeof(string),
                new PropertyMetadata(null, DefaultAssemblyChanged));

        /// <summary>
        /// Indicates, that the <see cref="DefaultAssemblyProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void DefaultAssemblyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Instance.FallbackAssembly = e.NewValue?.ToString();
            Instance.OnProviderChanged(obj);
        }

        /// <summary>
        /// To use when no assembly is specified.
        /// </summary>
        public string FallbackAssembly { get; set; }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj)
        {
            return obj.GetValueSync<string>(DefaultAssemblyProperty);
        } /// <summary>

          /// Returns the <see cref="AssemblyName"/> of the passed assembly instance
          /// </summary>
          /// <param name="assembly">The Assembly where to get the name from</param>
          /// <returns>The Assembly name</returns>
        protected string GetAssemblyName(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.FullName == null)
            {
                throw new NullReferenceException("assembly.FullName is null");
            }

            return assembly.FullName.Split(',')[0];
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value)
        {
            obj.SetValueSync(DefaultAssemblyProperty, value);
        }

        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected string GetAssembly(DependencyObject target)
        {
            if (target == null)
                return FallbackAssembly;

            var assembly = target.GetValueOrRegisterParentNotifier<string>(DefaultAssemblyProperty, ParentChangedAction, _parentNotifiers);
            return string.IsNullOrEmpty(assembly) ? FallbackAssembly : assembly;
        }

        private readonly ParentNotifiers _parentNotifiers = new ParentNotifiers();

        private void ParentChangedAction(DependencyObject obj)
        {
            OnProviderChanged(obj);
        }

        private void OnProviderChanged(DependencyObject dependencyObject)
        {
        }
    }
}