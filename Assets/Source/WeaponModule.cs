using UnityEngine;
using System.Collections;

public class WeaponModule : Module {

	public Weapon weapon;
	public float range = 15f;
	public bool useBaseRange = true;
	
	// Update is called once per frame
	new void Start () {
		base.Start ();
		Debug.Log ("Hurr");
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
}
