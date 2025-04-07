using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameObjectManagerWindow : EditorWindow
{
    private string searchText = "";
    private bool filterMeshRenderer;
    private bool filterCollider;
    private bool filterRigidbody;

    private Vector2 scrollPos;
    private List<GameObject> allSceneObjects = new List<GameObject>();
    private List<GameObject> displayedObjects = new List<GameObject>();
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();

    [MenuItem("Tools/🛠️ GameObject Manager")]
    public static void OpenWindow()
    {
        GetWindow<GameObjectManagerWindow>("🧠 GameObject Manager");
    }

    private void OnEnable()
    {
        RefreshSceneObjects();
    }

    private void OnGUI()
    {
        DrawSearchSection();
        DrawFilterSection();
        DrawGameObjectListSection();
        DrawTransformEditorSection();
        DrawBatchActionsSection();
    }

    #region UI Sections

    private void DrawSearchSection()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("🔍 Search", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        searchText = EditorGUILayout.TextField("Search by Name", searchText);
        if (EditorGUI.EndChangeCheck())
            UpdateDisplayedObjects();
    }

    private void DrawFilterSection()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("🧩 Filter by Components", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();

        filterMeshRenderer = EditorGUILayout.ToggleLeft("Has MeshRenderer", filterMeshRenderer);
        filterCollider = EditorGUILayout.ToggleLeft("Has Collider", filterCollider);
        filterRigidbody = EditorGUILayout.ToggleLeft("Has Rigidbody", filterRigidbody);

        if (EditorGUI.EndChangeCheck())
            UpdateDisplayedObjects();
    }

    private void DrawGameObjectListSection()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("📋 Scene GameObjects", EditorStyles.boldLabel);

        if (GUILayout.Button("🔄 Refresh Scene Objects"))
            RefreshSceneObjects();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250));
        foreach (var go in displayedObjects)
        {
            EditorGUILayout.BeginHorizontal();

            bool wasSelected = selectedObjects.Contains(go);
            bool isNowSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(20));
            if (isNowSelected != wasSelected)
            {
                if (isNowSelected) selectedObjects.Add(go);
                else selectedObjects.Remove(go);
            }

            bool newActive = EditorGUILayout.Toggle(go.activeSelf, GUILayout.Width(20));
            if (newActive != go.activeSelf)
            {
                Undo.RecordObject(go, "Toggle Active State");
                go.SetActive(newActive);
                EditorUtility.SetDirty(go);
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawTransformEditorSection()
{
    if (selectedObjects.Count == 0) return;

    GUILayout.Space(10);
    EditorGUILayout.LabelField("🧱 Transform Editor", EditorStyles.boldLabel);

    if (selectedObjects.Count == 1)
    {
        GameObject selected = selectedObjects.First();
        Transform tf = selected.transform;

        EditorGUI.BeginChangeCheck();

        Vector3 newPos = EditorGUILayout.Vector3Field("Position", tf.position);
        Vector3 newRot = EditorGUILayout.Vector3Field("Rotation", tf.eulerAngles);
        Vector3 newScale = EditorGUILayout.Vector3Field("Scale", tf.localScale);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tf, "Edit Transform");

            tf.position = newPos;
            tf.eulerAngles = newRot;
            tf.localScale = newScale;

            EditorUtility.SetDirty(tf);
        }

        if (GUILayout.Button("🔁 Reset Transform"))
        {
            Undo.RecordObject(tf, "Reset Transform");
            tf.position = Vector3.zero;
            tf.rotation = Quaternion.identity;
            tf.localScale = Vector3.one;
            EditorUtility.SetDirty(tf);
        }
    }
    else
    {
        EditorGUILayout.HelpBox("Select only one GameObject to edit its Transform directly.", MessageType.Info);

        GameObject reference = selectedObjects.First();
        Transform tf = reference.transform;

        Vector3 pos = EditorGUILayout.Vector3Field("Position", tf.position);
        Vector3 rot = EditorGUILayout.Vector3Field("Rotation", tf.eulerAngles);
        Vector3 scale = EditorGUILayout.Vector3Field("Scale", tf.localScale);

        if (GUILayout.Button("✏️ Apply to All Selected"))
        {
            foreach (var obj in selectedObjects)
            {
                Undo.RecordObject(obj.transform, "Apply Transform");
                obj.transform.position = pos;
                obj.transform.eulerAngles = rot;
                obj.transform.localScale = scale;
                EditorUtility.SetDirty(obj.transform);
            }
        }

        if (GUILayout.Button("🔁 Reset All Selected Transforms"))
        {
            foreach (var obj in selectedObjects)
            {
                Undo.RecordObject(obj.transform, "Reset Transform");
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(obj.transform);
            }
        }
    }
}


    private void DrawBatchActionsSection()
    {
        if (selectedObjects.Count == 0) return;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("⚙️ Batch Component Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("➕ Add Rigidbody"))
        {
            foreach (var obj in selectedObjects)
                if (obj.GetComponent<Rigidbody>() == null)
                    Undo.AddComponent<Rigidbody>(obj);
        }

        if (GUILayout.Button("➖ Remove Rigidbody"))
        {
            foreach (var obj in selectedObjects)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                    Undo.DestroyObjectImmediate(rb);
            }
        }

        if (GUILayout.Button("➕ Add BoxCollider"))
        {
            foreach (var obj in selectedObjects)
                if (obj.GetComponent<Collider>() == null)
                    Undo.AddComponent<BoxCollider>(obj);
        }

        if (GUILayout.Button("➖ Remove All Colliders"))
        {
            foreach (var obj in selectedObjects)
            {
                foreach (var col in obj.GetComponents<Collider>())
                    Undo.DestroyObjectImmediate(col);
            }
        }
    }

    #endregion

    #region Utility Methods

    private void RefreshSceneObjects()
    {
        allSceneObjects = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.hideFlags == HideFlags.None && go.scene.isLoaded)
            .ToList();

        UpdateDisplayedObjects();
    }

    private void UpdateDisplayedObjects()
    {
        displayedObjects = allSceneObjects.Where(go =>
            (string.IsNullOrEmpty(searchText) || go.name.ToLower().Contains(searchText.ToLower())) &&
            (!filterMeshRenderer || go.GetComponent<MeshRenderer>() != null) &&
            (!filterCollider || go.GetComponent<Collider>() != null) &&
            (!filterRigidbody || go.GetComponent<Rigidbody>() != null)
        ).ToList();
    }

    #endregion
}

