using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(SelectableParent))]
public class SelectableParentEditor : Editor {


	//Here is where we handle any user interactions with the scene view
	void OnSceneGUI()
	{
		Event e = Event.current;

		if(e.type == EventType.mouseDown ) {
			Debug.Log("MOUSE DOWN");
		}
		
		if(e.type == EventType.mouseUp ) {
			Debug.Log("MOUSE UP");
		}
		
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha1){
			Debug.Log("LOCKING ALL CHILLINS");
			var theParent = (SelectableParent)target;
			SetLockOnAllChildren(true, theParent);
			Repaint();
			e.Use ();
		}
		if(e.type == EventType.keyUp && e.keyCode == KeyCode.Alpha3){
			Debug.Log("UNLOCKING ALL CHILLINS");
			var theParent = (SelectableParent)target;
			SetLockOnAllChildren(false, theParent);
			Repaint();
			e.Use ();
		}
	}


	public void SetLockOnAllChildren(bool shouldLock, SelectableParent Parent){
		for(var i = 0;i<Parent.transform.childCount;i++){
			var t = Parent.transform.GetChild(i).gameObject;
			if(shouldLock){
				//t.hideFlags ^= HideFlags.NotEditable;
				t.hideFlags ^= HideFlags.HideInHierarchy;
				//t.hideFlags ^= HideFlags.HideInInspector;
			}else{
				t.hideFlags = HideFlags.None;
			}
			
			
		}
		
		
	}

}
