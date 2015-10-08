//This code is available under Creative Commons Attribution Share Alike: http://creativecommons.org/licenses/by-sa/3.0/.
//Author: Brandon Edmark
//Located at: http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset#ScriptableObjectUtility.cs

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
 
public static class ScriptableObjectUtility
{
	#if UNITY_EDITOR
	
	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static void CreateAsset<T> () where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();
 
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") 
		{
			path = "Assets";
		} 
		else if (Path.GetExtension (path) != "") 
		{
			path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
		}
 
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + typeof(T).ToString() + ".asset");
 
		AssetDatabase.CreateAsset (asset, assetPathAndName);
 
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}
	#endif
	
}