using UnityEngine;
using System.Collections;

public class WeaponModule : Module {

	public Weapon weapon;
	public float range = 15f;
	public bool useBaseRange = true;
	
	// Update is called once per frame
	new void Start () {
		base.Start ();
		if (parentBase) {
			if (useBaseRange) {
				range = parentBase.range;
				weapon.maxRange = range;
			}else{
				weapon.maxRange = range;
			}

			weapon.bulletDamage = Mathf.RoundToInt ((float)weapon.bulletDamage * parentBase.damageBoost);
			weapon.firerate *= parentBase.damageBoost;
		}else{
			weapon.maxRange = range;
		}
	}

	void Update () {

		if (parentBase) {
			if (parentBase.target && Vector3.Distance (parentBase.transform.position, parentBase.targetPos) < range) {
				weapon.target = parentBase.target;
				weapon.Fire ();
			}
		}else{
			weapon.Fire ();
		}
	}

	public override string ToString () {
		// This is gonna be a big one, hang on..
		string text = "Damage: " + (weapon.bulletDamage * weapon.bulletAmount * ResearchMenu.damageMul[(int)weapon.bulletData.effectiveAgainst]).ToString () + "\n\n - " + 
			"Firerate: " + (weapon.firerate * ResearchMenu.firerateMul[(int)weapon.bulletData.effectiveAgainst]).ToString () + "\n\n - " +
				"Spread: " + weapon.bulletSpread.ToString () + "\n\n - " + 
				"Muzzles: " + weapon.muzzles.Length.ToString () + "\n\n - " +
				"DPS: " + ((weapon.bulletDamage * weapon.bulletAmount * 
				ResearchMenu.damageMul[(int)weapon.bulletData.effectiveAgainst] *
				weapon.muzzles.Length) / weapon.firerate).ToString () + " - ";

		return text;
	}
}
