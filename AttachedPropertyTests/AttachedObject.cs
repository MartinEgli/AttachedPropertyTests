// -----------------------------------------------------------------------
// <copyright file="AttachedObject.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Data;
using System.Windows.Markup;

namespace AttachedPropertyTests
{
    using System;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TAttachedObject">The type of the attached object.</typeparam>
    /// <seealso cref="System.Windows.Markup.MarkupExtension" />
    public class AttachedGetterMarkupExtension<T, TAttachedObject> : MarkupExtension
        where TAttachedObject : AttachedObject<T>, new()
    {
        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if(provideValueTarget.TargetObject is DependencyObject dependencyObject)
            {
                return new TAttachedObject().GetValue(dependencyObject);
            }
            return null;
        }
    }


    public class AttachedStringGetterMarkupExtension : AttachedGetterMarkupExtension<string, AttachedString>
    {
      
    }

    public class
        AttachedDependencyPropertyGetterMarkupExtension : AttachedGetterMarkupExtension<Binding,
            AttachedDependencyProperty>
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (provideValueTarget.TargetObject is DependencyObject dependencyObject)
            {
                return new AttachedDependencyProperty().GetValue(dependencyObject)?.ProvideValue(serviceProvider);
            }
            return null;
        }
    }

    public class AttachedString : AttachedObject<string>{
    }

    public class AttachedDependencyProperty : AttachedObject<Binding>
    {
        
    }

    public class AttachedObject<T> : DependencyObject

    {
        /// <summary>
        /// The setter property
        /// </summary>
        public static readonly DependencyProperty SetterProperty = DependencyProperty.RegisterAttached("Setter", typeof(T), 
            typeof(AttachedObject<T>), new PropertyMetadata(null, SetterChanged));
        
        /// <summary>
        /// Setters the changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void SetterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //Instance.FallbackAssembly = e.NewValue?.ToString();
            //Instance.OnProviderChanged(obj);
        }

        /// <summary>
        /// To use when no assembly is specified.
        /// </summary>
        public T FallbackValue { get; set; }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static T GetSetter(DependencyObject obj)
        {
            return obj.GetValueSync<T>(SetterProperty);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetSetter(DependencyObject obj, T value)
        {
            obj.SetValueSync(SetterProperty, value);
        }

        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        public T GetValue(DependencyObject target)
        {
            if (target == null)
                return FallbackValue;

            var value = target.GetValueOrRegisterParentNotifier<T>(SetterProperty, ParentChangedAction, parentNotifiers);
            if( value == null){ return FallbackValue;}

            return value;
        }

        /// <summary>
        /// The parent notifiers
        /// </summary>
        private readonly ParentNotifiers parentNotifiers = new ParentNotifiers();

        /// <summary>
        /// Parents the changed action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ParentChangedAction(DependencyObject obj)
        {
            OnProviderChanged(obj);
        }

        /// <summary>
        /// Called when [provider changed].
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        private void OnProviderChanged(DependencyObject dependencyObject)
        {
        }
    }
}