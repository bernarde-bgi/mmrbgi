using UnityEngine;
using System.Collections;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshCollider))]
[Serializable]
public class VoxelStructure : MonoBehaviour {
	
	[SerializeField]
	public VoxelPallette pallette;
	public bool UsePalletteTexture = true;
	
	//We number the voxeltypes so that if new ones are added, it won't throw off the voxel structure data
	public enum VoxelType{
	    Empty = 0, Stone = 1, Crate = 2,
		Dirt = 16, DirtWithGrass = 17, Grass = 18, Bark = 19, Leaves = 20, Flower = 31,
		Red = 240, Orange = 241, Yellow=242, Green=243, Blue=244, Purple=245, Pink=246, 
		Beige=251, Brown=252, White=253, Grey=254, Black=255,
		OutOfBounds = 256,
	}

	
	//The action enum, used to determine which action to take when interacting
	public enum Action{
		Add=0,
		Erase,
		Paint,
		MaxPull,
		Flood,
		None
	}

	public enum Neighbor{
		Front=0,
		Back=1,
		Left=2,
		Right=3,
		Top=4,
		Bottom=5
	}

	public enum VoxelFace{
		Front=0,
		Back,
		Left,
		Right,
		Top,
		Bottom
	}
	
	//The current voxel used in all commands
	//public VoxelType CurrentVoxelType = VoxelType.Stone;
	//The current action, used to determine how to hand user interactions
	public static Action LMB_Action = Action.Add;
	public static Action RMB_Action = Action.Erase;
	public static Action Static_Action = Action.Erase;
	
	//flatVoxels, the data structure behind it all. Using a flat array to represent a 3d array, in the hopes of some
	//slight optimizations. Further testing should be done to determine if that is really the case.
	[SerializeField]
	[HideInInspector]
	public int[] flatVoxels = new int[WIDTH*HEIGHT*DEPTH];
	
	public static int SelectedVoxelIndex = 1;
	public static int SelectedVoxelType = 1;
	
//	[SerializeField]
//	[HideInInspector]
//	Dictionary<int, VoxelType> flatVoxelDict = new Dictionary<int, VoxelType>();
	
	
	
	//The Effective size of the voxel structure. Should never exceed the static WIDTH, HEIGHT, or DEPTH values
	//It's possible to increase these to the point that the generated mesh would be too large, resulting in an error
	//In the future, the code will just create a new gameobject if this upper limit is reached.	
	public int Width 	= 16;
	public int Height 	= 16;
	public int Depth 	= 8;
	
	//Max, static voxel sizes. These could in theory be any size, but should be kept small as they determine the size of the flatcells array 
	//Used to provide some consistency and allow for the resizing of structures after they are created.
	//***These should be kept as small as possible, and must not change after any voxel structures have been created***
	//***This will be refactored in the future***
	private static int WIDTH  = 32;
	private static int HEIGHT = 32;
	private static int DEPTH  = 32;
	
	
	
	//Used to selectively turn on or off different voxel faces.
	//Useful optimization for 2d/2.5d games
	public bool DrawFront 	= true;
	public bool DrawBack 	= true;
	public bool DrawTop 	= true;
	public bool DrawBottom 	= true;
	public bool DrawLeft 	= true;
	public bool DrawRight 	= true;
	
	
	public bool GenerateSecondaryUvSet = false;
	
	
	//Used to store mesh data during construction
	Vector3[] verts;
	int[] tris;
	Vector2[] uvs;
	Color32[] colors;
	Vector4[] tans;

	[SerializeField]
	public VoxelStructure[] neighbors;

	public VoxelComplex voxelComplex;



