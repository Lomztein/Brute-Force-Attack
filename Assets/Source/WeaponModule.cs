using UnityEngine;
using System.Collections;

public class WeaponModule : Module {

	public Weapon weapon;

	// Update is called once per frame
	public override void StartModule () {
		if (parentBase) {
			weapon.bulletRange = parentBase.range;
		}else{
			weapon.bulletRange = 15f;
		}
	}

	public override void UpdateModule () {

		if (parentBase) {
			if (parentBase.target && Vector3.Distance (parentBase.transform.position, parentBase.targetPos) < parentBase.range) {
				weapon.target = parentBase.target;
				weapon.Fire ();
			}
		}else{
			weapon.Fire ();
		}
	}
}
