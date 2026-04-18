using System;
using System.Reflection;

public static class ReflectionUtils
{
    public static bool TryGetAttribute<T>(this FieldInfo type, out T attribute) where T : Attribute
    {
        attribute = type.GetCustomAttribute<T>();
        return attribute != null;
    }
    
    public static FieldInfo GetPropertyBackingField(Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));
        
        
        var property = type.GetProperty(propertyName, 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        var backingField = property.DeclaringType.GetField($"<{property.Name}>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);

        return backingField;
    }
}