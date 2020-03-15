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

    using JetBrains.Annotations;

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
        public bool ContainsKey([NotNull] DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return this.inner.Keys.Any(x => x.TryGetTarget(out var t) && ReferenceEquals(t, target));
        }

        /// <summary>
        ///     Removes the entry.
        /// </summary>
        /// <param name="target">The target object.</param>
        public void Remove([NotNull] DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

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
        public void Add([NotNull] DependencyObject target, [NotNull] ParentChangedNotifier parentChangedNotifier)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (parentChangedNotifier == null)
            {
                throw new ArgumentNullException(nameof(parentChangedNotifier));
            }

            this.inner.Add(
                new WeakReference<DependencyObject>(target),
                new WeakReference<ParentChangedNotifier>(parentChangedNotifier));
        }
    }
}