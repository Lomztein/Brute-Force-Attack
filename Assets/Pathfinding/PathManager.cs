using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PathManager : MonoBehaviour {

	Queue<PathRequest> pathQueue = new Queue<PathRequest>();
	PathRequest currentPathRequest;
	Pathfinding pathfinder;
	bool processingPath = false;

	static PathManager instance;

	void Awake()
	{
		instance = this;
		pathfinder = GetComponent<Pathfinding>();

	}

	public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector2[], bool> function)
	{
		PathRequest request = new PathRequest(pathStart, pathEnd, function);
		instance.pathQueue.Enqueue(request);
		instance.TryNext();
	}

	void TryNext()
	{
		if (!processingPath && pathQueue.Count > 0)
		{
			processingPath = true;
			currentPathRequest = pathQueue.Dequeue();
			pathfinder.StartFindingPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
		}
	}

	public void FinishedProcessingPath(Vector2[] path, bool success)
	{
		currentPathRequest.function(path, success);
		processingPath = false;
		TryNext();
	}

	struct PathRequest
	{
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public Action<Vector2[], bool> function;

		public PathRequest(Vector3 _start, Vector3 _end, Action< Vector2[], bool> _function)
		{
			pathStart = _start;
			pathEnd = _end;
			function = _function;

		}
	}
}
