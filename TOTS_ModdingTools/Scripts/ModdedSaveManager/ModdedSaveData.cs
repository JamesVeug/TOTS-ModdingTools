using System;
using System.Collections.Generic;

namespace TOTS_ModdingTools;

public class ModdedSaveData
{
    public class Field<T>
    {
        public string key;
        public T value;
    }
    
    public string guid;
    public string name;

    internal List<Field<int>> intValues = new List<Field<int>>();
    internal List<Field<string>> strValues = new List<Field<string>>();
        
    public ModdedSaveData(string guid, string name)
    {
        this.guid = guid;
        this.name = name;
    }

    public int GetIntValue(string key, int defaultValue = 0, bool addIfMissing = true)
    {
        foreach (Field<int> field in intValues)
        {
            if (field.key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return field.value;
            }
        }

        if (addIfMissing)
        {
            intValues.Add(new Field<int>(){ key = key, value = defaultValue });
        }
            
        return defaultValue;
    }

    public string GetStringValue(string key, string defaultValue = "", bool addIfMissing = true)
    {
        foreach (Field<string> field in strValues)
        {
            if (field.key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return field.value;
            }
        }

        if (addIfMissing)
        {
            strValues.Add(new Field<string>(){ key = key, value = defaultValue });
        }
            
        return defaultValue;
    }

    public void SetInt(string key, int value)
    {
        foreach (Field<int> field in intValues)
        {
            if (field.key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                field.value = value;
                return;
            }
        }
            
        intValues.Add(new Field<int> { key = key, value = value });
    }

    public void SetString(string key, string value)
    {
        foreach (Field<string> field in strValues)
        {
            if (field.key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                field.value = value;
                return;
            }
        }
            
        strValues.Add(new Field<string> { key = key, value = value });
    }
}