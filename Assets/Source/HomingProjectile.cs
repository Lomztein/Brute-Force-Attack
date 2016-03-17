using UnityEngine;
using System.Collections;

public class HomingProjectile : Projectile {

	public float turnSpeed;
	public float speed;
	public static bool autoFindTarget;
    public bool slowWhenTurning;
	private TargetFinder targetFinder = new TargetFinder ();

	public override void Initialize () {
		speed = velocity.magnitude;
	}

	public virtual void FixedUpdate () {

		float startAngle = Angle.CalculateAngle (Vector3.zero, velocity);
		transform.rotation = Quaternion.Euler (0, 0, startAngle);

        float rotateMul = 0f;
        if (target && slowWhenTurning)
            rotateMul = (1f + Mathf.Max (1-Vector3.Dot (velocity.normalized, (target.position - transform.position).normalized), 0f));

        if (target)
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, Angle.CalculateAngle (transform, target)), turnSpeed * Time.fixedDeltaTime * rotateMul);

        velocity = transform.right * speed;

        CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

		if (autoFindTarget && !target)
			target = targetFinder.FindTarget (transform.position, 5f, Game.game.enemyLayer, new Colour[1] { Colour.Red }, new Colour[0], TargetFinder.SortType.Closest);
	}
}