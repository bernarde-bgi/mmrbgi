using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

[System.Serializable]
public class VoxelPallette : ScriptableObject {
	
	public Material AtlasMaterial;
	
	[SerializeField]
	public VoxelTemplate[] voxelTemplates = new VoxelTemplate[1];
	public List<KeyValuePair<int,int>> lookup = new List<KeyValuePair<int,int>>();
	public List<string> voxelNames = new List<string>();

	
	//For display/UX purposes, we want to have a sorted, filtered list.
	//To do this, we sort voxel templates by their display oder, while 
	//constructing a "lookup table" to refer to later when we need to 
	//get the "real" voxel template index in our voxelTemplates[] array
	public void ArrangeVoxelTemplates(){
		//wipe the list
		voxelNames = new List<string>();

		//sort the templates by their display order
		var sorted = voxelTemplates
			.Select((x, i) => new KeyValuePair<VoxelTemplate, int>(x, i))
			.OrderBy( x => { 
					if(x.Key != null) 
						return x.Key.DisplayOrder; 
					else
						return 100000;
					
				} )
			.ToList();
		
		//wipe the lookup
		lookup = new List<KeyValuePair<int,int>>();

		//construct the lookup table and the display list (voxelNames)
		for(var i = 0; i<sorted.Count;i++){
			if(sorted[i].Key!=null){
				lookup.Add(new KeyValuePair<int, int>(i, sorted[i].Value));
				voxelNames.Add(sorted[i].Key.name);
			}
		}
		

		
		
	}



	
}
