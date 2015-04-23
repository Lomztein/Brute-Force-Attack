using UnityEngine;
using System.Collections;

public class ResearchCollectorModule : Module {

	public LineRenderer line;
	public Transform muzzle;
	private float progress;
	public Transform pointer;

	void FixedUpdate () {
		if (parentBase.target) {
			float angle = Angle.CalculateAngle (transform.position, parentBase.target.position);
			pointer.eulerAngles = new Vector3 (0,0,angle);
			if (Vector3.Distance (transform.eulerAngles, pointer.eulerAngles) < 5f) {
				if (line.enabled == false)
					line.enabled = true;

				line.SetPosition (0, muzzle.position);
				line.SetPosition (1, parentBase.target.position);

				progress += Time.fixedDeltaTime;
				if (progress > 1f) {
					Destroy (parentBase.target.gameObject);
					progress = 0;
					Game.research++;
				}
			}else{

				if (line.enabled == true)
					line.enabled = false;
			}
		}else{
			if (line.enabled == true)
				line.enabled = false;
		}
	}
}
