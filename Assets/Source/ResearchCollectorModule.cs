using UnityEngine;
using System.Collections;

public class ResearchCollectorModule : Module {

	public LineRenderer line;
	public Transform muzzle;
	private float progress;
	public Transform pointer;
	public float multiplier = 2f;
	public float speed;

    new public int GetAssemblyDPS () {
        return (int)(multiplier * speed);
    }

	void FixedUpdate () {
		if (parentBase.target) {
			float angle = Angle.CalculateAngle (transform.position, parentBase.target.position);
			pointer.eulerAngles = new Vector3 (0,0,angle);
			if (Vector3.Distance (transform.eulerAngles, pointer.eulerAngles) < 5f) {
				if (line.enabled == false)
					line.enabled = true;

				line.SetPosition (0, muzzle.position);
				line.SetPosition (1, parentBase.target.position);
				parentBase.target.GetComponent<ResearchPoint>().Capture (upgradeMul * multiplier, speed * upgradeMul);
			}else{

				if (line.enabled == true)
					line.enabled = false;
			}
		}else{
			if (line.enabled == true)
				line.enabled = false;
		}
	}

    public override bool UpgradeModule () {
        bool passed = base.UpgradeModule ();
        upgradeMul = 1f + ((float)upgradeCount / MAX_UPGRADE_AMOUNT);
        return passed;
    }

    public override float GetEfficiency () {
        return multiplier * upgradeMul;
    }

    public override string ToString () {
		return "Collection Multiplier: " + (multiplier * upgradeMul).ToString () + " - \n\n" +
			"Collection Speed: " + (speed * upgradeMul).ToString () + " - ";
	}
}
