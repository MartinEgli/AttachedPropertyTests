using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AttachedPropertyTests
{
    public class AttachedBindingString : AttachedBinding<string>
    {

    }

    public class AttachedBinding<T> : DependencyObject

    {
        /// <summary>
        /// The setter property
        /// </summary>
        public static readonly DependencyProperty SetterProperty = DependencyProperty.RegisterAttached("Setter", typeof(T),
            typeof(AttachedBinding<T>), new PropertyMetadata(null, SetterChanged));

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
        public static Binding GetSetter(DependencyObject obj)
        {
            return obj.GetValueSync<Binding>(SetterProperty);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetSetter(DependencyObject obj, Binding value)
        {
            obj.SetValueSync(SetterProperty, value);
        }

        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        public T GetValueOrRegisterParentChanged(DependencyObject target, Action<T> parentChanged)
        {
            if (target == null)
                return FallbackValue;

            //target.RegisterDependencyPropertyChanged<AttachedBinding<T>>(SetterProperty);

            var value = target.GetValueOrRegisterParentNotifierX<T>(SetterProperty, this.ParentChangedAction, parentChanged,  parentNotifiers);
            if (value == null) { return FallbackValue; }

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

    public  class AttachedBindingStringGetterExtension : AttachedBindingGetterExtension<string>
    {
    }

    public abstract class AttachedBindingGetterExtension<T> : UpdatableMarkupExtension
    {
       
        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (provideValueTarget.TargetObject is DependencyObject dependencyObject)
            {
                return new AttachedBinding<T>().GetValueOrRegisterParentChanged(dependencyObject,(i) => this.UpdateValue(i));
            }
            return null;
        }
    }

    public abstract class UpdatableMarkupExtension : MarkupExtension
    {
        protected object TargetObject { get; private set; }

        protected object TargetProperty { get; private set; }

        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target)
            {
                TargetObject = target.TargetObject;
                TargetProperty = target.TargetProperty;
            }

            return ProvideValueInternal(serviceProvider);
        }

        protected void UpdateValue(object value)
        {
            if (TargetObject == null) return;
            if (TargetProperty is DependencyProperty)
            {
                DependencyObject obj = TargetObject as DependencyObject;
                DependencyProperty prop = TargetProperty as DependencyProperty;

                Action updateAction = () => obj.SetValue(prop, value);

                // Check whether the target object can be accessed from the
                // current thread, and use Dispatcher.Invoke if it can't

                if (obj.CheckAccess())
                    updateAction();
                else
                    obj.Dispatcher.Invoke(updateAction);
            }
            else // _targetProperty is PropertyInfo
            {
                PropertyInfo prop = TargetProperty as PropertyInfo;
                prop.SetValue(TargetObject, value, null);
            }
        }

        protected abstract object ProvideValueInternal(IServiceProvider serviceProvider);
    }
}
