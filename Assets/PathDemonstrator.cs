using UnityEngine;
using System.Collections;

public class PathDemonstrator : MonoBehaviour {

    public Vector2[] path;
    public int nodesPerSecond;
    public GameObject nodeArrow;

    public void StartPath () {
        StartCoroutine (Demonstrate ());
    }

    private IEnumerator Demonstrate () {
        for (int i = 0; i < path.Length; i++) {
            if (i < path.Length - 2) {
                Quaternion rot = Quaternion.Euler (0f, 0f, Angle.CalculateAngle (path[i], path[i + 1]));
                Destroy ((GameObject)Instantiate (nodeArrow, path[i], rot), 1f);
                yield return new WaitForSeconds (1f / nodesPerSecond);
            } else {
                Destroy (gameObject);
            }
        }
    }
}
