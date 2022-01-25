using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public abstract class ObservableObject
{

    private readonly Dictionary<string, object> _backingFieldValues = new();

    /// <summary>
    /// Gets a property value from the internal backing field
    /// </summary>
    protected T GetProperty<T>([CallerMemberName] string propertyName = null)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }
        object value;
        if (_backingFieldValues.TryGetValue(propertyName, out value))
        {
            return (T)value;
        }
        return default(T);
    }

    /// <summary>
    /// Saves a property value to the internal backing field
    /// </summary>
    protected bool SetProperty<T>(T newValue, [CallerMemberName] string propertyName = null)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }
        if (IsEqual(GetProperty<T>(propertyName), newValue)) return false;
        _backingFieldValues[propertyName] = newValue;
        return true;
    }

    private bool IsEqual<T>(T field, T newValue)
    {
        // Alternative: EqualityComparer<T>.Default.Equals(field, newValue);
        return Equals(field, newValue);
    }
}
