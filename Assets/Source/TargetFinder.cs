using UnityEngine;
using System.Collections;
using System.Linq;

public class TargetFinder {

	public enum SortType { Closest, Furtherst, Base, Random, MostHealth, LeastHealth };

	public Transform FindTarget (Vector3 position, float range, LayerMask targetLayer, Colour[] priorities) {

		// If anything is nearby, proceed.
		if (Physics.CheckSphere (position, range, targetLayer)) {

			// Get everything nearby on targetLayer and iterate through them,
			// checking distance and priorities of the base module.

			float dis = float.MaxValue;
			Transform ner = null;
			Transform pri = null;

			Collider[] nearby = Physics.OverlapSphere (position, range, targetLayer);
			for (int i = 0; i < nearby.Length; i++) {

				// TODO Add multiple types of targeting, such as closest, random, most health, and the sorts.

				// Check distance compared to previous iteration, and if smaller, save current iteration.

				float d = Vector3.Distance (position, nearby[i].transform.position);
				if (d < dis) {
					dis = d;
					ner = nearby[i].transform;

					Enemy enemy = nearby[i].GetComponent<Enemy>();
					if (enemy) {
						if (priorities.Contains (enemy.type)) {
							pri = nearby[i].transform;
						}
					}
				}
			}

			if (!ner)
				Debug.LogWarning ("Was iterated through nearby, but none was found");

			if (pri) {
				return pri;
			}else{
				return ner;
			}
		}

		return null;
	}
}
