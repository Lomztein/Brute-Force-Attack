using UnityEngine;
using System.Collections;

public class WeaponModule : Module {

	public Weapon weapon;
	public float range = 15f;
	public bool useBaseRange = true;
	
	// Update is called once per frame
	public override void StartModule () {
		if (parentBase && useBaseRange) {
			range = parentBase.range;
			weapon.maxRange = range;
		}else{
			weapon.maxRange = range;
		}
	}

	public override void UpdateModule () {

		if (parentBase) {
			if (parentBase.target && Vector3.Distance (parentBase.transform.position, parentBase.targetPos) < range) {
				weapon.target = parentBase.target;
				weapon.Fire ();
			}
		}else{
			weapon.Fire ();
		}
	}
}
