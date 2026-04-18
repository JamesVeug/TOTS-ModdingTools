using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace TOTS_ModdingTools.Helpers;

public static class ArrayExtensions
{
    public static void AddElements<T>(ref T[] array, T[] elements)
    {
        int startingIndex = array.Length;
        Array.Resize(ref array, startingIndex + elements.Length);
        for (int i = 0; i < elements.Length; i++)
        {
            array[startingIndex + i] = elements[i];
        }
    }
    
    public static void AddElements<T>(ref T[] array, List<T> elements)
    {
        int startingIndex = array.Length;
        Array.Resize(ref array, startingIndex + elements.Count);
        for (int i = 0; i < elements.Count; i++)
        {
            array[startingIndex + i] = elements[i];
        }
    }
    
    public static void AddElements<T>(ref T[] array, IEnumerable<T> elements)
    {
        List<T> list1 = ListPool<T>.Get();
        list1.AddRange(elements);
        AddElements(ref array, list1);
        ListPool<T>.Release(list1);
    }

    public static void AddElement<T>(ref T[] array, T element)
    {
        int startingIndex = array.Length;
        Array.Resize(ref array, startingIndex + 1);
        array[startingIndex] = element;
    }
    
    public static T[] Copy<T>(this T[] array)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        return copy;
    }
    
    public static bool NullOrEmpty<T>(this T[] array)
    {
        return array == null || array.Length == 0;
    }
    
    public static T[] ForceAdd<T>(this T[] arr, T element)
    {
        List<T> list = ((IEnumerable<T>) arr).ToList<T>();
        list.Add(element);
        return list.ToArray();
    }
    
    public static T SafeGet<T>(this T[] arr, int index, T defaultValue)
    {
        if (arr == null || index < 0 || index >= arr.Length)
        {
            return defaultValue;
        }
        return arr[index];
    }
}