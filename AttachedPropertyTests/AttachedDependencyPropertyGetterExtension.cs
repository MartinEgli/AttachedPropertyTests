// -----------------------------------------------------------------------
// <copyright file="AttachedDependencyPropertyGetterExtension.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class AttachedDependencyPropertyGetterExtension : AttachedGetterMarkupExtension<Binding,
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
}