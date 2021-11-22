using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Security.Cryptography;


public static class Extension
{
	// List Extension
	public static void Shuffle<T>( this List<T> list )
	{
		RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
		int n = list.Count;
		while (n > 1)
		{
			byte[] box = new byte[1];
			do provider.GetBytes(box);
			while (!(box[0] < n * (byte.MaxValue / n)));
			int k = (box[0] % n);
			n--;

			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	// Array Extension 
	public static void Shuffle<T>( this T[] array )
	{
		RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
		int n = array.Length;
		while (n > 1)
		{
			byte[] box = new byte[1];
			do provider.GetBytes(box);
			while (!(box[0] < n * (byte.MaxValue / n)));
			int k = (box[0] % n);
			n--;

			T value = array[k];
			array[k] = array[n];
			array[n] = value;
		}
	}

	// Transform Extension
	public static void ChangeLayerReculsively(this Transform transform, string layerName)
	{
		int layer = LayerMask.NameToLayer(layerName);
		if(layer == -1)
			return;
		
		transform.ChangeLayerReculsively(layer);
	}

	// Transform Extension
	public static void ChangeLayerReculsively(this Transform transform, int layer)
	{
		transform.gameObject.layer = layer;
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).ChangeLayerReculsively(layer);
		}
	}

    // Transform Extension
    public static T Find<T>(this Transform trans, string path) where T : UnityEngine.Object
    {
        Transform findTrans = trans.Find(path);
        if (findTrans != null)
        {
            if (typeof(T) == typeof(GameObject))
                return findTrans.gameObject as T;
            else
                return findTrans.GetComponent<T>();
        }
        else
        {
            Debug.LogErrorFormat("not found object : find Type-{0} : path {1}", typeof(T).Name, path);
        }

        return null;
    }
}
