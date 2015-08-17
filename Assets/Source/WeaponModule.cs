using UnityEngine;
using System.Collections;

public class WeaponModule : Module {

	public Weapon weapon;
	public float rangeMultiplier = 1f;

	public RotatorModule parentRotator;
	public static float indieRange = 7.5f;
	
	// Update is called once per frame
	new void Start () {
		FindParentRotator ();
		UpdateWeaponRange ();
		base.Start ();
	}

	public override bool UpgradeModule () {
		bool passed = base.UpgradeModule ();
		weapon.upgradeMul = upgradeMul;
		return passed;
	}

	void UpdateWeaponRange () {
		if (parentBase) {
			weapon.maxRange = parentBase.GetRange () * rangeMultiplier;
		}else{
			weapon.maxRange = indieRange * rangeMultiplier;
		}
	}
		

	void Update () {
		if (!IngameEditors.AssemblyEditorScene.isActive) {
			weapon.maxRange = parentBase.GetRange () * rangeMultiplier;
			if (parentBase.target) {
				weapon.target = parentBase.target;
				weapon.Fire (parentRotator, parentBase.transform.position, parentBase.targetPos);
			}
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

	public void RequestRange (GameObject rangeIndicator) {
		rangeIndicator.SendMessage ("GetRange", rangeMultiplier);
	}

	public override string ToString () {
		// This is gonna be a big one, hang on..
		string text = "Damage: " + (weapon.bulletDamage * weapon.damageMul * (float)(weapon.bulletAmount * weapon.amountMul) * ResearchMenu.damageMul[(int)weapon.GetBulletData ().effectiveAgainst] * upgradeMul).ToString () + "\n\n - " + 
			"Firerate: " + (1f/(weapon.firerate * weapon.firerateMul * ResearchMenu.firerateMul[(int)weapon.GetBulletData ().effectiveAgainst] / upgradeMul)).ToString () + "\n\n - " +
				"Spread: " + (weapon.bulletSpread * weapon.spreadMul).ToString () + "\n\n - " + 
				"Muzzles: " + weapon.muzzles.Length.ToString () + "\n\n - " +
				"DPS: " + ((weapon.bulletDamage * weapon.damageMul * weapon.bulletAmount * upgradeMul *
				ResearchMenu.damageMul[(int)weapon.GetBulletData ().effectiveAgainst] *
				weapon.muzzles.Length) / (weapon.firerate / upgradeMul * ResearchMenu.firerateMul[(int)weapon.GetBulletData().effectiveAgainst])).ToString () + " - ";

		return text;
	}
}
