using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour {
	
	public Transform[] muzzles;

	public GameObject bullet;
	public Projectile bulletData;
	public float bulletSpeed = 80f;
	public float bulletSpread = 5f;
	public int bulletDamage = 10;
	public int bulletAmount = 1;
	public float maxRange;
	public Transform target;
	public GameObject fireParticle;
	public float upgradeMul = 1f;
	public static float bulletSleepTime = 1f;

	public float damageMul = 1f;
	public float firerateMul = 1f;
	public float spreadMul = 1f;
	public int amountMul = 1;

	public float firerate;
	public float reloadTime;
	public float sequenceTime;
		
	public Transform pointer;
	public bool canFire = true;
	private Queue<GameObject> bulletPool = new Queue<GameObject>();

	public Projectile GetBulletData () {
		if (!bulletData) {
			bulletData = bullet.GetComponent<Projectile> ();
		}
		return bulletData;
	}

	public void ReturnBulletToPool (GameObject toPool) {
		bulletPool.Enqueue (toPool);
	}

	GameObject GetPooledBullet (Vector3 position, Quaternion rotation) {
		if (bulletPool.Count > 0) {
			GameObject b = bulletPool.Dequeue ();
			b.transform.position = position;
			b.transform.rotation = rotation;
			b.SetActive (true);
			return b;
		}

		return (GameObject)Instantiate (bullet, position, rotation);
	}

	public virtual void Start () {
		Debug.Log ("wat");
		pointer = new GameObject ("Pointer").transform;
		pointer.parent = transform;
		pointer.transform.position = transform.position;
	}

	IEnumerator DoFire () {

		Invoke ("ChamberBullet", firerate * firerateMul * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / upgradeMul);
		canFire = false;

		for (int m = 0; m < muzzles.Length; m++) {
			for (int i = 0; i < bulletAmount * amountMul; i++) {

				GameObject newBullet = GetPooledBullet (new Vector3 (muzzles[m].position.x, muzzles[m].position.y, 0), muzzles[m].rotation);
				Projectile pro = newBullet.GetComponent<Projectile>();
				
				pro.parentWeapon = this;
				pro.velocity = muzzles[m].rotation * new Vector3 (bulletSpeed * Random.Range (0.9f, 1.1f), Random.Range (-bulletSpread, bulletSpread));
				pro.parent = gameObject;
				pro.damage = (int)((float)bulletDamage * damageMul * ResearchMenu.damageMul[(int)GetBulletData ().effectiveAgainst] * upgradeMul);
				pro.range = maxRange * ResearchMenu.rangeMul * upgradeMul;
				pro.target = target;
				pro.Initialize ();
				
				if (pro.destroyOnTime)
					pro.Invoke ("ReturnToPool", maxRange * upgradeMul * ResearchMenu.rangeMul / bulletSpeed * 1.5f);
			
			}

			yield return new WaitForSeconds (sequenceTime * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / upgradeMul);

		}

	}

	public virtual void Fire (RotatorModule rotator, Vector3 basePos, Vector3 position) {
		if (canFire) {
			if (!rotator) {
				StartCoroutine ("DoFire");
				return;
			}
			float angle = Angle.CalculateAngle (rotator.transform.position, position);
			pointer.eulerAngles = new Vector3 (0,0,angle);
			if (Vector3.Distance (rotator.transform.eulerAngles, pointer.eulerAngles) < 1f) {
				StartCoroutine ("DoFire");
			}
		}
	}

	void ChamberBullet () {
		canFire = true;
	}
}
