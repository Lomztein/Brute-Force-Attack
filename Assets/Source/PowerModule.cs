using UnityEngine;
using System.Collections;

public class PowerModule : Module {

	public float powerGenerationSpeed;

	public static float CalculateTotalPowerGenerationSpeed () {

		GameObject[] modules = GameObject.FindGameObjectsWithTag ("Module");
		float power = 0f;

		foreach (GameObject m in modules) {

			PowerModule pm = m.GetComponent<PowerModule>();
			if (pm) {

				power += pm.powerGenerationSpeed;

			}
		}

		return power + 5f;
	}
}
