using UnityEngine;
using System.Collections;

public class ChargingBeamWeapon : Weapon {

	public LineRenderer line;

	public float charge;
	public float chargeSpeed;
	public float maxCharge;

	public static float chargeSpeedMultiplier = 1f;

	// Use this for initialization
	public void Start () {
		base.Start ();
		if (muzzles.Length > 1) {
			Debug.LogWarning ("Charging beam weapons currently only supports a single muzzle!");
		}
	}

	public override void Fire (RotatorModule rotator, Vector3 basePos, Vector3 position) {
		if (canFire) {
			if (!rotator) {
				UpdateBeam ();
				return;
			}
			float angle = Angle.CalculateAngle (basePos, position);
			pointer.eulerAngles = new Vector3 (0, 0, angle);
			if (Vector3.Distance (rotator.transform.eulerAngles, pointer.eulerAngles) < 1f) {
				UpdateBeam ();
			} else {
				BreakBeam ();
			}
		} else {
			BreakBeam ();
		}
	}

	void BreakBeam () {
		line.SetPosition (0, Vector3.zero);
		line.SetPosition (1, Vector3.zero);
		charge = 0f;
	}

	void Reload () {
		canFire = true;
	}

	void UpdateBeam () {
		Ray ray = new Ray (new Vector3 (muzzles[0].position.x, muzzles[0].position.y, 0), muzzles [0].right * maxRange * ResearchMenu.rangeMul * upgradeMul);
		RaycastHit hit;

		if (charge > maxCharge * ResearchMenu.damageMul[(int)GetBulletData().effectiveAgainst] * upgradeMul) {
			BreakBeam ();
			canFire = false;
			Invoke ("Reload", firerate * ResearchMenu.firerateMul[(int)GetBulletData().effectiveAgainst] / upgradeMul);
		}else{
			charge += chargeSpeed * Time.fixedDeltaTime * chargeSpeedMultiplier * upgradeMul;
		}
		
		line.SetWidth (charge / maxCharge * ResearchMenu.damageMul[(int)GetBulletData().effectiveAgainst] * upgradeMul,
		               charge / maxCharge * ResearchMenu.damageMul[(int)GetBulletData().effectiveAgainst] * upgradeMul);

		if (Physics.Raycast (ray, out hit, maxRange * ResearchMenu.rangeMul * upgradeMul, Game.game.enemyLayer)) {
			line.SetPosition (0, muzzles [0].position);
			line.SetPosition (1, hit.point);
			hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (Mathf.RoundToInt (charge * upgradeMul), GetBulletData ().effectiveAgainst));
		} else {
			line.SetPosition (0, muzzles[0].position);
			line.SetPosition (1, ray.GetPoint (maxRange));
		}
	}
}
