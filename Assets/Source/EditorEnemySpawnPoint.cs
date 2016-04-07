using UnityEngine;
using System.Collections;

public class EditorEnemySpawnPoint : MonoBehaviour {

    public bool endPlaced;
    public Vector3 endPoint;
    public LineRenderer line;
    public bool allowPlacement = true;

    void AllowPlacement () {
        allowPlacement = true;
    }

	void OnMouseDown () {
        if (allowPlacement && !endPlaced) {
            BattlefieldEditor.cur.spawnpoints.Remove (gameObject);
            Destroy (gameObject);
            return;
        }

        allowPlacement = false;
        endPlaced = false;
        Invoke ("AllowPlacement", 0.2f);
    }

    void Update () {
        if (!endPlaced) {
            Vector3 worldPos = (Vector2)PlayerInput.cur.RoundPos (Camera.main.ScreenToWorldPoint (Input.mousePosition), 1);
            line.SetPosition (0, transform.position + Vector3.back);
            line.SetPosition (1, worldPos + Vector3.back);
            endPoint = worldPos;
            if (Input.GetMouseButtonDown (0) && allowPlacement) {
                endPlaced = true;
            }
        }
    }

    public EnemySpawnPoint Convert () {
        EnemySpawnPoint point = ScriptableObject.CreateInstance<EnemySpawnPoint> ();
        point.worldPosition = transform.position;
        point.endPoint = ScriptableObject.CreateInstance<EnemyEndPoint> ();
        point.endPoint.worldPosition = endPoint;
        return point;
    }
}