	#region macro commands
	public void SolidCube(){
		for(int x = 0; x < Width; 	x++){
			for(int y = 0; y < Height; y++){
				for(int z = 0; z < Depth; z++){
					SetVoxel(x,y,z, SelectedVoxelType);
				}
			}
		}
		
	}
	public void Room(){
		for(int x = 0; x < Width; 	x++){
			for(int y = 0; y < Height; y++){
				for(int z = 0; z < Depth; z++){
					if(x==0 || x==Width-1 || y==0 || y==Height-1 || z == Depth-1)
						SetVoxel(x,y,z, SelectedVoxelType);
					else
						SetVoxel(x,y,z, 0);
				}
			}
		}
		
	}
	public void RoomNoBack(){
		for(int x = 0; x < Width; 	x++){
			for(int y = 0; y < Height; y++){
				for(int z = 0; z < Depth; z++){
					if(x==0 || x==Width-1 || y==0 || y==Height-1)
						SetVoxel(x,y,z, SelectedVoxelType);
					else
						SetVoxel(x,y,z, 0);
				}
			}
		}
		
	}
	public void ClearAll(){
		for(int x = 0; x < Width; 	x++){
			for(int y = 0; y < Height; y++){
				for(int z = 0; z < Depth; z++){
					SetVoxel(x,y,z, 0);
				}
			}
		}
	}
	
	
	
	
	public void SetAllNonEmpties(){
		for(int x = 0; x < Width; 	x++){
			for(int y = 0; y < Height; y++){
				for(int z = 0; z < Depth; z++){
					if(GetVoxel(x,y,z) != 0)
						SetVoxel(x,y,z, SelectedVoxelType);
				}
			}
		}
		
	}

	//Draw one vertical column on the front plane spaced 8 units apart
	public void PillarsFrontEight(){
		for(int x = 0; x < Width; x++){
			for(int y = 0; y < Height; y++){
				if(x%8==0)
					SetVoxel (x,y,0, SelectedVoxelType);
			}
		}
		
	}
	#endregion
	
	#region walls
	public void LeftWall(){
		for(int y = 0; y< Height; y++){
			for(int z = 0; z<Depth; z++){
				SetVoxel (0,y,z, SelectedVoxelType);
			}
		}
	}
	public void RightWall(){
		for(int y = 0; y < Height; y++){
			for(int z = 0; z < Depth; z++){
				SetVoxel (Width-1,y,z, SelectedVoxelType);
			}
		}
	}
	public void Floor(){
		for(int x = 0; x < Width; x++){
			for(int z = 0; z < Depth; z++){
				SetVoxel (x,0,z, SelectedVoxelType);
			}
		}
	}
	public void Ceiling(){
		for(int x = 0; x < Width; x++){
			for(int z = 0; z < Depth; z++){
				SetVoxel (x,Height-1,z, SelectedVoxelType);
			}
		}
	}
	public void BackWall(){
		for(int x = 0; x < Width; x++){
			for(int y = 0; y < Height; y++){
				SetVoxel (x,y,Depth-1, SelectedVoxelType);
			}
		}
	}
	public void FrontWall(){
		for(int x = 0; x < Width; x++){
			for(int y = 0; y < Height; y++){
				SetVoxel (x,y,0, SelectedVoxelType);
			}
		}
	}
	#endregion
	
	

	MeshFilter mf;
	MeshCollider mc;
	int f, k, r, l, t, b;
	VoxelTemplate fType, kType, rType, lType, tType, bType;
	
