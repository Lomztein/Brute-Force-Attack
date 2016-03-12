using UnityEngine;
using System.Collections;

public static class Utility {

    public static void ChangeParticleEmmisionRate (ParticleSystem system, float rate) {
        ParticleSystem.EmissionModule em = system.emission;
        ParticleSystem.MinMaxCurve mm = em.rate;
        mm.constantMax = rate;
        mm.constantMin = rate;
        em.rate = mm;
    }
}

