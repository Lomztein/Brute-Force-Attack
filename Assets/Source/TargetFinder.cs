using UnityEngine;
using System.Collections;

public class TargetFinder {

	public Transform FindTarget (Vector3 position, float range, LayerMask targetLayer) {

		// If anything is nearby, proceed.
		if (Physics.CheckSphere (position, range, targetLayer)) {

			// Get everything nearby on targetLayer and iterate through them,
			// checking distance and priorities of the base module.

			float dis = float.MaxValue;
			Transform ner = null;

			Collider[] nearby = Physics.OverlapSphere (position, range, targetLayer);
			for (int i = 0; i < nearby.Length; i++) {

				// TODO Add priority system.
				// TODO Add multiple types of targeting, such as closest, random, most health, and the sorts.

				// Check distance compared to previous iteration, and if smaller, save current iteration.

				float d = Vector3.Distance (position, nearby[i].transform.position);
				if (d < dis) {
					dis = d;
					ner = nearby[i].transform;
				}
			}

			if (!ner)
				Debug.LogWarning ("Was iterated through nearby, but none was found");

			return ner;

		}

		return null;
	}
}
