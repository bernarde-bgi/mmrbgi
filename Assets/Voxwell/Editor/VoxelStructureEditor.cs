using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[ExecuteInEditMode]
[CustomEditor(typeof(VoxelStructure))]
public class VoxelStructureEditor : Editor {
	
	//used for determining a 'mouse up' event
	bool wasLeftMouseDown = false;
	bool wasRightMouseDown = false;
	
	int sprayPaintThrottle = 100;
	DateTime lastSprayPaintAction;
	
	Vector2 currentMousePosition;

	
	void OnEnable(){

	}
	
	
	void Update () {
		var view = SceneView.currentDrawingSceneView; 
		if(view!=null && Camera.current!=null){
			if(wasLeftMouseDown){
				var currentTime = DateTime.Now;
				var dTime = currentTime.Subtract(lastSprayPaintAction);
				
				if(dTime.Milliseconds>sprayPaintThrottle){
					lastSprayPaintAction=currentTime;
					HandleClick(currentMousePosition, VoxelStructure.LMB_Action);
				}
			}
			if(wasRightMouseDown){
				var currentTime = DateTime.Now;
				var dTime = currentTime.Subtract(lastSprayPaintAction);
				
				if(dTime.Milliseconds>sprayPaintThrottle){
					lastSprayPaintAction=currentTime;
					HandleClick(currentMousePosition, VoxelStructure.RMB_Action);
				}
			}
		}

	}
	public override void OnInspectorGUI() {
		var voxelStructure = (VoxelStructure)target;
		
		
		voxelStructure.ClampDimensions();
		
		voxelStructure.pallette = (VoxelPallette)EditorGUILayout.ObjectField("Pallette", voxelStructure.pallette, typeof(VoxelPallette), false);
		EditorGUILayout.Separator();

		//Make sure we actually have a pallette, 
		//otherwise do nothing and display a message
		if(voxelStructure.pallette){
			voxelStructure.pallette.ArrangeVoxelTemplates();

			voxelStructure.voxelComplex = (VoxelComplex)EditorGUILayout.ObjectField("Voxel Complex", voxelStructure.voxelComplex, typeof(VoxelComplex));

			var currentVoxelType = EditorPrefs.GetInt("CurrentVoxelType");
			var voxelNames = voxelStructure.pallette.voxelNames.ToArray();


			EditorPrefs.SetInt("CurrentVoxelType", 
				EditorGUILayout.Popup("Current Voxel Type", currentVoxelType,  voxelNames)
			);

			//This is the tricky bit. 
			//We've figured out which item in the list has been selected, 
			//but that list is an altered, sorted version of the array in our VoxelPallette.
			//If we want to know what the REAL index of the voxel template in that array,
			//we have to consult the pallette.lookup, which is constructed by pallette.ArrangeVoxelTemplates()
			VoxelStructure.SelectedVoxelType = voxelStructure.pallette.lookup[EditorPrefs.GetInt("CurrentVoxelType")].Value;



		
			
			EditorGUILayout.Separator();
			
			EditorPrefs.SetInt("LMBAction", (int)(VoxelStructure.Action)EditorGUILayout.EnumPopup("LMB Action", (VoxelStructure.Action)EditorPrefs.GetInt("LMBAction")));
			VoxelStructure.LMB_Action = (VoxelStructure.Action)EditorPrefs.GetInt("LMBAction");
			
			EditorPrefs.SetInt("RMBAction", (int)(VoxelStructure.Action)EditorGUILayout.EnumPopup("RMB Action", (VoxelStructure.Action)EditorPrefs.GetInt("RMBAction")));
			VoxelStructure.RMB_Action = (VoxelStructure.Action)EditorPrefs.GetInt("RMBAction");
			
			EditorGUILayout.Separator();
			
			voxelStructure.Width = EditorGUILayout.IntField("w:", voxelStructure.Width);
			voxelStructure.Height = EditorGUILayout.IntField("h:", voxelStructure.Height);
			voxelStructure.Depth = EditorGUILayout.IntField("d:", voxelStructure.Depth);
			
			EditorGUILayout.Separator();
			
			voxelStructure.DrawFront = EditorGUILayout.Toggle("Draw Front", voxelStructure.DrawFront);
			voxelStructure.DrawBack  = EditorGUILayout.Toggle("Draw Back", voxelStructure.DrawBack);
			voxelStructure.DrawLeft = EditorGUILayout.Toggle("Draw Left", voxelStructure.DrawLeft);
			voxelStructure.DrawRight = EditorGUILayout.Toggle("Draw Right", voxelStructure.DrawRight);
			voxelStructure.DrawTop = EditorGUILayout.Toggle("Draw Top", voxelStructure.DrawTop);
			voxelStructure.DrawBottom = EditorGUILayout.Toggle("Draw Bottom", voxelStructure.DrawBottom);
			
			EditorGUILayout.Separator();
			//voxelStructure.GenerateSecondaryUvSet = EditorGUILayout.Toggle("Generate Secondary UV Set", voxelStructure.GenerateSecondaryUvSet, GUILayout.Width(60f), GUILayout.Height(60f), GUILayout.ExpandHeight(true));
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Generate Secondary UV Set");
			voxelStructure.GenerateSecondaryUvSet = EditorGUILayout.Toggle(voxelStructure.GenerateSecondaryUvSet);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			
		
		
		
			
			
			//Here we expose various Voxel Structure functions to the Unity Editor
			#region macro command buttons
			EditorGUILayout.BeginHorizontal();
		    if(GUILayout.Button("Solid Cube")){
			 	Undo.RecordObject(voxelStructure, "Solid Cube");
				voxelStructure.SolidCube();
				voxelStructure.Draw();
			}
			if(GUILayout.Button("Room With Back")){
				Undo.RecordObject(voxelStructure, "Room With Back");
				voxelStructure.Room();
				voxelStructure.Draw();
			}
			if(GUILayout.Button("Room Without Back")){
				Undo.RecordObject(voxelStructure, "Room Without Back");
				voxelStructure.RoomNoBack();
				voxelStructure.Draw();
			}
			EditorGUILayout.EndHorizontal();
			
			
			
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Left Wall")){
				Undo.RecordObject(voxelStructure, "Left");
				voxelStructure.LeftWall();
				voxelStructure.Draw();
			}
			if(GUILayout.Button("Right Wall")){
				Undo.RecordObject(voxelStructure, "Right");
				voxelStructure.RightWall();
				voxelStructure.Draw();
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Floor")){
				Undo.RecordObject(voxelStructure, "Floor");
				voxelStructure.Floor();
				voxelStructure.Draw();
			}
			if(GUILayout.Button("Ceiling")){
				Undo.RecordObject(voxelStructure, "Ceiling");
				voxelStructure.Ceiling();
				voxelStructure.Draw();
			}
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Front Wall")){
				Undo.RecordObject(voxelStructure, "Back Wall");
				voxelStructure.FrontWall();
				voxelStructure.Draw();
			}
			if(GUILayout.Button("Back Wall")){
				Undo.RecordObject(voxelStructure, "Back Wall");
				voxelStructure.BackWall();
				voxelStructure.Draw();
			}
			EditorGUILayout.EndHorizontal(); 
			