	int c;
	VoxelTemplate voxelType;
	Color32 voxelColor;
	
	
	//The magic:
	//Loop through each voxel, 
	//based on visibility/neighbors, 
	//determine which of the faces should be created,
	//if they should, get the verts, tris, and uvs for the face and
	//add it to the mesh.
	public void Draw(){
		
		if(pallette==null){
			Debug.LogError("No Pallette Selected");	
			
		}else{
			mf = GetComponent<MeshFilter>();
			mc = GetComponent<MeshCollider>();
			
		
			ClampDimensions();
			
		
			
			//Destroy all children
	//		int childs = transform.childCount;
	//        for (int i = childs - 1; i > 0; i--)
	//        {
	//            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
	//        }
			
			
			var currentStepCount = 1;
			var currentStepFour = currentStepCount * 4;
			var currentStepSix = currentStepCount * 6;
			
			if(verts==null || verts.Length!=Width*Height*Depth*24){
				verts  = new Vector3[Width*Height*Depth*24];
				tris   = new int    [Width*Height*Depth*36];
				uvs    = new Vector2[Width*Height*Depth*24];
				colors = new Color32[Width*Height*Depth*24];
				tans   = new Vector4[Width*Height*Depth*24];
			}else{
				Array.Clear(verts,0,verts.Length);
				Array.Clear(tris,0,tris.Length);
				Array.Clear(uvs,0,uvs.Length);
				Array.Clear(colors,0,colors.Length);
				Array.Clear(tans,0,tans.Length);
			}
			
			

			
			//The Main Drawing Loop. Lots of room for optimization,
			//but it seems to run quickly enough.
			for(int x = 0; x < Width; x++){
				for(int y = 0; y < Height; y++){
					for(int z = 0; z < Depth; z++){
						
						
						c = GetVoxel(x,y,z);
						voxelType = GetVoxelType(c);
						
						if(voxelType!= null && voxelType.shouldDraw){
							voxelColor = voxelType.color;
							
							if(!voxelType.drawFacesInCenter){
							
								#region voxel sides
								//Get all neighboring voxels to determine whether or not to draw the corresponding face
								f = GetVoxel(x,y,z-1);
								fType = GetVoxelType(f);
								k = GetVoxel(x,y,z+1);
								kType = GetVoxelType(k);
								r = GetVoxel(x+1,y,z);
								rType = GetVoxelType(r);
								l = GetVoxel(x-1,y,z);
								lType = GetVoxelType(l);
								t = GetVoxel(x,y+1,z);
								tType = GetVoxelType(t);
								b = GetVoxel(x,y-1,z);
								bType = GetVoxelType(b);
								
								
								
								//front face
								if(DrawFront && voxelType.drawFront && (fType == null || fType.drawFacesInCenter || !fType.shouldDraw )){
									
									
									verts[currentStepFour+0] = new Vector3(x,y,z);
									verts[currentStepFour+1] = new Vector3(x,y+1,z);
									verts[currentStepFour+2] = new Vector3(x+1,y+1,z);
									verts[currentStepFour+3] = new Vector3(x+1,y,z);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'f');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}
								//back face
								if(DrawBack && voxelType.drawBack && (kType==null || kType.drawFacesInCenter || !kType.shouldDraw)){
									verts[currentStepFour+0] = new Vector3(x+1,y,z+1);
									verts[currentStepFour+1] = new Vector3(x+1,y+1,z+1);
									verts[currentStepFour+2] = new Vector3(x,y+1,z+1);
									verts[currentStepFour+3] = new Vector3(x,y,z+1);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'k');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}
								//top
								if(DrawTop && voxelType.drawTop && (tType==null || tType.drawFacesInCenter || !tType.shouldDraw)){
									
									
									verts[currentStepFour+0] = new Vector3(x,y+1,z);
									verts[currentStepFour+1] = new Vector3(x,y+1,z+1);
									verts[currentStepFour+2] = new Vector3(x+1,y+1,z+1);
									verts[currentStepFour+3] = new Vector3(x+1,y+1,z);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 't');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}						
								//bottom
								if(DrawBottom && voxelType.drawBottom && (bType==null || bType.drawFacesInCenter  || !bType.shouldDraw)){
									
									verts[currentStepFour+3] = new Vector3(x,y,z);
									verts[currentStepFour+2] = new Vector3(x,y,z+1);
									verts[currentStepFour+1] = new Vector3(x+1,y,z+1);
									verts[currentStepFour+0] = new Vector3(x+1,y,z);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'b');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}	
								//right
								if(DrawRight && voxelType.drawRight && (rType==null || rType.drawFacesInCenter || !rType.shouldDraw)){
									
									verts[currentStepFour+0] = new Vector3(x+1,y,z);
									verts[currentStepFour+1] = new Vector3(x+1,y+1,z);
									verts[currentStepFour+2] = new Vector3(x+1,y+1,z+1);
									verts[currentStepFour+3] = new Vector3(x+1,y,z+1);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'r');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}	
								//left
								if(DrawLeft && voxelType.drawLeft && (lType==null || lType.drawFacesInCenter  || !lType.shouldDraw)){
									
									verts[currentStepFour+3] = new Vector3(x,y,z);
									verts[currentStepFour+2] = new Vector3(x,y+1,z);
									verts[currentStepFour+1] = new Vector3(x,y+1,z+1);
									verts[currentStepFour+0] = new Vector3(x,y,z+1);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'l');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}	
								#endregion
							
							}
							
