using UnityEngine;
using System.Collections;

public class FloorTrigger : MonoBehaviour {



	void OnTriggerEnter(Collider col){
		Debug.Log("drop");
		Application.LoadLevel (0);
	}
}
