using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshCollector : MonoBehaviour
{
	public void SendScene()
	{
		var sendedData = new SendedData();
		sendedData.cubesData = GetCubes();
		sendedData.cameraData = GetCamera();

		var json = JsonUtility.ToJson(sendedData);
		Debug.Log(json);

		WriteString(json);
		Application.OpenURL($"file://{Application.dataPath}/JSBuild/index.html");
	}

	private List<CubeData> GetCubes()
	{
		List<CubeData> cubesData = new List<CubeData>();
		var meshes = FindObjectsOfType<MeshFilter>();

		foreach (var mesh in meshes)
		{
			if (mesh.sharedMesh.name == "Cube" || mesh.sharedMesh.name == "Cube Instance")
			{
				var cubeData = new CubeData();
				var transformData = GetTransformData(mesh.gameObject.transform);
				cubeData.transformData = transformData;
				cubeData.color = ColorUtility.ToHtmlStringRGB(mesh.gameObject.GetComponent<MeshRenderer>().material.color);
				cubesData.Add(cubeData);
			}
		}

		return cubesData;
	}

	private CameraData GetCamera()
	{
		var camera = GameObject.FindGameObjectWithTag("MainCamera");
		var cameraData = new CameraData();

		if (camera != null)
		{
			cameraData.transformData = GetTransformData(camera.transform);
		}

		return cameraData;
	}

	private TransformData GetTransformData(Transform transform)
	{
		var transformData = new TransformData();

		var zFlipTransform = ZFlipTransform(transform);

		transformData.posX = zFlipTransform.pos.x;
		transformData.posY = zFlipTransform.pos.y;
		transformData.posZ = zFlipTransform.pos.z;

		transformData.rotX = zFlipTransform.rot.x;
		transformData.rotY = zFlipTransform.rot.y;
		transformData.rotZ = zFlipTransform.rot.z;

		transformData.scaleX = zFlipTransform.scale.x;
		transformData.scaleY = zFlipTransform.scale.y;
		transformData.scaleZ = zFlipTransform.scale.z;

		return transformData;
	}

	public static (Vector3 pos, Vector3 rot, Vector3 scale) ZFlipTransform(Transform transform)
	{
		var position = new Vector3
		(
			transform.localPosition.x, 
			transform.localPosition.y, 
			-transform.localPosition.z
		);

		var rotation = new Vector3
		(
			-transform.rotation.eulerAngles.x,
			-transform.rotation.eulerAngles.y,
			transform.rotation.eulerAngles.z
		) * (Mathf.PI / 180);

		var scale = new Vector3
		(
			-transform.localScale.x, 
			-transform.localScale.y, 
			transform.localScale.z
		);

		return (position, rotation, scale);
	}

	static void WriteString(string json)
	{
		var path = "Assets/JSBuild/data.js";

		if (File.Exists(path))
		{
			File.Delete(path);
		}

		StreamWriter writer = new StreamWriter(path, true);
		writer.WriteLine($"var json = '{json}';");
		writer.Close();
	}
}

[CustomEditor(typeof(MeshCollector))]
public class MeshCollectorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var meshCollector = (MeshCollector)target;
		if (GUILayout.Button("Send scene to html"))
		{
			meshCollector.SendScene();
		}
	}
}
