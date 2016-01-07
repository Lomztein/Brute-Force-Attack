using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Assembly {

	public string assemblyName;
	public string assemblyDesc;

	public List<Part> parts = new List<Part>();

	[System.NonSerialized]
    public Texture2D texture;

	[System.NonSerialized]
    public Button[] requiredResearch;

    [System.Serializable]
    public class Part {
        public bool isRoot;
		public string type;
        public int index;
        public int parentIndex;
        public float x;
		public float y;
        public float angle;

		public Part (bool _isRoot, string _type, int _index, int _parentIndex, float _x , float _y, float _angle) {
			isRoot = _isRoot;
			type = _type;
			index = _index;
			parentIndex = _parentIndex;
			x = _x;
			y = _y;
			angle = _angle;
		}

		public Part () {
		}
    }

	public static Assembly LoadFromFile (string fileName, bool isFullPath = false) {

		if (!isFullPath) fileName = Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + fileName + Module.MODULE_FILE_EXTENSION;

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (fileName, FileMode.Open);
		
		Assembly data = (Assembly)bf.Deserialize (file);
		file.Close ();

		Texture2D[] sprites = null;
		Vector3[] positions = null;

		ModuleAssemblyLoader.GetSpriteData (data, out sprites, out positions);
		data.texture = Module.CombineSprites (sprites, positions);

		return data;
	}

	public static void SaveToFile (string fileName, Assembly assembly) {
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + fileName + Module.MODULE_FILE_EXTENSION);
		
		bf.Serialize (file, assembly);
		file.Close ();
	}
}
