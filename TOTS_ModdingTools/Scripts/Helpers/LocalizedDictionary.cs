using System.Collections.Generic;
using UnityEngine;

public class LocalizedDictionary<K,V>
{
    private Dictionary<SystemLanguage, Dictionary<K,V>> _localizedData = new Dictionary<SystemLanguage, Dictionary<K,V>>();
    
    public void Add(SystemLanguage language, K key, V value)
    {
        if (!_localizedData.TryGetValue(language, out Dictionary<K,V> languageData))
        {
            languageData = new Dictionary<K,V>();
            _localizedData[language] = languageData;
        }
        
        languageData[key] = value;
    }

    public bool TryGetValue(SystemLanguage language, K key, out V value)
    {
        if (_localizedData.TryGetValue(language, out Dictionary<K,V> languageData))
        {
            return languageData.TryGetValue(key, out value);
        }
        
        value = default(V);
        return false;
    }
}