using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour {

    public static Dictionary<string, int> bulletIndex = new Dictionary<string, int>();
    public static List<Weapon> activeWeapons = new List<Weapon>();

    public string weaponIdentifier;
	public Transform[] muzzles;

	public GameObject[] bullet;
	public Projectile bulletData;
	public float bulletSpeed = 80f;
	public float bulletSpread = 5f;
	public int bulletDamage = 10;
	public int bulletAmount = 1;
	public float maxRange;
	public Transform target;
	public GameObject fireParticle;
    public int locBulletIndex;

	public float upgradeMul = 1f;
    public float damageUpgradeMul = 1f;

	public float damageMul = 1f;
	public float firerateMul = 1f;
	public float spreadMul = 1f;
	public int amountMul = 1;

	public float firerate;
	public float sequenceTime;
		
	public Transform pointer;
    public Transform pool;

	public bool canFire = true;
	private Queue<GameObject> bulletPool = new Queue<GameObject>();

    private const int FIRERATE_LIMIT = 50;

    public static void TechUpWeapon (string identifier) {
        bulletIndex[identifier]++;
        for (int i = 0; i < activeWeapons.Count; i++) {

            if (activeWeapons[i].weaponIdentifier == identifier) {
                activeWeapons[i].locBulletIndex = bulletIndex[identifier];
                activeWeapons[i].bulletData = null;

                foreach (Transform child in activeWeapons[i].pool)
                    Destroy(child.gameObject);

                activeWeapons[i].bulletPool.Clear();
                activeWeapons[i].GetComponentInParent<WeaponModule>().parentBase.GetFastestBulletSpeed();
            }
        }
    }

	public Projectile GetBulletData () {
        if (Game.currentScene == Scene.Play) {
            if (!bulletData) {
                bulletData = bullet[locBulletIndex].GetComponent<Projectile>();
            }
            return bulletData;
        }
        return null;
	}

	public void ReturnBulletToPool (GameObject toPool) {
		bulletPool.Enqueue (toPool);
	}

    void OnDestroy () {
        activeWeapons.Remove(this);
        if (pool)
            Destroy(pool.gameObject);
    }

	GameObject GetPooledBullet (Vector3 position, Quaternion rotation) {
		if (bulletPool.Count > 0) {
			GameObject b = bulletPool.Dequeue ();
			b.transform.position = position;
			b.transform.rotation = rotation;
			b.SetActive (true);
			return b;
		}

		return (GameObject)Instantiate (bullet[locBulletIndex], position, rotation);
	}

	public virtual void Start () {
		pointer = new GameObject ("Pointer").transform;
		pool = new GameObject (weaponIdentifier + "Pool").transform;
        pointer.parent = transform;
		pointer.transform.position = transform.position;
        if (Game.currentScene == Scene.Play) {
            locBulletIndex = bulletIndex[weaponIdentifier];
            activeWeapons.Add(this);
        }
	}

	IEnumerator DoFire () {

		Invoke ("ChamberBullet", GetFirerate (true));
		canFire = false;

		for (int m = 0; m < muzzles.Length; m++) {

			for (int i = 0; i < bulletAmount * amountMul; i++) {

				GameObject newBullet = GetPooledBullet (new Vector3 (muzzles[m].position.x, muzzles[m].position.y, 0), muzzles[m].rotation);
				Projectile pro = newBullet.GetComponent<Projectile>();

                pro.transform.SetParent(pool);
				pro.parentWeapon = this;
				pro.velocity = muzzles[m].rotation * new Vector3 (bulletSpeed * Random.Range (0.9f, 1.1f), Random.Range (-bulletSpread, bulletSpread));
				pro.parent = gameObject;
                pro.damage = GetDamage ();
				pro.range = maxRange * ResearchMenu.rangeMul * upgradeMul;
				pro.target = target;
				pro.Initialize ();
				
				if (pro.destroyOnTime)
					pro.Invoke ("ReturnToPool", maxRange * upgradeMul * ResearchMenu.rangeMul / bulletSpeed * 1.5f);
			
			}

            yield return new WaitForSeconds (GetSequenceTime (true));

		}

	}

    private float GetSequenceTime (bool limit) {
        if (muzzles.Length == 1)
            return 1f;
        if (limit)
            return Mathf.Max (sequenceTime * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / upgradeMul, 1f/FIRERATE_LIMIT);
        return sequenceTime * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / upgradeMul;
    }

    private float GetFirerate (bool limit) {
        if (limit)
            return Mathf.Max (firerate * firerateMul * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / upgradeMul, 1f/FIRERATE_LIMIT);
        return firerate * firerateMul * ResearchMenu.firerateMul[(int)GetBulletData ().effectiveAgainst] / upgradeMul;
    }

    private float GetFirerateLimitDamageMultiplier (float rate) {
        return Mathf.Max (((1f / rate - FIRERATE_LIMIT) / FIRERATE_LIMIT + 1), 1f);
    }

    private int GetDamage () {
        return (int)((float)bulletDamage * damageMul * ResearchMenu.damageMul[(int)GetBulletData ().effectiveAgainst] * damageUpgradeMul * GetFirerateLimitDamageMultiplier (GetFirerate (false)));
    }

    public virtual void Fire (RotatorModule rotator, Vector3 basePos, Vector3 position, string fireFunc = "DoFire") {
		if (canFire) {
			if (!rotator) {
				StartCoroutine (fireFunc);
				return;
			}
			float angle = Angle.CalculateAngle (rotator.transform.position, position);
			pointer.eulerAngles = new Vector3 (0,0,angle);
			if (Vector3.Distance (rotator.transform.eulerAngles, pointer.eulerAngles) < 1f || rotator.type != RotatorModule.RotatorType.Standard) {
				StartCoroutine (fireFunc);
			}
		}
    }

    public virtual float GetDPS () {
        if (Game.currentScene == Scene.Play) {
            return ((bulletDamage * damageMul * bulletAmount * damageUpgradeMul *
                    ResearchMenu.damageMul[(int)GetBulletData().effectiveAgainst] *
                    muzzles.Length) / (firerate / upgradeMul * ResearchMenu.firerateMul[(int)GetBulletData().effectiveAgainst]));
        }
        return 0f;
    }

	void ChamberBullet () {
		canFire = true;
	}
}
