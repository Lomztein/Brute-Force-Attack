using UnityEngine;
using System.Collections;

public class FocusBeamWeapon : Weapon {

	public LineRenderer line;

	public static float chargeSpeedMultiplier = 1f;

	// Use this for initialization
	public override void Start () {
		base.Start ();
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
	}

	void Reload () {
		canFire = true;
	}

	void FixedUpdate () {
		if (weaponModule.parentBase && weaponModule.parentBase.target && !weaponModule.parentBase.target.gameObject.activeSelf) {
			BreakBeam ();
		}else if (weaponModule.parentBase && !weaponModule.parentBase.target) {
            BreakBeam ();
        }
    }

    void UpdateBeam () {
		Ray ray = new Ray (new Vector3 (muzzles[0].position.x, muzzles[0].position.y, 0), muzzles [0].right * weaponModule.parentBase.GetRange ());
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit, weaponModule.parentBase.GetRange (), Game.game.enemyLayer)) {
			line.SetPosition (0, muzzles [0].position);
			line.SetPosition (1, hit.point);
			hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (Mathf.RoundToInt (GetDPS () * Time.fixedDeltaTime), GetBulletData ().effectiveAgainst, this));
		} else {
			line.SetPosition (0, muzzles[0].position);
			line.SetPosition (1, ray.GetPoint (weaponModule.parentBase.GetRange ()));
		}
	}
}
