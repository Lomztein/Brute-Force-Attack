using UnityEngine;
using System.Collections;

public class ResearchCollectorModule : Module {

	public LineRenderer line;
	public Transform muzzle;
	private float progress;
	public Transform pointer;
	public float multiplier = 2f;
	public float speed;

    public ParticleSystem[] numberWang;

    new public int GetAssemblyDPS () {
        return (int)(multiplier * speed);
    }

	void FixedUpdate () {
		if (parentBase.target) {
			float angle = Angle.CalculateAngle (transform.position, parentBase.target.position);
			pointer.eulerAngles = new Vector3 (0,0,angle);
            UpdateNumberwangPosition ();
			if (Vector3.Distance (transform.eulerAngles, pointer.eulerAngles) < 5f) {
                if (line.enabled == false) {
                    line.enabled = true;
                    EnableNumberwang ();
                }

				line.SetPosition (0, muzzle.position);
				line.SetPosition (1, parentBase.target.position);
				parentBase.target.GetComponent<ResearchPoint>().Capture (upgradeMul * multiplier, speed * upgradeMul);
			}else{

                if (line.enabled == true) {
                    line.enabled = false;
                    DisableNumberwang ();
                }
            }
		}else{
			if (line.enabled == true) {
				line.enabled = false;
                DisableNumberwang ();
            }
		}
	}
    
    public void UpdateNumberwangPosition () {
        foreach (ParticleSystem system in numberWang) {
            system.transform.position = parentBase.target.position;
            system.transform.LookAt (parentBase.transform);
            system.startSpeed = Vector3.Distance (parentBase.transform.position, parentBase.target.transform.position);
        }
    }

    public void EnableNumberwang () {
        foreach (ParticleSystem system in numberWang) {
            system.gameObject.SetActive (true);
        }
    }

    public void DisableNumberwang () {
        foreach (ParticleSystem system in numberWang) {
            system.gameObject.SetActive (false);
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
