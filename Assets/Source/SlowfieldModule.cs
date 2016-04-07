using UnityEngine;
using System.Collections;

public class SlowfieldModule : Module {

	public Transform rotator;
	public float rotateSpeed;
	public float range;

	public GameObject slowfieldArea;
	private SlowfieldArea slowfield;

	void FixedUpdate () {
		rotator.Rotate (Vector3.forward * rotateSpeed * Time.deltaTime * upgradeMul);
		if (!slowfield) {
			GameObject slow = (GameObject)Instantiate (slowfieldArea, transform.position, Quaternion.identity);
			slowfield = slow.GetComponent<SlowfieldArea> ();
			slowfield.slowAmount = 0.5f;
			slowfield.time = 0f;
			slow.transform.parent = transform;
		} else {
			slowfield.range = GetRange ();
		}
	}

    public override bool UpgradeModule () {
        bool passed = base.UpgradeModule ();
        upgradeMul = 1f + ((float)upgradeCount / MAX_UPGRADE_AMOUNT);
        return passed;
    }

    public override float GetEfficiency () {
        if (slowfield)
            return slowfield.GetSlowness ();
        return SlowfieldArea.staticMultiplier;
    }

    void OnDrawGizmos () {
		Gizmos.DrawWireSphere (transform.position, GetRange ());
	}

    public void RequestRange ( GameObject rangeIndicator ) {
        rangeIndicator.SendMessage ("GetRange", GetRange ());
    }

    public override float GetRange () {
        return range * upgradeMul;
    }
}
