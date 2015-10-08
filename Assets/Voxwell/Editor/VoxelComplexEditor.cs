using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[ExecuteInEditMode]
[CustomEditor(typeof(VoxelComplex))]
public class VoxelComplexEditor : Editor {
	
	
	
	Vector2 currentMousePosition;

	
	void OnEnable(){

	}
	
	
	public override void OnInspectorGUI() {
//		var voxelComplex = (VoxelComplex)target;
		
		
		
	}

	
	//Here is where we handle any user interactions with the scene view
	void OnSceneGUI()
	{
	    //If we are here, we must have received some event. Grab it for later usage.
		Event e = Event.current;
		currentMousePosition = e.mousePosition;


		
		//small hack to ensure the mouse events aren't overridden by the default unity actions
		if (e.shift && e.type == EventType.layout){
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		}
		
		//If it's an undo or redo, just redraw.
		//This doesn't always seem to fire, I'm think there must be a race condition
		if (e.commandName == "UndoRedoPerformed") {
//			VoxelComplex voxelComplex = (VoxelComplex)target;
			Repaint ();
	    	return;
	    }

		if(e.type == EventType.mouseDown ) {

			Debug.Log("MOUSE DOWN");



		}

		if(e.type == EventType.mouseUp ) {

			Debug.Log("MOUSE UP");
			
			
		}
		
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha1){
			Debug.Log("LOCKING ALL CHILLINS");
			var voxelComplex = (VoxelComplex)target;
			SetLockOnAllChildren(true, voxelComplex);
			Repaint();
			e.Use ();
		}
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha2){
			var voxelComplex = (VoxelComplex)target;
			SetLockOnAllChildren(false, voxelComplex);
			Repaint();
			e.Use ();
		}
	}
	
	public void HandleClick(Vector2 mousePosition){



	}
	
	public void SetLockOnAllChildren(bool shouldLock, VoxelComplex complex){
			Debug.Log("Locking ");
		for(var i = 0;i<complex.transform.childCount;i++){
			var t = complex.transform.GetChild(i).gameObject;
			Debug.Log("Locking " + t.name);
			if(shouldLock){
//				t.hideFlags ^= HideFlags.NotEditable;
				t.hideFlags ^= HideFlags.HideInHierarchy;
				t.hideFlags ^= HideFlags.HideInInspector;
			}else{
//				t.hideFlags &= ~HideFlags.NotEditable;
				t.hideFlags &= ~HideFlags.HideInHierarchy;
				t.hideFlags &= ~HideFlags.HideInInspector;
			}


		}


	}
	
	
	
	
	
	
	

}