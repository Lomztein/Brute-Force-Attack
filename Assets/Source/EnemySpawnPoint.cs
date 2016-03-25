using UnityEngine;
using System.Collections;

public class EnemySpawnPoint : ScriptableObject {
	
	public Vector3 worldPosition;
	public EnemyEndPoint endPoint;
	public Vector2[] path;
    public bool blocked;
	
}