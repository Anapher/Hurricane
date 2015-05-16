using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hurricane.Model
{
    [Serializable]
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(T value, ref T field, Expression<Func<object>> property)
        {
            return SetProperty(value, ref field, GetPropertyName(property));
        }

        protected virtual bool SetProperty<T>(T value, ref T field, [CallerMemberName]string propertyName = null)
        {
            if (field == null || !field.Equals(value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPropertyChanged(Expression<Func<object>> property)
        {
            OnPropertyChanged(GetPropertyName(property));
        }

        protected string GetPropertyName(Expression<Func<object>> property)
        {
            var lambda = property as LambdaExpression;
            MemberExpression memberExpression;

            var unaryExpression = lambda.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            var propertyInfo = memberExpression?.Member as PropertyInfo;
            return propertyInfo?.Name ?? string.Empty;
        }
    }
}
