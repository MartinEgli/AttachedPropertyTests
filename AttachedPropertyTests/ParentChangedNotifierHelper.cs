// -----------------------------------------------------------------------
// <copyright file="ParentChangedNotifierHelper.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    using JetBrains.Annotations;

    public static class ParentChangedNotifierHelper
    {
        /// <summary>
        ///     TryGetFunc
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public delegate bool TryGetFunc<TResult>(in DependencyObject dependencyObject, out TResult result);

        /// <summary>
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="parameter1">The parameter1.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public delegate bool TryGetFunc<T1, TResult>(
            in DependencyObject dependencyObject,
            in T1 parameter1,
            out TResult result);

        /// <summary>
        ///     Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject" />.
        ///     <para>If this is not available, it will register a <see cref="ParentChangedNotifier" /> on the last element.</para>
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject" />.</param>
        /// <param name="getFunc">The function that gets the value from a <see cref="DependencyObject" />.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>
        ///     The value, if possible.
        /// </returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            TryGetFunc<T> getFunc,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            return GetValueOrRegisterParentNotifier(target, getFunc, parentChangedAction, parentNotifiers, out _);
        }

        /// <summary>
        ///     Checks the type.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        private static bool CheckType(this DependencyObject dependencyObject)
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
        ///     Gets the value or register sourceObject notifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="tryGetPropertyValue">The get function.</param>
        /// <param name="parentChangedAction">The sourceObject changed action.</param>
        /// <param name="parentNotifiers">The sourceObject notifiers.</param>
        /// <param name="sourceObject">The dependency object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     tryGetPropertyValue
        ///     or
        ///     parentChangedAction
        ///     or
        ///     parentNotifiersRegister
        /// </exception>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            TryGetFunc<T> tryGetPropertyValue,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            out DependencyObject sourceObject)
        {
            var result = default(T);
            sourceObject = target;
            if (target == null)
            {
                return result;
            }

            if (tryGetPropertyValue == null)
            {
                throw new ArgumentNullException(nameof(tryGetPropertyValue));
            }

            if (parentChangedAction == null)
            {
                throw new ArgumentNullException(nameof(parentChangedAction));
            }

            if (parentNotifiers == null)
            {
                throw new ArgumentNullException(nameof(parentNotifiers));
            }

            return Loop(target, tryGetPropertyValue, parentChangedAction, parentNotifiers, out sourceObject);
        }

        /// <summary>
        ///     Gets the value or register parent notifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        /// <param name="parentChangedAction">The parent changed action.</param>
        /// <param name="parentNotifiers">The parent notifiers.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     parentChangedAction
        ///     or
        ///     parentNotifiers
        /// </exception>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            out DependencyObject sourceObject)
        {
            var result = default(T);
            sourceObject = target;
            if (target == null)
            {
                return result;
            }

            if (parentChangedAction == null)
            {
                throw new ArgumentNullException(nameof(parentChangedAction));
            }

            if (parentNotifiers == null)
            {
                throw new ArgumentNullException(nameof(parentNotifiers));
            }

            return WalkTreeUp<T>(target, property, parentChangedAction, parentNotifiers, out sourceObject);
            //            return Loop(target, property, parentChangedAction, parentNotifiersRegister, out sourceObject);
        }

        /// <summary>
        ///     Loops the specified target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="tryGetPropertyValue">The get function.</param>
        /// <param name="parentChangedAction">The sourceObject changed action.</param>
        /// <param name="parentNotifiers">The sourceObject notifiers.</param>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        private static T Loop<T>(
            this DependencyObject target,
            TryGetFunc<T> tryGetPropertyValue,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            out DependencyObject dependencyObject)
        {
            T result;
            dependencyObject = target;

            do
            {
                var hasResult = tryGetPropertyValue(dependencyObject, out result);

                if (hasResult && result != null && parentNotifiers.ContainsKey(target))
                {
                    parentNotifiers.Remove(target);
                }

                if (dependencyObject.CheckType())
                {
                    break;
                }

                if (dependencyObject.TryGetParent(out var parent))
                {
                    break;
                }

                if (hasResult)
                {
                    break;
                }

                if (parent == null)
                {
                    parentNotifiers.RegisterParentNotifier(target, dependencyObject, parentChangedAction);
                    break;
                }

                dependencyObject = parent;
            }
            while (true);

            return result;
        }

        private static T WalkTreeUp<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers,
            out DependencyObject dependencyObject)
        {
            dependencyObject = target;

            do
            {
                var hasResult = dependencyObject.HasDependencyProperty(property);

                if (hasResult)
                {
                    parentNotifiers.UnregisterParentNotifier(target);
                }

                if (dependencyObject.CheckType())
                {
                    break;
                }

                if (dependencyObject.TryGetParent(out var parent))
                {
                    break;
                }

                if (hasResult)
                {
                    break;
                }

                if (parent == null)
                {
                    parentNotifiers.RegisterParentNotifier(target, dependencyObject, parentChangedAction);
                    break;
                }

                dependencyObject = parent;
            }
            while (true);

            return dependencyObject.GetValueSync<T>(property);
        }

        /// <summary>
        ///     Loops the tree up.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        /// <param name="parentNotifiersRegister">The parent notifiers register.</param>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        private static T LoopTreeUp<T>(
            this DependencyObject target,
            DependencyProperty property,
            INotifierRegister parentNotifiersRegister,
            out DependencyObject dependencyObject)
        {
            dependencyObject = target;

            do
            {
                var hasResult = dependencyObject.HasDependencyProperty(property);

                if (hasResult)
                {
                    parentNotifiersRegister.Remove(target);
                }

                if (dependencyObject.CheckType())
                {
                    break;
                }

                if (dependencyObject.TryGetParent(out var parent))
                {
                    break;
                }

                if (hasResult)
                {
                    break;
                }

                if (parent == null)
                {
                    parentNotifiersRegister.Add(target, dependencyObject);
                    break;
                }

                dependencyObject = parent;
            }
            while (true);

            return dependencyObject.GetValueSync<T>(property);
        }

        /// <summary>
        ///     Unregisters the parent notifier.
        /// </summary>
        /// <param name="parentNotifiers">The parent notifiers.</param>
        /// <param name="target">The target.</param>
        private static void UnregisterParentNotifier(this ParentNotifiers parentNotifiers, DependencyObject target)
        {
            if (parentNotifiers.ContainsKey(target))
            {
                parentNotifiers.Remove(target);
            }
        }

        /// <summary>
        ///     Registers the sourceObject notifier.
        /// </summary>
        /// <param name="parentNotifiers">The sourceObject notifiers.</param>
        /// <param name="target">The target.</param>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="parentChangedAction">The sourceObject changed action.</param>
        private static void RegisterParentNotifier(
            this ParentNotifiers parentNotifiers,
            DependencyObject target,
            DependencyObject dependencyObject,
            Action<DependencyObject> parentChangedAction)
        {
            if (!(dependencyObject is FrameworkElement frameworkElement) || parentNotifiers.ContainsKey(target))
            {
                return;
            }

            var weakTarget = new WeakReference(target);

            void OnParentChangedHandler()
            {
                var localTarget = (DependencyObject)weakTarget.Target;
                if (!weakTarget.IsAlive)
                {
                    return;
                }

                parentChangedAction(localTarget);
                parentNotifiers.UnregisterParentNotifier(localTarget);

                //    if (parentNotifiers.ContainsKey(localTarget))
                //    {
                //        parentNotifiers.Remove(localTarget);
                //    }
            }

            var changedNotifier = new ParentChangedNotifier(frameworkElement, OnParentChangedHandler);

            parentNotifiers.Add(target, changedNotifier);
        }

        /// <summary>
        ///     Tries the get sourceObject.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="parent">The sourceObject.</param>
        /// <returns></returns>
        private static bool TryGetParent(
            [NotNull] this DependencyObject dependencyObject,
            [CanBeNull] out DependencyObject parent)
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

                // Try to get the sourceObject using the visual tree helper. This may fail on some occations.
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
                if (depObjParent == null && depObj is FrameworkElement frameworkElement)
                {
                    depObjParent = frameworkElement.Parent;
                }

                if (result == null && depObjParent == null)
                {
                    break;
                }

                // Assign the sourceObject to the current DependencyObject and start the next iteration.
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
            return target.GetValueOrRegisterParentNotifier<T>(
                property,
                parentChangedAction,
                parentNotifiers,
                out target);
            //return target.GetValueOrRegisterParentNotifier(
            //    (in DependencyObject dependencyObject, out T result) =>
            //        dependencyObject.TryGetValueSync(property, out result),
            //    parentChangedAction,
            //    parentNotifiersRegister);
        }

        /// <summary>
        ///     Gets the value or register sourceObject notifier x.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        /// <param name="parentChangedAction">The sourceObject changed action.</param>
        /// <param name="valueChangedAction">The value changed action.</param>
        /// <param name="parentNotifiers">The sourceObject notifiers.</param>
        /// <returns></returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            Action<T> valueChangedAction,
            ParentNotifiers parentNotifiers)
        {
            var value = target.GetValueOrRegisterParentNotifier(
                (in DependencyObject dependencyObject, out T result) =>
                    dependencyObject.TryGetValueSync(property, out result),
                parentChangedAction,
                parentNotifiers,
                out var sourceObject);

            sourceObject.AddValueChanged(property, valueChangedAction);
            return value;
        }

        /// <summary>
        ///     Gets the sourceObject in the visual or logical tree.
        /// </summary>
        /// <param name="depObj">The dependency object.</param>
        /// <param name="isVisualTree">True for visual tree, false for logical tree.</param>
        /// <returns>The sourceObject, if available.</returns>
        [CanBeNull]
        public static DependencyObject GetParent([NotNull] this DependencyObject depObj, bool isVisualTree)
        {
            if (depObj == null)
            {
                throw new ArgumentNullException(nameof(depObj));
            }

            if (depObj.CheckAccess())
            {
                return GetParentInternal(depObj, isVisualTree);
            }

            return depObj.Dispatcher?.Invoke(() => GetParentInternal(depObj, isVisualTree));
        }

        /// <summary>
        ///     Gets the sourceObject internal.
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