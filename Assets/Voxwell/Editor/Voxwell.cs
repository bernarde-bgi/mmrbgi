using UnityEngine;
using System.Collections;
using UnityEditor;


//General class for handling misc menus, maybe more later
public class Voxwell : Editor {

	[MenuItem("Assets/Create/Voxel Pallette")]
	public static void CreateVoxelPalletteAsset ()
	{
		ScriptableObjectUtility.CreateAsset<VoxelPallette> ();
	}

	[MenuItem("Assets/Create/Voxel Template")]
	public static void CreateVoxelTemplateAsset ()
	{
		ScriptableObjectUtility.CreateAsset<VoxelTemplate> ();
	}

	[MenuItem("GameObject/Voxwell/Voxel Structure")]
	public static void CreateVoxelStructure ()
	{
		var newgo = new GameObject();
		var newvs = newgo.AddComponent<VoxelStructure>();
		newvs.pallette = AssetDatabase.LoadAssetAtPath("Assets/Voxwell/Pallettes/Voxel Pallette.asset",typeof(VoxelPallette)) as VoxelPallette;
	}
}
