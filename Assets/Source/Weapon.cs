using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public Transform[] muzzles;

	public GameObject bullet;
	public Projectile bulletData;
	public float bulletSpeed = 80;
	public float bulletSpread = 5;
	public int bulletDamage = 10;
	public int bulletAmount = 1;
	public float maxRange;
	public Transform target;
	public GameObject fireParticle;

	public float firerate;
	public float reloadTime;
	public float sequenceTime;
		
	private Transform pointer;
	private bool canFire = true;

	public Projectile GetBulletData () {
		if (!bulletData) {
			bulletData = bullet.GetComponent<Projectile> ();
		}
		return bulletData;
	}

	// TODO Implement projectile pooling

	public virtual void Start () {
		pointer = new GameObject ("Pointer").transform;
		pointer.parent = transform;
		pointer.transform.position = transform.position;
	}

	IEnumerator DoFire () {

		Invoke ("ChamberBullet", firerate * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / Game.powerPercentage);
		canFire = false;

		for (int m = 0; m < muzzles.Length; m++) {
			for (int i = 0; i < bulletAmount; i++) {

				GameObject newBullet = (GameObject)Instantiate (bullet, new Vector3 (muzzles[m].position.x, muzzles[m].position.y, 0), muzzles[m].rotation);
				Projectile pro = newBullet.GetComponent<Projectile>();

				pro.velocity = muzzles[m].rotation * new Vector3 (bulletSpeed * Random.Range (0.9f, 1.1f), Random.Range (-bulletSpread, bulletSpread));
				pro.parent = gameObject;
				pro.damage = (int)((float)bulletDamage * ResearchMenu.damageMul[(int)GetBulletData ().effectiveAgainst]);
				pro.range = maxRange * ResearchMenu.rangeMul;
				pro.target = target;

				if (pro.destroyOnTime)
					Destroy (newBullet, maxRange / bulletSpeed * 1.5f);
			
			}

			yield return new WaitForSeconds (sequenceTime);

		}

	}

	public void Fire (Vector3 position) {
		if (canFire) {
			float angle = Angle.CalculateAngle (transform.position, position);
			pointer.eulerAngles = new Vector3 (0,0,angle);
			if (Vector3.Distance (transform.eulerAngles, pointer.eulerAngles) < 1f) {
				StartCoroutine ("DoFire");
			}
		}
	}

	void ChamberBullet () {
		canFire = true;
	}
}
