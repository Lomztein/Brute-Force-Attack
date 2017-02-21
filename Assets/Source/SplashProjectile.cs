using UnityEngine;
using System.Collections;

public class SplashProjectile : HomingProjectile {

    public float splashRange = 3f;
    public float splashDamageMultiplier = 1f;

    public override void OnHit ( Collider col, Vector3 point, Vector3 dir ) {
        base.OnHit (col, point, dir);
        Collider[] nearby = Physics.OverlapSphere (transform.position, splashRange);
        for (int i = 0; i < nearby.Length; i++) {
            Damage d = new Damage (Mathf.RoundToInt (damage * splashDamageMultiplier), effectiveAgainst, parentWeapon);
            nearby[i].SendMessage ("OnTakeDamage", d, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmos () {
        Gizmos.DrawWireSphere (transform.position, splashRange);
    }
}
