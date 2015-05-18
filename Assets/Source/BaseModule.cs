using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	public List<Colour> priorities;

	[Header ("Boosts")]
	public float damageBoost = 1f;
	public float firerateBoost = 1f;

	// Update is called once per frame
	void Update () {
		if (!target) {
			FindTarget ();
		}else{

			if (enableAdvancedTracking) {
				Vector3 delPos = target.position - prevPos;
				if (delPos.magnitude > 0.1f) {
					targetVel = delPos/Time.fixedDeltaTime;
					targetPos = target.position + Vector3.Distance (transform.position, target.position)/fastestBulletSpeed * targetVel;
					prevPos = target.position;
					Debug.DrawLine (target.position, targetPos);
				}else{
					targetPos = target.position;
				}
			}else{
				targetPos = target.position;
			}

			if (Vector3.Distance (transform.position, targetPos) > range)
				target = null;
		}
	}

	void FindTarget () {
		target = targetFinder.FindTarget (transform.position, range * Game.powerPercentage, targetLayer, priorities.ToArray ());
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
			if (weapons[i].transform.parent.GetComponent<WeaponModule>().parentBase == this) {
				if (weapons[i].bulletSpeed > speed)
					speed = weapons[i].bulletSpeed;

				if (!priorities.Contains (weapons[i].GetBulletData ().effectiveAgainst)) {
					priorities.Add (weapons[i].GetBulletData ().effectiveAgainst);
				}
			}
		}

		return speed;
	}

	public override string ToString () {
		string text = "";
		text += "Range: " + range.ToString () + " - \n\n";
		if (damageBoost != 1f) {
			text += "Damage Multiplier: " + damageBoost.ToString () + " - \n\n";
		}
		if (firerateBoost != 1f) {
			text += "Firerate Mulitplier: " + firerateBoost.ToString ();
		}
		return text;
	}
}
