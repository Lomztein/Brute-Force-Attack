using UnityEngine;
using System.Collections;

public class FocusBeamWeapon : Weapon {

	public LineRenderer line;

	public float charge;
	public float maxCharge;

	public float chargeSpeed;
	public float reloadTime;

	public static float chargeSpeedMultiplier = 1f;

	// Use this for initialization
	public override void Start () {
		base.Start ();
		bulletDamage = (int)maxCharge;
		if (muzzles.Length > 1) {
			Debug.LogWarning ("Charging beam weapons currently only supports a single muzzle!");
		}
	}

	public override void Fire (RotatorModule rotator, Vector3 basePos, Vector3 position, string fireFunc = "DoFire") {
		if (canFire) {
			if (!rotator) {
				UpdateBeam ();
				return;
			}
			float angle = Angle.CalculateAngle (rotator.transform.position, position);
			pointer.eulerAngles = new Vector3 (0, 0, angle);
			if (Vector3.Distance (rotator.transform.eulerAngles, pointer.eulerAngles) < 5f) {
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

	float GetChargeSpeed () {
		return chargeSpeed * firerate * Time.fixedDeltaTime * chargeSpeedMultiplier * upgradeMul;
	}

	float GetMaxCharge () {
		return maxCharge * ResearchMenu.damageMul [(int)GetBulletData ().effectiveAgainst] * upgradeMul;
	}

	void FixedUpdate () {
		if (weaponModule.parentBase.target && !weaponModule.parentBase.target.gameObject.activeSelf) {
			BreakBeam ();
		}
	}

    public override float GetDPS () {
        if (Game.currentScene == Scene.Play) {
            return ((maxCharge * damageMul * damageUpgradeMul * ResearchMenu.damageMul[(int)Colour.Red]) * upgradeMul);
        }
        return 0f;
    }

    void UpdateBeam () {
		bulletDamage = (int)maxCharge;
		Ray ray = new Ray (new Vector3 (muzzles[0].position.x, muzzles[0].position.y, 0), muzzles [0].right * maxRange * ResearchMenu.rangeMul * upgradeMul);
		RaycastHit hit;

		if (charge > GetMaxCharge ()) {
			BreakBeam ();
			canFire = false;
		}else{
            charge = maxCharge;
		}
		
		line.SetWidth (Mathf.Clamp01 (charge / GetMaxCharge () / 4f),
		               Mathf.Clamp01 (charge / GetMaxCharge ()) / 4f);

		if (Physics.Raycast (ray, out hit, maxRange * ResearchMenu.rangeMul * upgradeMul, Game.game.enemyLayer)) {
			line.SetPosition (0, muzzles [0].position);
			line.SetPosition (1, hit.point);
			hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (Mathf.RoundToInt (GetDPS () * Time.deltaTime), GetBulletData ().effectiveAgainst, this));
		} else {
			line.SetPosition (0, muzzles[0].position);
			line.SetPosition (1, ray.GetPoint (maxRange));
		}
	}
}
