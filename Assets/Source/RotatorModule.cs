﻿using UnityEngine;
using System.Collections;

public class RotatorModule : Module {

	public enum RotatorType { Standard, Sprayer, Spinner }

	public RotatorType type;
	public float turnSpeed;
	public float defualtRot;
	public float angleToTarget;
	public float sprayAngle = 60f;
	public int torque;
	private float torqueSpeedMul;

	// Update is called once per frame
	void FixedUpdate () {
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

                break;

            case RotatorType.Sprayer:
			    if (EnemyManager.waveStarted) {
                    float toTarget = Angle.CalculateAngle (transform.position, parentBase.targetPos);
                    angleToTarget = toTarget + Mathf.Sin (Time.time / Mathf.Rad2Deg * GetSpeed ()) *  (360f / sprayAngle);
			    }
                break;
            case RotatorType.Spinner:
                if (EnemyManager.waveStarted)
                    angleToTarget += GetSpeed ();
                break;
		}

        RotateToAngle ();
    }

    public override float GetSpeed () {
		return turnSpeed * ResearchMenu.turnrateMul * upgradeMul * torqueSpeedMul;
	}

	void RotateToAngle () {
		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, angleToTarget), GetSpeed () * Time.deltaTime);
	}

    public void OnToggleModRotation (int index) {
        type = (RotatorType)index;
    }

    public void OnInitializeToggleModRotation (ModuleMod mod) {
        mod.GetReturnedMeta ((int)type);
    }

	public override string ToString () {
		return "Turn speed: " + GetSpeed ().ToString () + " - \nWeight: " + 
			GetChildrenModuleWeight ().ToString () + " / " + torque.ToString () + " - ";
	}
}