			EditorGUILayout.Separator();
			if(GUILayout.Button("Set All Non-Empties")){
				Undo.RecordObject(voxelStructure, "Set All Non-Empties");
				voxelStructure.SetAllNonEmpties();
				voxelStructure.Draw();
			}

			if(GUILayout.Button("Pillars Front Eight")){
				Undo.RecordObject(voxelStructure, "Pillars Front Eight");
				voxelStructure.PillarsFrontEight();
				voxelStructure.Draw();
			}
			
			EditorGUILayout.Separator();	
		    if(GUILayout.Button("Draw")){
				voxelStructure.Draw();
			}
		    if(GUILayout.Button("Clear All")){
				Undo.RecordObject(voxelStructure, "Clear All");
				voxelStructure.ClearAll();
				voxelStructure.Draw();
			}
			EditorGUILayout.Separator();	
			EditorGUILayout.Separator();	    
			if(GUILayout.Button("Export Mesh")){
				voxelStructure.ExportMesh();
			}	
		}else{
			EditorGUILayout.LabelField("You must select a Voxel Pallette before you can do anything!");
			Debug.LogWarning("No Pallette selected");
		}
		#endregion



		if (GUI.changed) EditorUtility.SetDirty(target);
		
	}

	void HandleClick (Vector2 mousePosition, VoxelStructure.Action action)
	{
		
		VoxelStructure voxelStructure = (VoxelStructure)target;
		//Strange for the editor, but can sometimes happen)
		if(Camera.current==null){
			Debug.Log("Strange, Camera.current was null. Please try again in a moment.");	
		}else{
			
		
			RaycastHit hit;
			Ray ray = HandleUtility.GUIPointToWorldRay(currentMousePosition);
	
			//If we hit something
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)){
				//get the position of the voxel that was clicked
				var targetVoxelPositionPreScale = (hit.point - (hit.normal*.01f)) - voxelStructure.transform.position;
				var targetVoxelPosition = new Vector3(
					targetVoxelPositionPreScale.x/voxelStructure.transform.localScale.x,
					targetVoxelPositionPreScale.y/voxelStructure.transform.localScale.y,
					targetVoxelPositionPreScale.z/voxelStructure.transform.localScale.z
					);
				//and the neighbor on the side it was clicked on
				var normalNeighborVoxelPositionPreScale = (hit.point + (hit.normal*.01f)) - voxelStructure.transform.position;
				var normalNeighborVoxelPosition = new Vector3(
					normalNeighborVoxelPositionPreScale.x / voxelStructure.transform.localScale.x,
					normalNeighborVoxelPositionPreScale.y / voxelStructure.transform.localScale.y,
					normalNeighborVoxelPositionPreScale.z / voxelStructure.transform.localScale.z
					);
				
				//Use the currently selected action to determine which function to call on the Voxel Structure
				switch(action){
					case VoxelStructure.Action.MaxPull:
						Undo.RecordObject(voxelStructure, "Max Pull");
						voxelStructure.MaxExtrude(targetVoxelPosition, hit.normal);
						voxelStructure.Draw();
						EditorUtility.SetDirty(target);
						break;
					case VoxelStructure.Action.Paint:
					Undo.RecordObject(voxelStructure, "Paint");
						voxelStructure.SetVoxel(targetVoxelPosition, VoxelStructure.SelectedVoxelType);
						voxelStructure.Draw();
						EditorUtility.SetDirty(target);
						break;
					case VoxelStructure.Action.Add:
					Undo.RecordObject(voxelStructure, "Add");
						voxelStructure.SetVoxel(normalNeighborVoxelPosition, VoxelStructure.SelectedVoxelType);
						voxelStructure.Draw();
						EditorUtility.SetDirty(target);
						break;
					case VoxelStructure.Action.Erase: 	
						
					Undo.RecordObject(voxelStructure, "Erase");
						voxelStructure.SetVoxel(targetVoxelPosition, 0);
						voxelStructure.Draw();
						EditorUtility.SetDirty(target);
						
						break;
					case VoxelStructure.Action.Flood:
					Undo.RecordObject(voxelStructure, "Flood");
						voxelStructure.FloodBasic((int)Mathf.Floor(hit.point.y));
						voxelStructure.Draw();
						EditorUtility.SetDirty(target);
						break;
					
					}
				
			
			}
		}
		
		
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
			VoxelStructure voxelStructure = (VoxelStructure)target;
	    	voxelStructure.Draw();
			Repaint ();
	    	return;
	    }

		

		//if shift && mousedown, do that spraypaint thang
		if( e.shift && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) ){
			var currentTime = DateTime.Now;
			var dTime = currentTime.Subtract(lastSprayPaintAction);
			if(dTime.Milliseconds>sprayPaintThrottle){
				lastSprayPaintAction=currentTime;
				if(e.button == 0){
					HandleClick (e.mousePosition, VoxelStructure.LMB_Action);
				}
				else if(e.button == 1){
					HandleClick (e.mousePosition, VoxelStructure.RMB_Action);
				}
				e.Use ();
			}
			
		}

		else if(e.type == EventType.mouseDown ) {
			if(e.button == 0){
		    	wasLeftMouseDown = true;
				HandleClick (currentMousePosition, VoxelStructure.LMB_Action);
			}else if(e.button == 1){
		    	wasRightMouseDown = true;
				HandleClick (currentMousePosition, VoxelStructure.RMB_Action);
			}
		}


		
		
		
		
		
		
		
		
		
		
		//Keyboard shortcuts for tools
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha1){
			EditorPrefs.SetInt("LMBAction", (int)VoxelStructure.Action.Add);
			VoxelStructure.LMB_Action = VoxelStructure.Action.Add;
			Repaint();
			e.Use ();
		}
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha2){
			EditorPrefs.SetInt("LMBAction", (int)VoxelStructure.Action.MaxPull);
			VoxelStructure.LMB_Action = VoxelStructure.Action.MaxPull;	
			Repaint();
			e.Use ();
		}		
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha3){
			EditorPrefs.SetInt("LMBAction", (int)VoxelStructure.Action.Paint);
			VoxelStructure.LMB_Action = VoxelStructure.Action.Paint;	
			Repaint();
			e.Use ();
		}
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha4){
			EditorPrefs.SetInt("LMBAction", (int)VoxelStructure.Action.Erase);
			VoxelStructure.LMB_Action = VoxelStructure.Action.Erase;	
			Repaint();
			e.Use ();
		}
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha5){
			EditorPrefs.SetInt("LMBAction", (int)VoxelStructure.Action.Flood);
			VoxelStructure.LMB_Action = VoxelStructure.Action.Flood;	
			Repaint();
			e.Use ();
		}
	}
	
	
	
	
	
	
	
	
	
	
	

}