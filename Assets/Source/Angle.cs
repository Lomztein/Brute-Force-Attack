using UnityEngine; 

public class Angle {
	
	static public float CalculateAngle (Vector3 from, Vector3 to) {
		return Mathf.Atan2(to.y-from.y, to.x-from.x) * 180f / Mathf.PI;
	}
	
	static public float CalculateAngle (Transform from, Transform to) {
		return CalculateAngle (from.position, to.position);
	}
}
