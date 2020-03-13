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
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// <para>If this is not available, it will register a <see cref="ParentChangedNotifier"/> on the last element.</para>
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="getFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            Func<DependencyObject, T> getFunction,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            var result = default(T);
            if (target == null) return result;
            if (getFunction == null) throw new ArgumentNullException(nameof(getFunction));
            if (parentChangedAction == null) throw new ArgumentNullException(nameof(parentChangedAction));
            if (parentNotifiers == null) throw new ArgumentNullException(nameof(parentNotifiers));
            
            var dependencyObject = target;
            var weakTarget = new WeakReference(target);

            while (result == null)
            {
                // Try to get the value using the provided GetFunction.
                result = getFunction(dependencyObject);

                if (result != null && parentNotifiers.ContainsKey(target))
                    parentNotifiers.Remove(target);

                // Try to get the parent using the visual tree helper. This may fail on some occations.
                if (dependencyObject is System.Windows.Controls.ToolTip)
                    break;

                if (!(dependencyObject is Visual) && !(dependencyObject is Visual3D) && !(dependencyObject is FrameworkContentElement))
                    break;

                if (dependencyObject is Window)
                    break;

                DependencyObject parent;

                if (dependencyObject is FrameworkContentElement element)
                    parent = element.Parent;
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
                        break;
                    }
                }

                //// If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                //if (parent == null && dependencyObject is FrameworkElement element)
                //    parent = element.Parent;

                if (result == null && parent == null)
                {
                    // Try to establish a notification on changes of the Parent property of dp.
                    if (dependencyObject is FrameworkElement frameworkElement && !parentNotifiers.ContainsKey(target))
                    {
                        var changedNotifier = new ParentChangedNotifier(frameworkElement, () =>
                            {
                                var localTarget = (DependencyObject)weakTarget.Target;
                                if (!weakTarget.IsAlive) return;

                                // Call the action...
                                parentChangedAction(localTarget);
                                // ...and remove the notifier - it will probably not be used again.
                                if (parentNotifiers.ContainsKey(localTarget))
                                    parentNotifiers.Remove(localTarget);
                            });

                        parentNotifiers.Add(target, changedNotifier);
                    }
                    break;
                }

                // Assign the parent to the current DependencyObject and start the next iteration.
                dependencyObject = parent;
            }
            return result;
        }


        public static T GetValueOrRegisterParentNotifier<T>(
           this DependencyObject target,
           Func<DependencyObject, T> getFunction,
           Action<DependencyObject> parentChangedAction,
           ParentNotifiers parentNotifiers, out DependencyObject dependencyObject)
        {
            var result = default(T);
            dependencyObject = target;
            if (target == null) return result;
            if (getFunction == null) throw new ArgumentNullException(nameof(getFunction));
            if (parentChangedAction == null) throw new ArgumentNullException(nameof(parentChangedAction));
            if (parentNotifiers == null) throw new ArgumentNullException(nameof(parentNotifiers));

            dependencyObject = target;
            var weakTarget = new WeakReference(target);

            while (result == null)
            {
                // Try to get the value using the provided GetFunction.
                result = getFunction(dependencyObject);

                if (result != null && parentNotifiers.ContainsKey(target))
                    parentNotifiers.Remove(target);

                // Try to get the parent using the visual tree helper. This may fail on some occations.
                if (dependencyObject is System.Windows.Controls.ToolTip)
                    break;

                if (!(dependencyObject is Visual) && !(dependencyObject is Visual3D) && !(dependencyObject is FrameworkContentElement))
                    break;

                if (dependencyObject is Window)
                    break;

                DependencyObject parent;

                if (dependencyObject is FrameworkContentElement element)
                    parent = element.Parent;
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
                        break;
                    }
                }

                //// If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                //if (parent == null && dependencyObject is FrameworkElement element)
                //    parent = element.Parent;

                if (result == null && parent == null)
                {
                    // Try to establish a notification on changes of the Parent property of dp.
                    if (dependencyObject is FrameworkElement frameworkElement && !parentNotifiers.ContainsKey(target))
                    {
                        var changedNotifier = new ParentChangedNotifier(frameworkElement, () =>
                        {
                            var localTarget = (DependencyObject)weakTarget.Target;
                            if (!weakTarget.IsAlive) return;

                            // Call the action...
                            parentChangedAction(localTarget);
                            // ...and remove the notifier - it will probably not be used again.
                            if (parentNotifiers.ContainsKey(localTarget))
                                parentNotifiers.Remove(localTarget);
                        });

                        parentNotifiers.Add(target, changedNotifier);
                    }
                    break;
                }

                // Assign the parent to the current DependencyObject and start the next iteration.
                if (result == null)
                {
                    dependencyObject = parent;
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="getFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValue<T>(this DependencyObject target, Func<DependencyObject, T> getFunction)
        {
            var ret = default(T);

            if (target != null)
            {
                var depObj = target;

                while (ret == null)
                {
                    // Try to get the value using the provided GetFunction.
                    ret = getFunction(depObj);

                    // Try to get the parent using the visual tree helper. This may fail on some occations.
                    if (!(depObj is Visual) && !(depObj is Visual3D) && !(depObj is FrameworkContentElement))
                        break;

                    DependencyObject depObjParent;

                    if (depObj is FrameworkContentElement element)
                        depObjParent = element.Parent;
                    else
                    {
                        try { depObjParent = depObj.GetParent(true); }
                        catch { break; }
                    }
                    // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                    if (depObjParent == null && depObj is FrameworkElement)
                        depObjParent = ((FrameworkElement)depObj).Parent;

                    if (ret == null && depObjParent == null)
                        break;

                    // Assign the parent to the current DependencyObject and start the next iteration.
                    depObj = depObjParent;
                }
            }

            return ret;
        }

        /// <summary>
        /// Tries to get a value from a <see cref="DependencyProperty"/> that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// If this is not available, it will register a <see cref="ParentChangedNotifier"/> on the last element.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="property">A <see cref="DependencyProperty"/> that will be read out.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            return target.GetValueOrRegisterParentNotifier(depObj => depObj.GetValueSync<T>(property), parentChangedAction, parentNotifiers);
        }

        public static T GetValueOrRegisterParentNotifierX<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            Action<T> valueChangedAction,

            ParentNotifiers parentNotifiers)
        {
            var value = target.GetValueOrRegisterParentNotifier(depObj => depObj.GetValueSync<T>(property), parentChangedAction, parentNotifiers, out var parent);
            var desc = DependencyPropertyDescriptor.FromProperty(property, typeof(AttachedObject<T>));
            var v =  desc.GetValue(parent);
            desc.AddValueChanged(parent, (sender, args) => valueChangedAction(parent.GetValueSync<T>(property)));

            return value;
        }
        
        /// <summary>
        /// Gets the parent in the visual or logical tree.
        /// </summary>
        /// <param name="depObj">The dependency object.</param>
        /// <param name="isVisualTree">True for visual tree, false for logical tree.</param>
        /// <returns>The parent, if available.</returns>
        public static DependencyObject GetParent(this DependencyObject depObj, bool isVisualTree)
        {
            if (depObj.CheckAccess())
                return GetParentInternal(depObj, isVisualTree);

            return (DependencyObject)depObj.Dispatcher.Invoke(new Func<DependencyObject>(() => GetParentInternal(depObj, isVisualTree)));
        }

        /// <summary>
        /// Gets the parent internal.
        /// </summary>
        /// <param name="depObj">The dep object.</param>
        /// <param name="isVisualTree">if set to <c>true</c> [is visual tree].</param>
        /// <returns></returns>
        private static DependencyObject GetParentInternal(DependencyObject depObj, bool isVisualTree)
        {
            if (isVisualTree)
                return VisualTreeHelper.GetParent(depObj);

            return LogicalTreeHelper.GetParent(depObj);
        }
    }
}