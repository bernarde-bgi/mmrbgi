// C# example:
using UnityEngine;
using UnityEditor;

//The main editor window. Right now, just handles redrawing groups of voxelstructures
//but more to come soon :D
public class VoxwellEditorWindow : EditorWindow {
	
	
	// Add menu named "My Window" to the Window menu
	[MenuItem ("Window/Voxwell")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		EditorWindow.GetWindow (typeof (VoxwellEditorWindow));
	}
	
	void OnGUI () {
		var centeredStyle = new GUIStyle(GUI.skin.GetStyle("label"));
		centeredStyle.alignment = TextAnchor.MiddleCenter;
		centeredStyle.fontSize = 14;
		centeredStyle.fontStyle = FontStyle.Bold;
		
		GUILayout.Label("Utility Methods", centeredStyle);
		if(GUILayout.Button("Redraw Selected\nStructures")){
			Undo.RecordObjects(Selection.gameObjects, "Redraw Selected \nStructures");
			RedrawSelectedStructures();
		}
		
	}
	
	
	static void RedrawSelectedStructures(){
		
		var selected = Selection.gameObjects;
		for(var i = 0;i<selected.Length;i++){
			var vs = selected[i].GetComponent<VoxelStructure>();
			if(vs){
				vs.Draw();		
			}
		}
	}
	
	

	

}