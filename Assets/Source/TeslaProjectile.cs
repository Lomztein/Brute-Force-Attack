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

	public AnimationCurve arcingCurve;
	public float arcingSpeed;
	private int vertexCount;
	private Vector3[] positions;
	private int arcDir;

	public override void Initialize () {

		lineRenderer.material.color = new Color (1,1,1,1);
		if (isMainLighting) {
			chainIndex = chainAmount;
			Invoke ("ReturnToPool", 0.5f);
		} else {
			Destroy (gameObject, 1f);
		}

		Ray ray = new Ray (transform.position, velocity.normalized);
		RaycastHit hit;

		if (Physics.SphereCast (ray, spherecastWidth, out hit, range, Game.game.enemyLayer)) {
			DrawTeslaBeam (hit.point);
			end = hit.point;
			OnHit (hit.collider, hit.point, transform.right);
		}else{
			DrawTeslaBeam (ray.GetPoint (range));
			end = ray.GetPoint (range);
		}
	}

	public override void OnHit (Collider col, Vector3 point, Vector3 dir) {
		base.OnHit (col, point, dir);
		if (chainIndex > 0) {
			TargetFinder finder = new TargetFinder ();
			Transform t = finder.FindTarget (point, 5f, Game.game.enemyLayer, new Colour[1] { Colour.None }, new Colour[0], TargetFinder.SortType.Random);
			if (t) {
				//Vector3 dir = (t.position - hit.point).normalized;
				GameObject light = (GameObject)Instantiate (chainLighting, point, Quaternion.identity);
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
		if (Random.Range (0, 50) == 0) {
			DrawTeslaBeam (end);
		}else{
			MakeBeamLookWicked ();
		}
		lineRenderer.material.color = new Color (1,1,1,lineRenderer.material.color.a - 2f * Time.fixedDeltaTime);
	}

	void DrawTeslaBeam (Vector3 end) {

		arcDir = Random.Range (-1, 2);
		lineRenderer.SetPosition (0, transform.position);

		Vector3 dir = velocity.normalized * segmentLength;
		vertexCount = (Mathf.CeilToInt (Vector3.Distance (transform.position, end) / segmentLength));
		lineRenderer.SetVertexCount (vertexCount);
		positions = new Vector3[vertexCount];
		positions[0] = transform.position;

		
		for (int i = 1; i < vertexCount - 1; i++) {
			positions[i] = transform.position + dir * i + (Vector3)Random.insideUnitCircle * noiseAmount;
			lineRenderer.SetPosition (i, positions[i]);
		}

		lineRenderer.SetPosition (vertexCount - 1, end);
	}

	void MakeBeamLookWicked () {

		Vector3 rot = Quaternion.Euler (0f, 0f, 90f) * velocity.normalized;
		for (int i = 1; i < vertexCount - 1; i++) {
			positions[i] += (rot * arcingSpeed * Time.fixedDeltaTime * arcingCurve.Evaluate ((float)i / (float)vertexCount) * Random.Range (-arcDir * 1f, arcDir * 4f)) + ((Vector3)Random.insideUnitCircle * noiseAmount / 4f);
			lineRenderer.SetPosition (i, positions[i]);
		}
	}
}
	