using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour {

	public bool simple = true;
	public Map map;
	public PathManager pathManager;

	public int index;

	public static Pathfinding finder;

	void Awake() {
		finder = this;
	}

	public void StartFindingPath(Vector3 start, Vector3 end) {
		start = new Vector2 (start.x, start.y);
		end = new Vector2 (end.x, end.y);
		StartCoroutine(FindPath(start, end));
	}

	public Vector2 WorldToNode (Vector3 pos) {
		return new Vector2 (pos.x + map.gridX/2, pos.y + map.gridY/2);
	}
	
	Vector3 NodeToWorld (Vector2 node) {
		return new Vector3 (-map.gridX/2f + node.x, -map.gridY/2f + node.y);
	}

	public static void BakePaths () {
		for (int i = 0; i < Game.game.enemySpawnPoints.Count; i++) {
            Game.game.enemySpawnPoints[i].path = null;
			PathManager.RequestPath (Game.game.enemySpawnPoints[i].worldPosition, Game.game.enemySpawnPoints[i].endPoint.worldPosition, finder.OnFinished);
		}
		finder.Invoke ("ResetIndex", EnemyManager.readyWaitTime);
	}

	void ResetIndex () {
		index = 0;
	}

	public void OnFinished(Vector2[] _path, bool success) {
		if (success) {
			Game.game.enemySpawnPoints[index].path = _path;
			index++;
		}
	}

	public static void ChangeArea (Rect rect, bool clear) {
		
		int startX = Mathf.RoundToInt (rect.x);
		int startY = Mathf.RoundToInt (rect.y);
		int w      = Mathf.RoundToInt (rect.width - 1);
		int h	   = Mathf.RoundToInt (rect.height - 1);
		
		for (int y = startY; y < startY + h + 1; y++) {
			for (int x = startX; x < startX + w + 1; x++) {
				Vector2 pos = finder.WorldToNode (new Vector3 (x,y));

                int xx = Mathf.RoundToInt (pos.x);
				int yy = Mathf.RoundToInt (pos.y);

                if (finder.IsInsideField (xx,yy)) {
					
					finder.map.nodes[xx,yy].isWalkable = clear;
					if (Game.isWalled[xx,yy] != Game.WallType.None)
						finder.map.nodes[xx,yy].isWalkable = false;
					
				}
			}
		}
	}

	public bool IsInsideField (int x, int y) {
		if (x < 0 || x > map.gridX-1) return false;
		if (y < 0 || y > map.gridY-1) return false;
		return true;
	}

	IEnumerator FindPath(Vector2 startPos, Vector2 targetPos)
	{
		Node startNode =  map.WorldPointToGridPoint(startPos);
		Node targetNode = map.WorldPointToGridPoint(targetPos);

		Vector2[] waypoints = new Vector2[0];
		bool success = false;

		if (!startNode.isWalkable && !targetNode.isWalkable)
			yield break;

		Heap<Node> openSet = new Heap<Node>(map.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		while (openSet.Count > 0)
		{
			Node currentNode = openSet.RemoveFirst();

			closedSet.Add(currentNode);

			if (currentNode == targetNode)
			{
				success = true;
				break;
			}

			foreach (Node neighbour in map.GetNeighbours(currentNode))
			{
				if (!neighbour.isWalkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				int MovemntCostToneighbour = currentNode.gCost + DistanceBetween(currentNode, neighbour);
				if (MovemntCostToneighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = MovemntCostToneighbour;
					neighbour.hCost = DistanceBetween(neighbour, targetNode);
					neighbour.parent = currentNode;
					if (!openSet.Contains(neighbour))
					{
						openSet.Add(neighbour);
					}
					else
					{
						openSet.UpdateItem(neighbour);
					}

				}
			}
		}
		yield return null;
		if (success)
		{
			waypoints = GetPath(startNode, targetNode);
		}
		pathManager.FinishedProcessingPath(waypoints, success);

	}

	Vector2[] GetPath (Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node trackNode = endNode;
		
		while (trackNode != startNode)
		{
			path.Add(trackNode);
			trackNode = trackNode.parent;
		}
		Vector2[] newPath;
		if (simple)
		{
			newPath = simplePath(path);
		}
		else
		{
			newPath = convertToVector2(path);
		}
		Array.Reverse(newPath);
		return newPath;

	}

	Vector2[] simplePath(List<Node> path)
	{
		List<Vector2> waypoints = new List<Vector2>();
		Vector2 dirOld = Vector2.zero;
		waypoints.Add(path[0].worldPos);
		for (int a = 1; a < path.Count; a++)
		{
			Vector2 dirNew = new Vector2 (path[a].nodeX - path[a-1].nodeX, path[a].nodeY - path[a-1].nodeY);
			if (dirNew != dirOld)
			{
				waypoints.Add(path[a].worldPos);
			}
			dirOld = dirNew;
		}
		return waypoints.ToArray();
	}

	Vector2[] convertToVector2(List<Node> path)
	{
		List<Vector2> waypoints = new List<Vector2>();
		foreach (Node n in path)
		{
			waypoints.Add(n.worldPos);
		}
		return waypoints.ToArray();
	}

	int DistanceBetween(Node nodeA, Node nodeB)
	{
		int distanceX = Mathf.Abs (nodeA.nodeX - nodeB.nodeX);
		int distanceY = Mathf.Abs (nodeA.nodeY - nodeB.nodeY);

		if (distanceX > distanceY)
			return 14 * distanceY + 10 * (distanceX - distanceY);
		return 14 * distanceX + 10 * (distanceY - distanceX);
		// Test out abs

	}

	[System.Serializable]
	public class DPath {
		
		public Vector2[] nodes;
		
		public DPath (Vector2[] nodes) {
			this.nodes = nodes;
		}
	}
}
