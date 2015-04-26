using UnityEngine;
using System.Collections;

public class BaseModule : Module {

	[Header ("Targeting")]
	public Transform target;
	public Vector3 targetPos;
	private Vector3 prevPos;
	private Vector3 targetVel;

	public LayerMask targetLayer;
	private TargetFinder targetFinder = new TargetFinder ();

	public static bool enableAdvancedTracking;

	[Header ("Stats")]
	public float range;
	private float fastestBulletSpeed;

	[Header ("Boosts")]
	public float damageBoost = 1f;
	public float firerateBoost = 1f;

	// Update is called once per frame
	void Update () {
		if (!target) {
			FindTarget ();
		}else{

			if (enableAdvancedTracking) {
				targetVel = (target.position - prevPos)/Time.fixedDeltaTime;
				targetPos = target.position + Vector3.Distance (transform.position, target.position)/fastestBulletSpeed * targetVel;
				prevPos = target.position;
				Debug.DrawLine (target.position, targetPos);
			}else{
				targetPos = target.position;
			}

			if (Vector3.Distance (transform.position, targetPos) > range)
				target = null;
		}
	}

	void FindTarget () {
		target = targetFinder.FindTarget (transform.position, range * Game.powerPercentage, targetLayer);
		if (target)
			targetPos = target.position;
	}

	void OnNewModuleAdded () {
		fastestBulletSpeed = GetFastestBulletSpeed ();
	}

	float GetFastestBulletSpeed () {
		Weapon[] weapons = GetComponentsInChildren<Weapon>();
		float speed = 0;
		for (int i = 0; i < weapons.Length; i++) {
			if (weapons[i].bulletSpeed > speed)
				speed = weapons[i].bulletSpeed;
		}

		return speed;
	}
}
