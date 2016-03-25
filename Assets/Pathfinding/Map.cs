using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Map : MonoBehaviour {

	public bool DrawGizmos;
	public LayerMask noneWalkable;
	public float gridRadius;
	public float[] extraCost;

	public Color gridColor = Color.white;

	public Node[,] nodes;

	public float cubeHeight;
	public float cubeWidth;
	public int gridX, gridY;
	private float gridDiameter;

	public void Initialize () {
		gridDiameter = gridRadius * 2;

		Game g = Game.game;
		cubeWidth = g.battlefieldWidth;
		cubeHeight = g.battlefieldHeight;
		gridX = g.battlefieldWidth;
		gridY = g.battlefieldHeight;

		MakeGrid();
	}

	public int MaxSize
	{
		get {
			return gridX * gridY;
		}
	}


	void MakeGrid()
	{
		nodes = new Node[gridX,gridY];
		for (int x = 0; x < gridX; x++) {
			for (int y = 0; y < gridY; y++) {
				nodes[x,y] = new Node(true,new Vector2 (x - cubeWidth / 2f + gridRadius, y - cubeHeight / 2f + gridRadius), x, y);
			}
		}

	}

	public Node WorldPointToGridPoint(Vector2 point)
	{
		float procentX = (Mathf.Clamp01((point.x + cubeWidth/2) / cubeWidth));
		float procentY = (Mathf.Clamp01((point.y + cubeHeight/2) / cubeHeight));
		int x = Mathf.RoundToInt((gridX - 1) * procentX);
		int y = Mathf.RoundToInt((gridY - 1) * procentY);
		return nodes[x, y];
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

        int x = node.nodeX;
        int y = node.nodeY;

        if (Pathfinding.finder.IsInsideField (x + 1, y))
            neighbours.Add(nodes[x + 1, y]);

        if (Pathfinding.finder.IsInsideField(x, y + 1))
            neighbours.Add(nodes[x, y + 1]);

        if (Pathfinding.finder.IsInsideField(x - 1, y))
            neighbours.Add(nodes[x - 1, y]);

        if (Pathfinding.finder.IsInsideField(x, y - 1))
            neighbours.Add(nodes[x, y - 1]);

        /*for (int x = node.nodeX-1; x <= node.nodeX+1; x++)
		{
			for (int y = node.nodeY-1; y <= node.nodeY+1; y++)
			{
				if (x == node.nodeX && y == node.nodeY)
					continue;

				if (x > -1 && x < gridX && y > -1 && y < gridY)
				{
					neighbours.Add(nodes[x,y]);
				}
			}
		}*/
        return neighbours;
	}

	void OnDrawGizmos () {
		if (nodes != null && DrawGizmos) {

			Gizmos.color = gridColor;
			foreach (Node n in nodes) {

				Gizmos.color = n.isWalkable?gridColor:Color.red;
				Gizmos.DrawCube(new Vector3(n.worldPos.x, n.worldPos.y, 0), Vector3.one * (gridDiameter-.1f));
			}
		}
	}
	
}

public class Node : IHeapItem<Node>
{
	public bool isWalkable;
	public Vector2 worldPos;
	public int nodeX;
	public int nodeY;

	public int gCost;
	public int hCost;
	public Node parent;
	int Heapindex;

	public Node(bool _isWalkable, Vector2 _worldPos, int _nodeX, int _nodeY)
	{
		isWalkable = _isWalkable;
		worldPos = _worldPos;
		nodeX = _nodeX;
		nodeY = _nodeY;
	}

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public int heapIndex {
		get {
			return Heapindex;
		}
		set {
			Heapindex = value;
		}
	}

	public int CompareTo(Node compareToNode)
	{
		int compare = fCost.CompareTo(compareToNode.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(compareToNode.hCost);
		}
		return (-compare);
	}

}