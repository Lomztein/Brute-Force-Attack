using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class TargetFinder {

	public enum SortType { Closest, Furthest, MostHealth, LeastHealth, First, Last, Random};

	public Transform FindTarget (Vector3 position, float range, LayerMask targetLayer, Colour[] priorities, Colour[] ignore, SortType sort) {

		// If anything is nearby, proceed.
		if (Physics.CheckSphere (position, range, targetLayer)) {

			// Get everything nearby on targetLayer and iterate through them,
			// checking distance and priorities of the base module.

			float dis = float.MaxValue;
			Transform ner = null;
			Transform pri = null;

			Collider[] nearby = Physics.OverlapSphere (position, range, targetLayer);
			for (int i = 0; i < nearby.Length; i++) {
					
			// Check distance compared to previous iteration, and if smaller, save current iteration.

				float d = 0f;
				Enemy enemy = nearby[i].GetComponent<Enemy>();

				switch (sort) {

				case SortType.LeastHealth:
					d = (float)enemy.health;
					break;
						
				case SortType.MostHealth:
					d = -(float)enemy.health;
					break;

				case SortType.Last:
					d = -(float)enemy.GetPathDistanceRemaining ();
					break;

				case SortType.First:
					d = (float)enemy.GetPathDistanceRemaining ();
					break;

				case SortType.Closest:
					d = Vector3.Distance (position, nearby[i].transform.position);
					break;

				case SortType.Furthest:
					d = -Vector3.Distance (position, nearby[i].transform.position);
					break;

				case SortType.Random:
					return nearby[Random.Range (0, nearby.Length)].transform;
				
				default:
					d = Vector3.Distance (position, nearby[i].transform.position);
					break;
				}
				
				if (d < dis && !ignore.Contains (enemy.type)) {
					dis = d;
					ner = nearby[i].transform;

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
