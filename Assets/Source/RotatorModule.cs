using UnityEngine;
using System.Collections;

public class RotatorModule : Module {

	public enum RotatorType { Standard, Sprayer, Spinner }

	public RotatorType type;
	public float turnSpeed;
	public float defualtRot;
	private float angleToTarget;
	private float sprayAngle = 30f;
	public int torque;
	private float torqueSpeedMul;

	// Update is called once per frame
	void Update () {
		Turn ();
	}

	new void Start () {
		base.Start ();
		defualtRot = transform.eulerAngles.z;
	}

	int GetChildrenModuleWeight () {
		Module[] modules = GetComponentsInChildren<Module> ();
		int curWeight = 0;
		for (int i = 0; i < modules.Length; i++) {
			curWeight += modules [i].moduleClass;
		}
		return curWeight;
	}

	void OnNewModuleAdded () {
		int weight = GetChildrenModuleWeight ();
		if (weight < torque) {
			torqueSpeedMul = 1f;
		} else {
			torqueSpeedMul = Mathf.Clamp ((1-(weight - torque) / torque), 0.1f, 1f);
			// This may or may not be a clusterfuck of stuff. I don't math good at the moment.
		}
	}

	void Turn () {
		if (EnemyManager.waveStarted) {
			angleToTarget = transform.eulerAngles.z;
		}else{
			angleToTarget = defualtRot;
		}
        switch (type) {
            case RotatorType.Standard:

			    if (parentBase)
				    if (parentBase.target)
					    angleToTarget = Angle.CalculateAngle (transform.position, parentBase.targetPos);

			    RotateToAngle ();
                break;

            case RotatorType.Sprayer:
			    if (EnemyManager.waveStarted) {
				    angleToTarget = defualtRot + Mathf.Sin (Time.time * (360 / GetSpeed ())) * sprayAngle;
				    RotateToAngle ();
			    }
                break;
            case RotatorType.Spinner:
                if (EnemyManager.waveStarted)
                    angleToTarget += GetSpeed ();
                break;
		}
	}

	public float GetSpeed () {
		return turnSpeed * ResearchMenu.turnrateMul * upgradeMul * torqueSpeedMul;
	}

	void RotateToAngle () {
		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, angleToTarget), GetSpeed () * Time.deltaTime);
	}

    public void OnToggleModType (int index) {
        type = (RotatorType)index;
    }

	public override string ToString () {
		return "Turn speed: " + GetSpeed ().ToString () + " - \nWeight: " + 
			GetChildrenModuleWeight ().ToString () + " / " + torque.ToString () + " - ";
	}
}