using UnityEngine;
using System.Collections;

public class Unit_forunit : MonoBehaviour {

	public Transform target;
	public float speed = 5f;
	Vector2[] path;
	int targetIndex = 0;
	Vector3 oldPos;
	int notOld = 0;

	void Start()
	{
		oldPos = target.position;
		PathManager.RequestPath(transform.position, target.position, OnFinished);
	}

	void FixedUpdate()
	{
		if (oldPos != target.position)
		{
			notOld += 2;
			oldPos = target.position;

		}
		else
		{
			if (notOld > 1)
			{
				notOld = 0;
				PathManager.RequestPath(transform.position, target.position, OnFinished);
			}
		}
	}

	public void OnFinished(Vector2[] _path, bool success)
	{
		if (success)
		{
			path = null;
			targetIndex = 0;
			path = _path;
			StopCoroutine("PathMove");
			StartCoroutine("PathMove");
		}
	}

	IEnumerator PathMove()
	{
		Vector3 currentWaypoint = new Vector3(path[0].x, path[0].y, -0.5f);

		while (true)
		{
			if (transform.position == currentWaypoint)
			{
				targetIndex++;
				if (targetIndex >= path.Length)
					yield break;
				currentWaypoint = new Vector3(path[targetIndex].x, path[targetIndex].y, -0.5f);
			}

			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
			yield return null;
		}

	}

	public void OnDrawGizmos()
	{
		if (path != null)
		{
			Gizmos.color = Color.black;
			for (int i = targetIndex; i < path.Length; i++)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawCube(new Vector3(path[i].x, path[i].y, 0), new Vector3(0.2f, 0.2f, 0.2f));

				if (i == targetIndex)
				{
					Gizmos.DrawLine(transform.position,new Vector3(path[i].x, path[i].y, 0));
				}
				else
				{
					Gizmos.DrawLine(new Vector3(path[i-1].x, path[i-1].y, 0), new Vector3(path[i].x, path[i].y, 0));
				}
			}
		}
	}
}
