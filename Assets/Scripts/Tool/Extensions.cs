#region

using System.Linq;
using UnityEngine;

#endregion

public static class GameObjectExtension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (!comp)
        {
            comp = go.AddComponent<T>();
        }

        return comp;
    }

    public static T GetOrAddComponent<T>(this Component comp) where T : Component
    {
        var otherComp = comp.gameObject.GetComponent<T>();
        if (!otherComp)
        {
            otherComp = comp.gameObject.AddComponent<T>();
        }

        return otherComp;
    }

    public static T FindObjectOfType<T>() where T : Component
    {
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None).First();
    }

    public static T[] FindObjectsOfType<T>() where T : Component
    {
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
    }
}