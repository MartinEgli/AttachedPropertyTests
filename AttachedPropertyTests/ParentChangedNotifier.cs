// -----------------------------------------------------------------------
// <copyright file="ParentChangedNotifier.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     A class that helps listening to changes on the Parent property of FrameworkElement objects.
    /// </summary>
    public class ParentChangedNotifier : DependencyObject, IDisposable
    {
        /// <summary>
        ///     An attached property that will take over control of change notification.
        /// </summary>
        public static DependencyProperty ParentProperty = DependencyProperty.RegisterAttached(
            "Parent",
            typeof(DependencyObject),
            typeof(ParentChangedNotifier),
            new PropertyMetadata(ParentChanged));

        /// <summary>
        ///     A static list of actions that should be performed on parent change events.
        ///     <para>- Entries are added by each call of the constructor.</para>
        ///     <para>- All elements are called by the parent changed callback with the particular sender as the key.</para>
        /// </summary>
        private static readonly Dictionary<WeakReference, List<Action>> OnParentChangedList =
            new Dictionary<WeakReference, List<Action>>();

        /// <summary>
        ///     The element this notifier is bound to. Needed to release the binding and Action entry.
        /// </summary>
        private WeakReference element;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="element">The element whose Parent property should be listened to.</param>
        /// <param name="onParentChanged">The action that will be performed upon change events.</param>
        public ParentChangedNotifier(FrameworkElement element, Action onParentChanged)
        {
            this.element = new WeakReference(element);

            if (onParentChanged != null)
            {
                if (!OnParentChangedList.ContainsKey(this.element))
                {
                    var foundOne = false;

                    foreach (var key in OnParentChangedList.Keys)
                    {
                        if (!ReferenceEquals(key.Target, element))
                        {
                            continue;
                        }

                        this.element = key;
                        foundOne = true;
                        break;
                    }

                    if (!foundOne)
                    {
                        OnParentChangedList.Add(this.element, new List<Action>());
                    }
                }

                OnParentChangedList[this.element].Add(onParentChanged);
            }

            if (element.CheckAccess())
            {
                this.SetBinding();
            }
            else
            {
                element.Dispatcher?.Invoke(this.SetBinding);
            }
        }

        #region ParentChanged callback

        /// <summary>
        ///     The callback for changes of the attached Parent property.
        /// </summary>
        /// <param name="obj">The sender.</param>
        /// <param name="args">The argument.</param>
        private static void ParentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!(obj is FrameworkElement notifier))
            {
                return;
            }

            var weakNotifier =
                OnParentChangedList.Keys.SingleOrDefault(x => x.IsAlive && ReferenceEquals(x.Target, notifier));

            if (weakNotifier == null)
            {
                return;
            }

            var list = new List<Action>(OnParentChangedList[weakNotifier]);
            foreach (var onParentChanged in list)
            {
                onParentChanged();
            }

            list.Clear();
        }

        #endregion ParentChanged callback

        /// <summary>
        ///     Sets the binding.
        /// </summary>
        private void SetBinding()
        {
            var binding = new Binding("Parent") { RelativeSource = new RelativeSource() };
            binding.RelativeSource.Mode = RelativeSourceMode.FindAncestor;
            binding.RelativeSource.AncestorType = typeof(FrameworkElement);
            BindingOperations.SetBinding((FrameworkElement)this.element.Target, ParentProperty, binding);
        }

        /// <summary>
        ///     Disposes all used resources of the instance.
        /// </summary>
        public void Dispose()
        {
            var weakElement = this.element;
            var weakElementReference = weakElement.Target;

            if (weakElementReference == null || !weakElement.IsAlive)
            {
                return;
            }

            try
            {
                ((FrameworkElement)weakElementReference).ClearValue(ParentProperty);

                if (OnParentChangedList.ContainsKey(weakElement))
                {
                    var list = OnParentChangedList[weakElement];
                    list.Clear();
                    OnParentChangedList.Remove(weakElement);
                }
            }
            finally
            {
                this.element = null;
            }
        }

        #region Parent property

        /// <summary>
        ///     Get method for the attached property.
        /// </summary>
        /// <param name="element">The target FrameworkElement object.</param>
        /// <returns>The target's parent FrameworkElement object.</returns>
        public static FrameworkElement GetParent(FrameworkElement element)
        {
            return element.GetValueSync<FrameworkElement>(ParentProperty);
        }

        /// <summary>
        ///     Set method for the attached property.
        /// </summary>
        /// <param name="element">The target FrameworkElement object.</param>
        /// <param name="value">The target's parent FrameworkElement object.</param>
        public static void SetParent(FrameworkElement element, FrameworkElement value)
        {
            element.SetValueSync(ParentProperty, value);
        }

        #endregion Parent property
    }
}