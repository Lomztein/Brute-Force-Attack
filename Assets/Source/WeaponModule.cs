using UnityEngine;
using System.Collections;

public class WeaponModule : Module {

	public Weapon weapon;
	public float rangeMultiplier = 1f;

	public RotatorModule parentRotator;
	
	// Update is called once per frame
	new void Start () {
		base.Start ();
		if (parentBase) {
			weapon.bulletDamage = Mathf.RoundToInt ((float)weapon.bulletDamage * parentBase.damageBoost);
			weapon.firerate *= parentBase.damageBoost;
			weapon.maxRange = parentBase.range * rangeMultiplier;
		}else{
			weapon.maxRange = 20f * rangeMultiplier;
		}
		FindParentRotator ();
	}

	void Update () {

		if (parentBase) {
			if (parentBase.target && Vector3.Distance (parentBase.transform.position, parentBase.targetPos) < weapon.maxRange) {
				weapon.target = parentBase.target;
				weapon.Fire (parentRotator, parentBase.transform.position, parentBase.targetPos);
			}
		}else{
			weapon.Fire (parentRotator, transform.position, weapon.transform.position + weapon.transform.right);
		}
	}

	void FindParentRotator () {
		if (transform.parent == null) {
			parentRotator = GetComponent<RotatorModule>();
		}else{
			Transform cur = transform;
			while (parentRotator == null && cur.parent) {
				parentRotator = cur.GetComponent<RotatorModule>();
				if (!parentRotator) cur = cur.parent;
			}
			parentRotator = cur.GetComponent<RotatorModule>();
		}
	}

	public override string ToString () {
		// This is gonna be a big one, hang on..
		string text = "Damage: " + (weapon.bulletDamage * weapon.bulletAmount * ResearchMenu.damageMul[(int)weapon.GetBulletData ().effectiveAgainst]).ToString () + "\n\n - " + 
			"Firerate: " + (weapon.firerate * ResearchMenu.firerateMul[(int)weapon.GetBulletData ().effectiveAgainst]).ToString () + "\n\n - " +
				"Spread: " + weapon.bulletSpread.ToString () + "\n\n - " + 
				"Muzzles: " + weapon.muzzles.Length.ToString () + "\n\n - " +
				"DPS: " + ((weapon.bulletDamage * weapon.bulletAmount * 
				ResearchMenu.damageMul[(int)weapon.GetBulletData ().effectiveAgainst] *
				weapon.muzzles.Length) / weapon.firerate).ToString () + " - ";

		return text;
	}
}
