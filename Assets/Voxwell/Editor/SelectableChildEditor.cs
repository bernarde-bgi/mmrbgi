using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(SelectableChild))]
public class SelectableChildEditor : Editor {


	//Here is where we handle any user interactions with the scene view
	void OnSceneGUI()
	{
		Event e = Event.current;

		if(e.type == EventType.mouseDown ) {
			var theChild = (SelectableChild)target;
			Debug.Log(theChild.name);
		}
		if(e.type == EventType.Repaint){

			var theChild = (SelectableChild)target;
			Selection.activeGameObject = theChild.parent.gameObject;
		}
		if(e.type == EventType.Layout){
			var theChild = (SelectableChild)target;
			Selection.activeGameObject = theChild.parent.gameObject;
			e.Use();
		}
//		if(e.type == EventType.mouseUp ) {
//			var theChild = (SelectableChild)target;
//			Selection.activeGameObject = theChild.parent.gameObject;
//			e.Use();
//		}
	}
}
