using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VoxelTemplate : ScriptableObject {

	//Used to determine where in the display lists this template should be
	//Less = higher on the list.
	public int DisplayOrder = 0;

	//Used for vertex colors
	public Color32 color = Color.white;

	//determine whether or not to use the front face uvs for all faces
	public bool useFrontUvsForAllFaces = false;

	//Determine whether or not to draw the faces in the center of the voxel.
	//As opposed to drawing 'on the surface' which happens by default
	public bool drawFacesInCenter = false;

	//the size of each tile in the atlas. 
	//If our texture atlas is 16/16 tiles, then this will be 1/16, or .0625
	//If our texture atlas is 8x8 tiles, then this will be 1/8, or .125
	public Vector2 atlasScale = new Vector2(0.0625f, 0.0625f);

	//The uv offsets (used for uv-mapping) for each face
	//offset.x = percentage distance from left edge of the atlas
	//offset.y = percentage distance from bottom edge of the atlas
	public Vector2 UVOffsetFront;
	public Vector2 UVOffsetBack;	
	public Vector2 UVOffsetTop;
	public Vector2 UVOffsetBottom;
	public Vector2 UVOffsetLeft;
	public Vector2 UVOffsetRight;
	

	//determine whether or not to draw this voxel
	public bool shouldDraw = true;

	//determine whether or not to draw each individual face
	public bool drawFront = true;
	public bool drawBack = true;
	public bool drawTop = true;
	public bool drawBottom = true;
	public bool drawLeft = true;
	public bool drawRight = true;



	//public GameObject Special; //Coming Soon!


}
