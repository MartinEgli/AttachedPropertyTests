// -----------------------------------------------------------------------
// <copyright file="ParentChangedNotifierHelper.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    public static class ParentChangedNotifierHelper
    {
        /// <summary>
        ///     Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject" />.
        ///     <para>If this is not available, it will register a <see cref="ParentChangedNotifier" /> on the last element.</para>
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject" />.</param>
        /// <param name="getFunction">The function that gets the value from a <see cref="DependencyObject" />.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>
        ///     The value, if possible.
        /// </returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            Func<DependencyObject, T> getFunction,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            return GetValueOrRegisterParentNotifier(target, getFunction, parentChangedAction, parentNotifiers, out _);
        }

        /// <summary>
        ///     Checks the type.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        private static bool CheckType(DependencyObject dependencyObject)
        {
            if (dependencyObject is System.Windows.Controls.ToolTip)
            {
                return true;
            }

            if (!(dependencyObject is Visual) && !(dependencyObject is Visual3D)
                                              && !(dependencyObject is FrameworkContentElement))
            {
                return true;
            }

            if (dependencyObject is Window)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the value or register parent notifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="getFunction">The get function.</param>
        /// <param name="parentChangedAction">The parent changed action.</param>
        /// <param name="parentNotifiers">The parent notifiers.</param>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     getFunction
        ///     or
        ///     parentChangedAction
        ///     or
        ///     parentNotifiers
        /// </exception>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            Func<DependencyObject, T> getFunction,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            out DependencyObject dependencyObject)
        {
            var result = default(T);
            dependencyObject = target;
            if (target == null)
            {
                return result;
            }

            if (getFunction == null)
            {
                throw new ArgumentNullException(nameof(getFunction));
            }

            if (parentChangedAction == null)
            {
                throw new ArgumentNullException(nameof(parentChangedAction));
            }

            if (parentNotifiers == null)
            {
                throw new ArgumentNullException(nameof(parentNotifiers));
            }

            return Loop(target, getFunction, parentChangedAction, parentNotifiers, out dependencyObject);
        }

        /// <summary>
        ///     Loops the specified target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="getFunction">The get function.</param>
        /// <param name="parentChangedAction">The parent changed action.</param>
        /// <param name="parentNotifiers">The parent notifiers.</param>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        private static T Loop<T>(
            DependencyObject target,
            Func<DependencyObject, T> getFunction,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            out DependencyObject dependencyObject)
        {
            T result;
            var weakTarget = new WeakReference(target);
            dependencyObject = target;

            do
            {
                result = getFunction(dependencyObject);

                if (result != null && parentNotifiers.ContainsKey(target))
                {
                    parentNotifiers.Remove(target);
                }

                if (CheckType(dependencyObject))
                {
                    break;
                }

                if (TryGetParent(dependencyObject, out var parent))
                {
                    break;
                }

                if (result != null)
                {
                    break;
                }

                if (parent == null)
                {
                    RegisterParentNotifier(target, parentChangedAction, parentNotifiers, dependencyObject, weakTarget);
                    break;
                }

                dependencyObject = parent;
            }
            while (true);

            return result;
        }

        /// <summary>
        ///     Registers the parent notifier.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parentChangedAction">The parent changed action.</param>
        /// <param name="parentNotifiers">The parent notifiers.</param>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="weakTarget">The weak target.</param>
        private static void RegisterParentNotifier(
            DependencyObject target,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            DependencyObject dependencyObject,
            WeakReference weakTarget)
        {
            if (dependencyObject is FrameworkElement frameworkElement && !parentNotifiers.ContainsKey(target))
            {
                var changedNotifier = new ParentChangedNotifier(
                    frameworkElement,
                    () =>
                        {
                            var localTarget = (DependencyObject)weakTarget.Target;
                            if (!weakTarget.IsAlive)
                            {
                                return;
                            }

                            parentChangedAction(localTarget);

                            if (parentNotifiers.ContainsKey(localTarget))
                            {
                                parentNotifiers.Remove(localTarget);
                            }
                        });

                parentNotifiers.Add(target, changedNotifier);
            }
        }

        /// <summary>
        ///     Tries the get parent.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        private static bool TryGetParent(DependencyObject dependencyObject, out DependencyObject parent)
        {
            if (dependencyObject is FrameworkContentElement element)
            {
                parent = element.Parent;
            }
            else
            {
                try
                {
                    parent = dependencyObject.GetParent(false);
                }
                catch
                {
                    parent = null;
                }
            }

            if (parent == null)
            {
                try
                {
                    parent = dependencyObject.GetParent(true);
                }
                catch
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject" />.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject" />.</param>
        /// <param name="getFunction">The function that gets the value from a <see cref="DependencyObject" />.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValue<T>(this DependencyObject target, Func<DependencyObject, T> getFunction)
        {
            var result = default(T);

            if (target == null)
            {
                return result;
            }

            var depObj = target;

            while (result == null)
            {
                // Try to get the value using the provided GetFunction.
                result = getFunction(depObj);

                // Try to get the parent using the visual tree helper. This may fail on some occations.
                if (!(depObj is Visual) && !(depObj is Visual3D) && !(depObj is FrameworkContentElement))
                {
                    break;
                }

                DependencyObject depObjParent;

                if (depObj is FrameworkContentElement element)
                {
                    depObjParent = element.Parent;
                }
                else
                {
                    try
                    {
                        depObjParent = depObj.GetParent(true);
                    }
                    catch
                    {
                        break;
                    }
                }

                // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                if (depObjParent == null && depObj is FrameworkElement)
                {
                    depObjParent = ((FrameworkElement)depObj).Parent;
                }

                if (result == null && depObjParent == null)
                {
                    break;
                }

                // Assign the parent to the current DependencyObject and start the next iteration.
                depObj = depObjParent;
            }

            return result;
        }

        /// <summary>
        ///     Tries to get a value from a <see cref="DependencyProperty" /> that is stored somewhere in the visual tree above
        ///     this <see cref="DependencyObject" />.
        ///     If this is not available, it will register a <see cref="ParentChangedNotifier" /> on the last element.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject" />.</param>
        /// <param name="property">A <see cref="DependencyProperty" /> that will be read out.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            return target.GetValueOrRegisterParentNotifier(
                depObj => depObj.GetValueSync<T>(property),
                parentChangedAction,
                parentNotifiers);
        }

        /// <summary>
        ///     Gets the value or register parent notifier x.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        /// <param name="parentChangedAction">The parent changed action.</param>
        /// <param name="valueChangedAction">The value changed action.</param>
        /// <param name="parentNotifiers">The parent notifiers.</param>
        /// <returns></returns>
        public static T GetValueOrRegisterParentNotifierX<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            Action<T> valueChangedAction,
            ParentNotifiers parentNotifiers)
        {
            var value = target.GetValueOrRegisterParentNotifier(
                depObj => depObj.GetValueSync<T>(property),
                parentChangedAction,
                parentNotifiers,
                out var parent);
            var desc = DependencyPropertyDescriptor.FromProperty(property, typeof(AttachedObject<T>));
            desc.AddValueChanged(parent, (sender, args) => valueChangedAction(parent.GetValueSync<T>(property)));
            return value;
        }

        /// <summary>
        ///     Gets the parent in the visual or logical tree.
        /// </summary>
        /// <param name="depObj">The dependency object.</param>
        /// <param name="isVisualTree">True for visual tree, false for logical tree.</param>
        /// <returns>The parent, if available.</returns>
        public static DependencyObject GetParent(this DependencyObject depObj, bool isVisualTree)
        {
            if (depObj.CheckAccess())
            {
                return GetParentInternal(depObj, isVisualTree);
            }

            return depObj.Dispatcher?.Invoke(() => GetParentInternal(depObj, isVisualTree));
        }

        /// <summary>
        ///     Gets the parent internal.
        /// </summary>
        /// <param name="depObj">The dep object.</param>
        /// <param name="isVisualTree">if set to <c>true</c> [is visual tree].</param>
        /// <returns></returns>
        private static DependencyObject GetParentInternal(DependencyObject depObj, bool isVisualTree)
        {
            if (isVisualTree)
            {
                return VisualTreeHelper.GetParent(depObj);
            }

            return LogicalTreeHelper.GetParent(depObj);
        }
    }
}