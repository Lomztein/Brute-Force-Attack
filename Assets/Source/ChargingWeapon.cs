using UnityEngine;
using System.Collections;

public class ChargingWeapon : Weapon {

    public float chargeTime;
    public ParticleSystem chargeParticle;
    public bool continuousFiring;

    private bool isCharged;
    public float endParticleRate;

    public override void Fire (RotatorModule rotator, Vector3 basePos, Vector3 position, string fireFunc = "DoFire") {
        base.Fire (rotator, basePos, position, "Charge");
    }

    public override void Start () {
        base.Start ();
        Utility.ChangeParticleEmmisionRate (chargeParticle, 0f);
    }

    IEnumerator Charge () {
        canFire = false;
        for (int i = 0; i < (int)(chargeTime / Time.fixedDeltaTime); i++) {
            /*if (continuousFiring && isCharged) {
                i = (int)(chargeTime / Time.fixedDeltaTime);
            }*/
            Utility.ChangeParticleEmmisionRate (chargeParticle, Mathf.Lerp (0f, endParticleRate, (float)i / (float)(chargeTime / Time.fixedDeltaTime)));
            yield return new WaitForFixedUpdate ();
        }
        isCharged = true;
        Utility.ChangeParticleEmmisionRate (chargeParticle, 0f);
        StartCoroutine ("DoFire");
    }
}
