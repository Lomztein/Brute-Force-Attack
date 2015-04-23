using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Dijkstra : MonoBehaviour {

	public Node[,] nodes;
	public Vector2[] near;
	public float[] extraCost;

	public bool debugDrawNodes;
	public bool debugDrawPaths;
	public bool debugDynamicUpdate;

	public DPath[] paths;

	private int width;
	private int height;

	private List<Node> curPath;
	public static Dijkstra cur;

	public int bakeSpeed = 10;
	public EnemySpawn enemySpawn;

	public void InitializeDijkstraField (int w, int h) {
		cur = this;

		width = w;
		height = h;

		nodes = new Node[width,height];

		for (int y = 1; y < height; y++) {
			for (int x = 1; x < width; x++) {
				nodes[x,y] = new Node (x,y);
				nodes[x,y].isClear = true;
			}
		}

		for (int y = 1; y < height; y++)
			for (int x = 1; x < width; x++) {
				nodes[x,y].nearby = new Node[near.Length];
				for (int i = 0; i < near.Length; i++)
				if (IsInsideField (x + (int)near[i].x, y + (int)near[i].y))
					nodes[x,y].nearby[i] = nodes[x + (int)near[i].x, y + (int)near[i].y];
			}
	}

	public bool IsInsideField (int x, int y) {
		if (x < 1 || x > width-1) return false;
		if (y < 1 || y > height-1) return false;
		return true;
	}

	public Vector2 WorldToNode (Vector3 pos) {
		return new Vector2 (pos.x + width/2, pos.y + height/2);
	}

	Vector3 NodeToWorld (Node node) {
		return new Vector3 (-width/2f + node.x, -height/2f + node.y);
	}

	public static void BakePaths () {
		cur.StopAllCoroutines ();
		cur.StartCoroutine ("UpdatePathsToDatastream");
	}

	public IEnumerator UpdatePathsToDatastream () {

		paths = new DPath[width];
		for (int x = 1; x < width; x++) {
			enemySpawn.waveCounterIndicator.text = "Initializing: " + ((int)((float)x/(float)width*100f)).ToString () + "%";
			curPath = null;

			Dictionary<Node, float> dist = new Dictionary<Node, float>();
			Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

			List<Node> unvisited = new List<Node>();

			Node source = nodes[x, height-1];
			Node target = nodes[x, 1];

			dist[source] = 0f;
			prev[source] = null;

			foreach (Node v in nodes) {
				if (v != source && v != null) {
					dist[v] = Mathf.Infinity;
					prev[v] = null;
				}

				unvisited.Add (v);
			}

			int bakeIndex = 0;
			while (unvisited.Count != 0) {

				Node u = null;

				foreach (Node poss in unvisited) {
					if (poss != null) {
						if (u == null || dist[poss] < dist[u]) {
							u = poss;
						}
					}
				}

				if (u == target)
					break;

				unvisited.Remove (u);

				int index = 0;
				foreach (Node v in u.nearby) {

					float alt = dist[u] + u.DistanceTo (v) + extraCost[index];
					index++;

					if (v != null) {

						if (debugDrawPaths)
							Debug.DrawLine (NodeToWorld (u), NodeToWorld (v), Color.blue, 0.1f, false);

						if (alt < dist[v]) {
							dist[v] = alt;
							prev[v] = u;
						}
					}
				}

				bakeIndex++;
				if (bakeIndex % bakeSpeed == 0) {
					yield return new WaitForEndOfFrame ();
				}

			}

			if (prev[target] == null) {
				StopCoroutine ("UpdatePathsToDatastream");
			}

			curPath = new List<Node>();
			Node curr = target;

			while (prev[curr] != null) {
				curPath.Add (curr);
				curr = prev[curr];
			}

			curPath.Reverse ();

			List<Vector3> newPath = new List<Vector3>();
			foreach (Node n in curPath) {
				newPath.Add (NodeToWorld (n));
			}

			paths[x] = new DPath (newPath.ToArray ());
		}

		enemySpawn.StartWave ();
	}

	public static Vector3[] GetPath (Vector3 start) {
		int x = (int)cur.WorldToNode (start).x;
		x = Mathf.Clamp (x, 1, cur.paths.Length-1);
		return cur.paths[x].nodes;
	}

	public static void ChangeArea (Rect rect, bool clear) {

		int startX = Mathf.RoundToInt (rect.x);
		int startY = Mathf.RoundToInt (rect.y);
		int w      = Mathf.RoundToInt (rect.width);
		int h	   = Mathf.RoundToInt (rect.height);

		for (int y = startY; y < startY + h + 1; y++) {
			for (int x = startX; x < startX + w + 1; x++) {
				Vector2 pos = cur.WorldToNode (new Vector3 (x,y));

				int xx = Mathf.RoundToInt (pos.x);
				int yy = Mathf.RoundToInt (pos.y);

				if (cur.IsInsideField (xx,yy)) {

					cur.nodes[xx,yy].isClear = clear;
					if (Game.isWalled[xx,yy])
						cur.nodes[xx,yy].isClear = false;
					
				}
			}
		}

		if (cur.debugDynamicUpdate)
			Dijkstra.BakePaths ();
	}

	public class Node {

		public int x;
		public int y;
		public bool isClear;
		public Node[] nearby;

		public Node (int x, int y) {
			this.x = x;
			this.y = y;
		}

		public float DistanceTo (Node node) {
			if (node != null) {
				if (node.isClear) {
					return Vector2.Distance (new Vector2 (x,y), new Vector2 (node.x, node.y));
				}else{
					return 10000000f;
				}
			}else{
				return Mathf.Infinity;
			}
		}

	}

	[System.Serializable]
	public class DPath {

		public Vector3[] nodes;

		public DPath (Vector3[] nodes) {
			this.nodes = nodes;
		}
	}

	void Update () {

		if (debugDrawNodes)
			for (int y = 1; y < height; y++)
				for (int x = 1; x < width; x++) {
					Node node = nodes[x,y];
					for (int i = 0; i < node.nearby.Length; i++)
					if (node.nearby[i] != null) {
					if (node.isClear && node.nearby[i].isClear) {
						Vector3 v = new Vector3 (-width/2f, -height/2f);
						Debug.DrawLine (v + new Vector3 (x,y),v + new Vector3 (node.nearby[i].x, node.nearby[i].y));
					}
				}
			}

		if (debugDrawPaths) {
			for (int i = 1; i < paths.Length; i++) {
				if (paths[i] != null) {
					for (int j = 0; j < paths[i].nodes.Length-1; j++) {
						Debug.DrawLine (paths[i].nodes[j], paths[i].nodes[j + 1], Color.red);
					}
				}
			}
		}
	}
}
