using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseModule : Module {

	[Header ("Targeting")]
	public Transform target;
	public Vector3 targetPos;
	private Vector3 prevPos;
	private Vector3 targetVel;
	public TargetFinder.SortType sortType = TargetFinder.SortType.Closest;

	public LayerMask targetLayer;
	private TargetFinder targetFinder = new TargetFinder ();

	public static bool enableAdvancedTracking;

	[Header ("Stats")]
	public float range;
	private float targetingRange;
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

			if (enableAdvancedTracking && fastestBulletSpeed > 1.1) {
				Vector3 delPos = target.position - prevPos;
				if (delPos.magnitude > 0.1f) {
					targetVel = delPos/Time.fixedDeltaTime;
					targetPos = target.position + Vector3.Distance (transform.position, target.position)/fastestBulletSpeed * targetVel;
					prevPos = target.position;
				}else{
					targetPos = target.position;
				}
			}else{
				targetPos = target.position;
			}

			if (Vector3.Distance (transform.position, targetPos) > GetRange ())
				target = null;
		}
	}

	public override void Start () {
		base.Start ();
		targetingRange = range;
	}

	public float GetRange () {
		return Mathf.Min (range, targetingRange) * upgradeMul * ResearchMenu.rangeMul;
	}

	void FindTarget () {
		target = targetFinder.FindTarget (transform.position, GetRange (), targetLayer, priorities.ToArray (), sortType);
		if (target)
			targetPos = target.position;
	}

	void OnNewModuleAdded () {
		GetFastestBulletSpeed ();
	}

	public void GetFastestBulletSpeed () {
		Weapon[] weapons = GetComponentsInChildren<Weapon>();
		float speed = 0;
		float r = float.MaxValue;
		for (int i = 0; i < weapons.Length; i++) {
			if (weapons[i].transform.parent.GetComponent<WeaponModule>().parentBase == this) {
				if (weapons[i].bulletSpeed > speed)
					speed = weapons[i].bulletSpeed;

				if (!priorities.Contains (weapons[i].GetBulletData ().effectiveAgainst)) {
					priorities.Add (weapons[i].GetBulletData ().effectiveAgainst);
				}

				if (weapons[i].maxRange < r) {
					r = weapons[i].maxRange * weapons[i].upgradeMul * ResearchMenu.rangeMul;
				}

				weapons[i].damageMul = damageBoost;
				weapons[i].firerateMul = (1f/firerateBoost);
			}
		}

		targetingRange = r;
		fastestBulletSpeed = speed;
	}

	public void RequestRange (GameObject rangeIndicator) {
		rangeIndicator.SendMessage ("GetRange", GetRange ());
	}

	public override string ToString () {
		string text = "";
		text += "Range: " + (GetRange ()).ToString () + " - \n\n";
		if (damageBoost != 1f) {
			text += "Damage Multiplier: " + damageBoost.ToString () + " - \n\n";
		}
		if (firerateBoost != 1f) {
			text += "Firerate Mulitplier: " + firerateBoost.ToString ();
		}
		return text;
	}

	public override void SetIsBeingPlaced () {
		base.SetIsBeingPlaced ();
		targetingRange = range;
	}
}
