using UnityEngine;
using System.Collections;

public class BaseModule : Module {

	public Transform target;
	public Vector3 targetPos;

	public LayerMask targetLayer;
	private TurretTargetFinder targetFinder = new TurretTargetFinder ();

	public float range;

	// Update is called once per frame
	public override void UpdateModule () {
		if (!target) {
			FindTarget ();
		}else{
			targetPos = target.position;
			if (Vector3.Distance (transform.position, targetPos) > range)
				target = null;
		}
	}

	void FindTarget () {
		target = targetFinder.FindTarget (this, range, targetLayer);
	}
}
