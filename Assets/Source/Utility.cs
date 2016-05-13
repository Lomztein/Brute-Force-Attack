using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public static class Utility {

    public static void ChangeParticleEmmisionRate (ParticleSystem system, float rate) {
        ParticleSystem.EmissionModule em = system.emission;
        ParticleSystem.MinMaxCurve mm = em.rate;
        mm.constantMax = rate;
        mm.constantMin = rate;
        em.rate = mm;
    }

    public static string ExtractSaveName (string file) {
        string f = file.Substring (file.LastIndexOf ('/') + 1);
        return f.Substring (0, f.Length - ".dat".Length);
    }

    public static T LoadObjectFromFile<T> (string path) {
        if (File.Exists (path)) {

            BinaryFormatter bf = new BinaryFormatter ();
            FileStream file = File.Open (path, FileMode.Open);

            T data = (T)bf.Deserialize (file);
            file.Close ();
            return data;
        }
        Debug.LogError ("Failed to load file at " + path);
        return default (T);
    }
}

