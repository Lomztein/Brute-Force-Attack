using UnityEngine; 

public class Angle {
	
	static public float CalculateAngle (Vector3 from, Vector3 to) {
		return Mathf.Atan2(to.y-from.y, to.x-from.x)*180 / Mathf.PI;
	}
	
	static public float CalculateAngle (Transform from, Transform to) {
		return Mathf.Atan2(to.position.y-from.position.y, to.position.x-from.position.x)*180 / Mathf.PI;
	}
}
