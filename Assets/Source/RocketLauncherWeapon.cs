using UnityEngine;
using System.Collections;

public class RocketLauncherWeapon : Weapon {

	public static bool[] doubleRocketsEnabled = new bool[7];
	public bool enableDoubleRocketResearch;

	// Use this for initialization
	void Start () {
		base.Start ();
		if (doubleRocketsEnabled[(int)bulletData.effectiveAgainst] && enableDoubleRocketResearch) {
			Transform[] newM = new Transform[muzzles.Length * 2];
			for (int i = 0; i < muzzles.Length; i++) {
				newM[i] = muzzles[i];
				newM[i + muzzles.Length] = new GameObject ("Muzzle").transform;
				newM[i + muzzles.Length].transform.position = muzzles[i].transform.position;
				newM[i + muzzles.Length].transform.rotation = muzzles[i].transform.rotation;
				newM[i + muzzles.Length].transform.parent = transform;
			}

			sequenceTime /= 2f;
			muzzles = newM;
		}
	}
}
