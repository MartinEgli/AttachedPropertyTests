// -----------------------------------------------------------------------
// <copyright file="INotifierRegister.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System.Windows;

    public interface INotifierRegister
    {
        void Remove(DependencyObject target);

        void Add(DependencyObject target, DependencyObject dependencyObject);
    }
}