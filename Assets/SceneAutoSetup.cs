using UnityEditor;
using UnityEngine;

public class SceneAutoSetup
{
    [MenuItem("Tools/Setup Assessment Scene")]
    public static void SetupScene()
    {
        CreateCubesWithMeshRenderer();
        CreateCubesWithBoxCollider();
        CreateGameObjectsWithRigidbody();
        CreateSpheresWithMeshAndCollider();
        CreateCombinedComponentObjects();
        CreateInactiveObjects();
        CreateTransformOnlyObjects();

        Debug.Log("✅ Test sahnesi başarıyla oluşturuldu.");
    }

    private static void CreateCubesWithMeshRenderer()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = $"MeshCube_{i}";
        }
    }

    private static void CreateCubesWithBoxCollider()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = new GameObject($"ColliderCube_{i}");
            obj.AddComponent<BoxCollider>();
            GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(obj.transform); // Görsel olsun diye cube child
        }
    }

    private static void CreateGameObjectsWithRigidbody()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = new GameObject($"RigidbodyObj_{i}");
            obj.AddComponent<Rigidbody>();
        }
    }

    private static void CreateSpheresWithMeshAndCollider()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = $"SphereCombo_{i}";
            if (!obj.GetComponent<SphereCollider>())
                obj.AddComponent<SphereCollider>();
            if (!obj.GetComponent<MeshRenderer>())
                obj.AddComponent<MeshRenderer>();
        }
    }

    private static void CreateCombinedComponentObjects()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = new GameObject($"FullCombo_{i}");
            obj.AddComponent<Rigidbody>();
            obj.AddComponent<BoxCollider>();
            obj.AddComponent<MeshRenderer>();
        }
    }

    private static void CreateInactiveObjects()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = new GameObject($"InactiveObject_{i}");
            obj.SetActive(false);
        }
    }

    private static void CreateTransformOnlyObjects()
    {
        for (int i = 0; i < 5; i++)
        {
            var obj = new GameObject($"TransformOnly_{i}");
        }
    }
}
