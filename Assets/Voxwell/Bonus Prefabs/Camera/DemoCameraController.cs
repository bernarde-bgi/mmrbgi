using UnityEngine;
using System.Collections;

public class DemoCameraController : MonoBehaviour {
	
	public enum CameraType{
		xy,
		xz
		
	}
	
	public CameraType cameraType = CameraType.xy;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(cameraType == CameraType.xz){
			transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Time.deltaTime * 10, Space.World);
		}else if(cameraType == CameraType.xy){
			transform.Translate(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * Time.deltaTime * 10, Space.World);
			
		}
	}
}
