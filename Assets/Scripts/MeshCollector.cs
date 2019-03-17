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
        Application.OpenURL("file://" + Application.dataPath + "/JSBuild/index.html");
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

        Vector3 oldPosition = transform.position;
        Quaternion OldRotation = transform.rotation;
        Vector3 olDScale = transform.lossyScale;

        ZFlipTransform(transform);

        transformData.posX = transform.position.x;
        transformData.posY = transform.position.y;
        transformData.posZ = transform.position.z;

        transformData.rotX = transform.rotation.eulerAngles.x * (Mathf.PI / 180);
        transformData.rotY = transform.rotation.eulerAngles.y * (Mathf.PI / 180);
        transformData.rotZ = transform.rotation.eulerAngles.z * (Mathf.PI / 180);

        transformData.scaleX = transform.localScale.x;
        transformData.scaleY = transform.localScale.y;
        transformData.scaleZ = transform.localScale.z;

        transform.position = oldPosition;
        transform.rotation = OldRotation;
        transform.localScale = olDScale;

        return transformData;
    }

    public static void ZFlipTransform(Transform transform)
    {
        Vector3 position = transform.localPosition;
        transform.localPosition = new Vector3(position.x, position.y, -position.z);

        Quaternion rotation = transform.localRotation;
        ZFlipRotation(ref rotation);
        transform.localRotation = rotation;

        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-scale.x, -scale.y, scale.z);
    }

    public static void ZFlipRotation(ref Quaternion rotation)
    {
        Vector3 eulerAngles = rotation.eulerAngles;
        rotation.eulerAngles = new Vector3(-eulerAngles.x, -eulerAngles.y, eulerAngles.z);
    }

    static void WriteString(string json)
    {
        string path = "Assets/JSBuild/data.js";

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

        MeshCollector meshCollector = (MeshCollector)target;
        if (GUILayout.Button("Send scene to html"))
        {
            meshCollector.SendScene();
        }
    }
}
