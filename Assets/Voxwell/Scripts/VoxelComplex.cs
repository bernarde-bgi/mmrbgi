using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
[SelectionBase]
public class VoxelComplex : MonoBehaviour {


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public VoxelStructure GetVoxelStructureAtPosition(Vector3 position){
		var childrenVS = new List<VoxelStructure>(GetComponentsInChildren<VoxelStructure>());
		return childrenVS.Find(m => m.transform.position == position);
	}
}
