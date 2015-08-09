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
		if (EnemySpawn.waveStarted) {
			angleToTarget = transform.eulerAngles.z;
		}else{
			angleToTarget = defualtRot;
		}
		if (type == RotatorType.Standard) {

			if (parentBase)
				if (parentBase.target)
					angleToTarget = Angle.CalculateAngle (transform.position, parentBase.targetPos);

			RotateToAngle ();
		}else if (type == RotatorType.Sprayer) {
			if (EnemySpawn.waveStarted) {
				angleToTarget = defualtRot + Mathf.Sin (Time.time * (360 / turnSpeed)) * sprayAngle;
				RotateToAngle ();
			}
		}else if (type == RotatorType.Spinner && EnemySpawn.waveStarted) {
			transform.Rotate (0,0,turnSpeed * ResearchMenu.turnrateMul * Time.deltaTime);
		}
	}

	float GetSpeed () {
		return turnSpeed * ResearchMenu.turnrateMul * upgradeMul * torqueSpeedMul;
	}

	void RotateToAngle () {
		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, angleToTarget), GetSpeed () * Time.deltaTime);
	}

	public override string ToString () {
		return "Turn speed: " + GetSpeed ().ToString () + " - \nWeight: " + 
			GetChildrenModuleWeight ().ToString () + " / " + torque.ToString () + " - ";
	}
}