using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathDemonstrator : MonoBehaviour {

    public Vector2[] path;
    public int nodesPerSecond;
    public GameObject nodeArrow;

    public static PathDemonstrator cur;
    public static List<PathDemonstrator> demonstrators = new List<PathDemonstrator>();

    public void StartPath () {
        StartCoroutine (Demonstrate ());
    }

    private IEnumerator Demonstrate () {
        cur = this;
        demonstrators.Add (this);

        for (int i = 0; i < path.Length; i++) {
            if (i < path.Length - 2) {
                Quaternion rot = Quaternion.Euler (0f, 0f, Angle.CalculateAngle (path[i], path[i + 1]));
                Destroy ((GameObject)Instantiate (nodeArrow, path[i], rot), 1f);
                yield return new WaitForSeconds (1f / nodesPerSecond);
            } else {
                Destroy ();
                OnEndFinishedDemonstration ();
            }
        }
    }

    public void Destroy () {
        demonstrators.Remove (this);
        Destroy (gameObject);
    }

    public void OnEndFinishedDemonstration () {
        if (EnemyManager.cur.pathDemonstratorButton.sprite == EnemyManager.cur.pathDemonstratorButtonSprites[1]) {
            if (demonstrators.Count == 0) {
                EnemyManager.cur.DemonstratePaths ();
            }
        }
    }
}