							//for voxels marked as 'drawFacesInCenter'
							else{
								if(DrawFront && voxelType.drawFront){
									verts[currentStepFour+0] = new Vector3(x,y,z+.5f);
									verts[currentStepFour+1] = new Vector3(x,y+1,z+.5f);
									verts[currentStepFour+2] = new Vector3(x+1,y+1,z+.5f);
									verts[currentStepFour+3] = new Vector3(x+1,y,z+.5f);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'f');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}
								//back face
								if(DrawBack && voxelType.drawBack){
									verts[currentStepFour+3] = new Vector3(x,y,z+.5f);
									verts[currentStepFour+2] = new Vector3(x,y+1,z+.5f);
									verts[currentStepFour+1] = new Vector3(x+1,y+1,z+.5f);
									verts[currentStepFour+0] = new Vector3(x+1,y,z+.5f);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'k');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}
								//top
								if(DrawTop && voxelType.drawTop){
									
									
									verts[currentStepFour+0] = new Vector3(x,y+.5f,z);
									verts[currentStepFour+1] = new Vector3(x,y+.5f,z+1);
									verts[currentStepFour+2] = new Vector3(x+1,y+.5f,z+1);
									verts[currentStepFour+3] = new Vector3(x+1,y+.5f,z);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 't');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}						
								//bottom
								if(DrawBottom && voxelType.drawBottom){
									
									verts[currentStepFour+3] = new Vector3(x,y+.5f,z);
									verts[currentStepFour+2] = new Vector3(x,y+.5f,z+1);
									verts[currentStepFour+1] = new Vector3(x+1,y+.5f,z+1);
									verts[currentStepFour+0] = new Vector3(x+1,y+.5f,z);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'b');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}	
								//right
								if(DrawRight && voxelType.drawRight){
									
									verts[currentStepFour+0] = new Vector3(x+.5f,y,z);
									verts[currentStepFour+1] = new Vector3(x+.5f,y+1,z);
									verts[currentStepFour+2] = new Vector3(x+.5f,y+1,z+1);
									verts[currentStepFour+3] = new Vector3(x+.5f,y,z+1);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'r');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}	
								//left
								if(DrawLeft && voxelType.drawLeft){
									
									verts[currentStepFour+3] = new Vector3(x+.5f,y,z);
									verts[currentStepFour+2] = new Vector3(x+.5f,y+1,z);
									verts[currentStepFour+1] = new Vector3(x+.5f,y+1,z+1);
									verts[currentStepFour+0] = new Vector3(x+.5f,y,z+1);
									
									tris[currentStepSix+0] = currentStepFour+0;
									tris[currentStepSix+1] = currentStepFour+1;
									tris[currentStepSix+2] = currentStepFour+2;
									
									tris[currentStepSix+3] = currentStepFour+2;
									tris[currentStepSix+4] = currentStepFour+3;
									tris[currentStepSix+5] = currentStepFour+0;
									
									var myuvs = GetVoxelUvs(voxelType, 'l');
									uvs[currentStepFour+0] = myuvs[0];
									uvs[currentStepFour+1] = myuvs[1];
									uvs[currentStepFour+2] = myuvs[2];
									uvs[currentStepFour+3] = myuvs[3];
									
									colors[currentStepFour+0] = voxelColor;
									colors[currentStepFour+1] = voxelColor;
									colors[currentStepFour+2] = voxelColor;
									colors[currentStepFour+3] = voxelColor;
									
									tans[currentStepFour+0] = Vector4.zero;
									tans[currentStepFour+1] = Vector4.zero;
									tans[currentStepFour+2] = Vector4.zero;
									tans[currentStepFour+3] = Vector4.zero;
									
									currentStepCount ++;
									currentStepFour = 4 * currentStepCount;
									currentStepSix = 6 * currentStepCount;
								}	
								
								
								
							}
						}
							

						
						
						
					}
				}
			}
			
			//trim the arrays
			var subverts = verts.Take(currentStepCount*4).ToArray();
			
			if(subverts.Length<64000){
				
				if(UsePalletteTexture){
					if(pallette.AtlasMaterial!=null)
						GetComponent<Renderer>().material = pallette.AtlasMaterial;	
					else{
						Debug.LogWarning("There is no atlas material set on the Voxel Pallette. Will use current Renderer Material instead.");
					}
				}
				
				
				var subtris = tris.Take(currentStepCount*6).ToArray();
				var subuvs = uvs.Take(currentStepCount*4).ToArray();
				var subcolors = colors.Take(currentStepCount*4).ToArray();
				var subtans = tans.Take(currentStepCount*4).ToArray();
				
				
				var someMesh = mf.sharedMesh;
				if(someMesh!=null){
					someMesh.Clear();
				}else{
					someMesh = new Mesh();	
				}
		        someMesh.vertices = subverts;
		        someMesh.triangles = subtris;
				someMesh.uv = subuvs;
				someMesh.colors32 = subcolors;
		        someMesh.tangents = subtans;
				
				someMesh.RecalculateBounds();
		        someMesh.RecalculateNormals();
				
		        mf.sharedMesh = someMesh;

				#if UNITY_EDITOR
				if(GenerateSecondaryUvSet && subverts.Length>4){
					Unwrapping.GenerateSecondaryUVSet(mf.sharedMesh);
				}
				#endif

				mc.sharedMesh = null;
				mc.sharedMesh = someMesh;
			
			}
			else{
				Debug.LogError("You are attempting to create a structure that would require more than 64k vertices, which is the upper limit in Unity. Please adjust the size of your structure. In the future, this will no longer happen. I'm sorry!");	
			}
			
		
		
		}
		
		
		
		
		
		
		
		
		
	}

	
	#region getters and setters
	public int GetVoxel(int x, int y, int z){

		if(x >= Width || x < 0 || y>=Height || y< 0 || z >= Depth || z < 0)
			return -1;
		else {
			return flatVoxels[convert(x,y,z)];
		}
		
	}
					
	public int GetVoxel(Vector3 v){
		var x = (int)v.x;
		var y = (int)v.y;
		var z = (int)v.z;

		return GetVoxel(x,y,z);
	}
		
	public void SetVoxel(int x, int y, int z, int newVoxel){
		if(x>=Width){
			SetVoxelOnNeighbor(Neighbor.Right, 0, y, z, newVoxel);
		}
		else if(x<0){
			SetVoxelOnNeighbor(Neighbor.Left, Width-1, y, z, newVoxel);
		}

		else if(y>=Height){
			SetVoxelOnNeighbor(Neighbor.Top, x, 0, z, newVoxel);
		}
		else if(y<0){
			SetVoxelOnNeighbor(Neighbor.Bottom, x, Height-1, z, newVoxel);
		}


		else if(z>=Depth){
			SetVoxelOnNeighbor(Neighbor.Back, x, y, 0, newVoxel);
		}
		else if(z<0){
			SetVoxelOnNeighbor(Neighbor.Front, x, y, Depth-1, newVoxel);
		}

		else{
			flatVoxels[convert(x,y,z)] = newVoxel;
		}
	}
	public void SetVoxel(Vector3 v, int newVoxel){
			
		var x = (int)Math.Floor(v.x);
		var y = (int)Math.Floor(v.y);
		var z = (int)Math.Floor(v.z);
		SetVoxel(x,y,z, newVoxel);
		
	}
	public void SetVoxelOnNeighbor(Neighbor neighbor, int x, int y, int z, int newVoxel){


		Debug.Log("Setting Voxel On Neighbor");


		if(voxelComplex!=null){
			Vector3 newPosition = Vector3.zero;
			
			switch (neighbor){
			case Neighbor.Right:
				newPosition.x = Width*transform.localScale.x;
				break;
			case Neighbor.Left:
				newPosition.x-=Width*transform.localScale.x;
				break;
			case Neighbor.Front:
				newPosition.z-=Depth*transform.localScale.z;
				break;
			case Neighbor.Back:
				newPosition.z+=Depth*transform.localScale.z;
				break;
			case Neighbor.Top:
				newPosition.y+=Height*transform.localScale.y;
				break;
			case Neighbor.Bottom:
				newPosition.y-=Height*transform.localScale.y;
				break;
				
			}


			var neighbor_maybe = (VoxelStructure)voxelComplex.GetVoxelStructureAtPosition(transform.position+newPosition);
			if(neighbor_maybe==null){

				var newGo = new GameObject();
				var newStructure = newGo.AddComponent<VoxelStructure>();
				newStructure.voxelComplex = voxelComplex;
				newStructure.transform.parent = voxelComplex.transform;

				newStructure.Width = Width;
				newStructure.Height = Height;
				newStructure.Depth = Depth;
				
				newStructure.pallette = pallette;
				//
				newStructure.transform.localScale = transform.localScale;
				newStructure.transform.position = transform.position + newPosition;
				newStructure.SetVoxel(x,y,z,newVoxel);
				newStructure.Draw();

			}else{
				neighbor_maybe.SetVoxel(x,y,z,newVoxel);
				neighbor_maybe.Draw();
			}

			
		}

		

	}
	#endregion
	
	
	
	
	
	
	#region Special Actions
	public void MaxExtrude(Vector3 v, Vector3 dir){
		var nextVoxelPosition = v+dir;
		var nextVoxel = GetVoxel (nextVoxelPosition);
		while(nextVoxel == 0){
			SetVoxel(nextVoxelPosition, SelectedVoxelType);	
			nextVoxelPosition+=dir;
			nextVoxel = GetVoxel(nextVoxelPosition);
		}
		
	}
	public void FloodBasic(int y){
		for(int x = 0;x<Width;x++){
			for(int z= 0;z<Depth;z++){
				if(GetVoxel(x,y,z)==0)
					SetVoxel(x,y,z, SelectedVoxelType);
			}
		}
	}
	#endregion

	public void ExportMesh(){

		#if UNITY_EDITOR
		var path = EditorUtility.OpenFolderPanel("Select a folder to export your mesh","Assets","");
		Debug.Log(path);
		if (path.Length != 0) {
			var betterPath = path.Substring(path.IndexOf("Assets")) + "/";
			Debug.Log(betterPath);
			AssetDatabase.CreateAsset(gameObject.GetComponent<MeshFilter>().sharedMesh, betterPath + gameObject.name + ".asset"); 
		}
		#endif
		
	}
	
	
	
	
	
	//Utility method to convert an x,y,z coordinate into a 'flattened' integer
	private int convert(int x, int y, int z){
		return x + WIDTH * (y + (HEIGHT * z));
	}
	
	//Used for a bit of offset in the uvmapping if you find neighboring pixels are 'bleeding' into eachother
	private float ONE_PIXEL = .0002f;
	
	
	
	
	
	#region drawing helpers
	//Determine what UVs to use for a given voxel face.
	//This may seem odd, but I'm doing this here rather than storing this data in the voxel class, since we want that class to be as small as possible.
	//This function only runs during mesh building, whereas storing all of this data would mean Unity uses a lot more memory... I think!
	//TODO: Abstract this to an 'atlas position' like 0,14 instead of 0f/16f, 14f/16f
	Vector2[] GetVoxelUvs(VoxelTemplate voxelType, char dir){
		
		if(pallette){
			
			//var voxelType = GetVoxelType(voxelType);//pallette.voxelTypes[voxelType];
			if(voxelType!=null){
				if(dir=='f' || voxelType.useFrontUvsForAllFaces){
					uvs[0].x=voxelType.UVOffsetFront.x;
					uvs[0].y=voxelType.UVOffsetFront.y;
					
					uvs[1].x=voxelType.UVOffsetFront.x;
					uvs[1].y=voxelType.UVOffsetFront.y + voxelType.atlasScale.y;
					
					uvs[2].x=voxelType.UVOffsetFront.x + voxelType.atlasScale.x;
					uvs[2].y=voxelType.UVOffsetFront.y + voxelType.atlasScale.y;
					
					uvs[3].x=voxelType.UVOffsetFront.x + voxelType.atlasScale.x;
					uvs[3].y=voxelType.UVOffsetFront.y;
				}else if(dir=='k'){
					uvs[0].x=voxelType.UVOffsetBack.x;
					uvs[0].y=voxelType.UVOffsetBack.y;
					
					uvs[1].x=voxelType.UVOffsetBack.x;
					uvs[1].y=voxelType.UVOffsetBack.y + voxelType.atlasScale.y;
					
					uvs[2].x=voxelType.UVOffsetBack.x + voxelType.atlasScale.x;
					uvs[2].y=voxelType.UVOffsetBack.y + voxelType.atlasScale.y;
					
					uvs[3].x=voxelType.UVOffsetBack.x + voxelType.atlasScale.x;
					uvs[3].y=voxelType.UVOffsetBack.y;
				}else if(dir=='l'){
					uvs[0].x=voxelType.UVOffsetLeft.x;
					uvs[0].y=voxelType.UVOffsetLeft.y;
					
					uvs[1].x=voxelType.UVOffsetLeft.x;
					uvs[1].y=voxelType.UVOffsetLeft.y + voxelType.atlasScale.y;
					
					uvs[2].x=voxelType.UVOffsetLeft.x + voxelType.atlasScale.x;
					uvs[2].y=voxelType.UVOffsetLeft.y + voxelType.atlasScale.y;
					
					uvs[3].x=voxelType.UVOffsetLeft.x + voxelType.atlasScale.x;
					uvs[3].y=voxelType.UVOffsetLeft.y;
				}else if(dir=='r'){
					uvs[0].x=voxelType.UVOffsetRight.x;
					uvs[0].y=voxelType.UVOffsetRight.y;
					
					uvs[1].x=voxelType.UVOffsetRight.x;
					uvs[1].y=voxelType.UVOffsetRight.y + voxelType.atlasScale.y;
					
					uvs[2].x=voxelType.UVOffsetRight.x + voxelType.atlasScale.x;
					uvs[2].y=voxelType.UVOffsetRight.y + voxelType.atlasScale.y;
					
					uvs[3].x=voxelType.UVOffsetRight.x + voxelType.atlasScale.x;
					uvs[3].y=voxelType.UVOffsetRight.y;
				}else if(dir=='t'){
					uvs[0].x=voxelType.UVOffsetTop.x;
					uvs[0].y=voxelType.UVOffsetTop.y;
					
					uvs[1].x=voxelType.UVOffsetTop.x;
					uvs[1].y=voxelType.UVOffsetTop.y + voxelType.atlasScale.y;
					
					uvs[2].x=voxelType.UVOffsetTop.x + voxelType.atlasScale.x;
					uvs[2].y=voxelType.UVOffsetTop.y + voxelType.atlasScale.y;
					
					uvs[3].x=voxelType.UVOffsetTop.x + voxelType.atlasScale.x;
					uvs[3].y=voxelType.UVOffsetTop.y;
				}else if(dir=='b'){
					uvs[0].x=voxelType.UVOffsetBottom.x;
					uvs[0].y=voxelType.UVOffsetBottom.y;
					
					uvs[1].x=voxelType.UVOffsetBottom.x;
					uvs[1].y=voxelType.UVOffsetBottom.y + voxelType.atlasScale.y;
					
					uvs[2].x=voxelType.UVOffsetBottom.x + voxelType.atlasScale.x;
					uvs[2].y=voxelType.UVOffsetBottom.y + voxelType.atlasScale.y;
					
					uvs[3].x=voxelType.UVOffsetBottom.x + voxelType.atlasScale.x;
					uvs[3].y=voxelType.UVOffsetBottom.y;
				}
			}
		}
		
		
		
		
		//Uncomment this if you want to use the offset from above
        //bl
        uvs[0].x += ONE_PIXEL;
        uvs[0].y += ONE_PIXEL;

        //tl
        uvs[1].x += ONE_PIXEL;
        uvs[1].y -= ONE_PIXEL;

        //tr
        uvs[2].x -= ONE_PIXEL;
        uvs[2].y -= ONE_PIXEL;

        //br
        uvs[3].x -= ONE_PIXEL;
        uvs[3].y += ONE_PIXEL;


		return uvs;
	}
	bool isFlat(int voxelType){
		return false;
//		switch ((VoxelType)voxelType){
//			case VoxelType.Flower:
//
//				return true;
//			default:
//				return false;
//			
//		}
		
	}
	
	VoxelTemplate GetVoxelType(int v){
		if(v<pallette.voxelTemplates.Length && v>0){
			return pallette.voxelTemplates[v];	
		}else{
			return null;
		}
	}

	
	public void ClampDimensions(){
		if(Width>WIDTH)
			Width = WIDTH;
		if(Height>HEIGHT)
			Height = HEIGHT;
		if(Depth>DEPTH)
			Depth = DEPTH;
		
		if(Width<0)
			Width = 0;
		if(Height<0)
			Height = 0;
		if(Depth<0)
			Depth = 0;		
	}
	#endregion
}
