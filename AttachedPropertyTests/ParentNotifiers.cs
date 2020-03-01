// -----------------------------------------------------------------------
// <copyright file="ParentNotifiers.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    /// <summary>
    ///     A memory safe dictionary storage for <see cref="ParentChangedNotifier" /> instances.
    /// </summary>
    public class ParentNotifiers
    {
        private readonly Dictionary<WeakReference<DependencyObject>, WeakReference<ParentChangedNotifier>> _inner =
            new Dictionary<WeakReference<DependencyObject>, WeakReference<ParentChangedNotifier>>();

        /// <summary>
        ///     Check, if it contains the key.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>True, if the key exists.</returns>
        public bool ContainsKey(DependencyObject target)
        {
            return this._inner.Keys.Any(x => x.TryGetTarget(out var t) && ReferenceEquals(t, target));
        }

        /// <summary>
        ///     Removes the entry.
        /// </summary>
        /// <param name="target">The target object.</param>
        public void Remove(DependencyObject target)
        {
            var singleOrDefault = this._inner.Keys.SingleOrDefault(
                x =>
                    {
                        x.TryGetTarget(out var t);
                        return ReferenceEquals(t, target);
                    });

            if (singleOrDefault != null)
            {
                if (this._inner[singleOrDefault].TryGetTarget(out var t))
                {
                    t.Dispose();
                }

                this._inner.Remove(singleOrDefault);
            }
        }

        /// <summary>
        ///     Adds the key-value-pair.
        /// </summary>
        /// <param name="target">The target key object.</param>
        /// <param name="parentChangedNotifier">The notifier.</param>
        public void Add(DependencyObject target, ParentChangedNotifier parentChangedNotifier)
        {
            this._inner.Add(
                new WeakReference<DependencyObject>(target),
                new WeakReference<ParentChangedNotifier>(parentChangedNotifier));
        }
    }
}