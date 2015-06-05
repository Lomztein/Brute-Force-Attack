using UnityEngine;
using System.Collections;

public class TeslaProjectile : Projectile {

	public float spherecastWidth;
	public float segmentLength;
	public float noiseAmount;
	public LineRenderer lineRenderer;
	private Vector3 end;

	public static int chainAmount;
	public int chainIndex;
	public GameObject chainLighting;
	public bool isMainLighting;

	public override void Initialize () {

		if (isMainLighting) {
			chainIndex = chainAmount;
			Invoke ("ReturnToPool", 1f);
		} else {
			Destroy (gameObject, 1f);
		}

		Ray ray = new Ray (transform.position, velocity.normalized);
		RaycastHit hit;

		if (Physics.SphereCast (ray, spherecastWidth, out hit, range, Game.game.enemyLayer)) {
			DrawTeslaBeam (hit.point);
			end = hit.point;
			OnHit (hit);
		}else{
			DrawTeslaBeam (ray.GetPoint (range));
			end = ray.GetPoint (range);
		}
	}

	public override void OnHit (RaycastHit hit) {
		base.OnHit (hit);
		if (chainIndex > 0) {
			TargetFinder finder = new TargetFinder ();
			Transform t = finder.FindTarget (hit.point, 5f, Game.game.enemyLayer, new Colour[1] { Colour.None }, TargetFinder.SortType.Random);
			if (t) {
				Vector3 dir = (t.position - hit.point).normalized;
				GameObject light = (GameObject)Instantiate (chainLighting, hit.point, Quaternion.identity);
				TeslaProjectile pro = light.GetComponent<TeslaProjectile>();
				pro.velocity = dir;
				pro.chainIndex = chainIndex - 1;
				pro.damage = Mathf.RoundToInt ((float)damage / 1.5f);
				pro.parent = parent;
				pro.target = t;
				pro.range = 5f;

				pro.Initialize ();
			}
		}
	}

	void FixedUpdate () {
		if (Random.Range (0, 50) == 0)
			DrawTeslaBeam (end);
	}

	void DrawTeslaBeam (Vector3 end) {
		lineRenderer.SetPosition (0, transform.position);
		
		int segments = (Mathf.CeilToInt (Vector3.Distance (transform.position, end) / segmentLength));
		Vector3 dir = velocity.normalized * segmentLength;
		lineRenderer.SetVertexCount (segments);

		for (int i = 1; i < segments - 1; i++) {
			lineRenderer.SetPosition (i, transform.position + dir * i + Random.insideUnitSphere * noiseAmount);
		}

		lineRenderer.SetPosition (segments - 1, end);
	}
}
