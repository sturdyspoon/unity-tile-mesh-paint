using UnityEngine;
using UnityEditor;
using System.Linq;

public class TileMeshPaint : EditorWindow
{
    bool shouldRepaint;

    Sprite tile;
    Vector3 tileCenter;
    Vector2 tileHalfSize;

    string[] rotationOptions = new string[] { "0", "90", "180", "270" };
    int rotationIndex = 0;

    MeshCollider mouseOverMeshCollider;
    int[] mouseOverIndexes;
    Vector3[] mouseOverQuad;

    const float outlineWidth = 3f;

    [MenuItem("/Window/Tile Mesh Paint")]
    public static void ShowWindow()
    {
        TileMeshPaint window = GetWindow<TileMeshPaint>();
        window.titleContent = new GUIContent("Tile Mesh Paint");
        window.Show();
    }

    void OnGUI()
    {
        Texture t = GetActiveTexture();

        if (t == null)
        {
            string helpMessage = "Select a Game Object which has a Mesh Renderer, Mesh Collider and a Material with a Texture.";
            EditorGUILayout.HelpBox(helpMessage, MessageType.Info);
        }
        else
        {
            string spriteSheet = AssetDatabase.GetAssetPath(t);
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();

            float width = position.width;
            float scale = width / t.width;
            float height = t.height * scale;
            Rect rect = new Rect(0, 0, width, height);
            EditorGUI.DrawTextureTransparent(rect, t);

            if (tileCenter == Vector3.zero)
            {
                SetTile(sprites[0]);
            }

            foreach (Sprite s in sprites)
            {
                float w = s.rect.width * scale;
                float h = s.rect.height * scale;
                float x = s.rect.x * scale;
                float y = height - h - (s.rect.y * scale);

                if (GUI.Button(new Rect(x, y, w, h), "", new GUIStyle()))
                {
                    SetTile(s);
                }

                if (tile == s)
                {
                    EditorGUI.DrawRect(new Rect(x, y, w - (outlineWidth / 2f), outlineWidth), Color.white);
                    EditorGUI.DrawRect(new Rect(x, y + h - outlineWidth, w - (outlineWidth / 2f), outlineWidth), Color.white);
                    EditorGUI.DrawRect(new Rect(x, y, outlineWidth, h - (outlineWidth / 2f)), Color.white);
                    EditorGUI.DrawRect(new Rect(x + w - outlineWidth, y, outlineWidth, h - (outlineWidth / 2f)), Color.white);
                }
            }

            GUILayout.Space(height);

            GUILayout.Label("");

            rotationIndex = EditorGUILayout.Popup("Rotation", rotationIndex, rotationOptions);


            if (GUILayout.Button(new GUIContent("Unshare Mesh Vertices", "Regenerate the mesh so each triangle has its own unshared vertices. This is needed if you want to paint a tile on the mesh and not affect adjacent tiles.")))
            {
                UnshareMeshVertices();
            }

            if (GUILayout.Button(new GUIContent("Create Mesh Asset", "Create and save a new Mesh Asset. This is needed if you want to reuse the mesh in a prefab and the mesh is not in the Assets folder.")))
            {
                CreateMeshAsset();
            }
        }
    }

    Texture GetActiveTexture()
    {
        Texture t = null;

        if (Selection.activeGameObject != null)
        {
            MeshRenderer meshRenderer = Selection.activeGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                t = meshRenderer.sharedMaterial.GetTexture("_MainTex");
            }
        }

        return t;
    }

    void CreateMeshAsset()
    {
        MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();

        Mesh mesh = meshFilter.sharedMesh;

        string path = EditorUtility.SaveFilePanel("Create Mesh Asset", "Assets/", mesh.name, "asset");

        if (!string.IsNullOrEmpty(path))
        {
            path = FileUtil.GetProjectRelativePath(path);

            if (MeshAssetAlreadyExists(mesh))
            {
                mesh = CopyMesh(meshFilter);
            }

            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
        }
    }

    bool MeshAssetAlreadyExists(Mesh mesh)
    {
        string path = AssetDatabase.GetAssetPath(mesh);
        return !string.IsNullOrEmpty(path);
    }

    Mesh CopyMesh(MeshFilter meshFilter)
    {
        Mesh mesh = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
        mesh.name = meshFilter.sharedMesh.name;

        Undo.RecordObject(meshFilter, "Copy Mesh");
        meshFilter.sharedMesh = mesh;

        MeshCollider meshCollider = meshFilter.GetComponent<MeshCollider>();

        if (meshCollider != null)
        {
            Undo.RecordObject(meshCollider, "Copy Mesh");
            meshCollider.sharedMesh = mesh;
        }

        return mesh;
    }

    void UnshareMeshVertices()
    {
        MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();

        Mesh mesh = meshFilter.sharedMesh;

        string path = AssetDatabase.GetAssetPath(mesh);

        if (IsNotSaveableMesh(path))
        {
            mesh = CopyMesh(meshFilter);
        }

        Undo.RecordObject(mesh, "Change UVs");

        int[] newTriangles = new int[mesh.triangles.Length];
        Vector3[] newVertices = new Vector3[mesh.triangles.Length];
        Vector2[] newUV = new Vector2[mesh.triangles.Length];

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            newTriangles[i] = i;
            newVertices[i] = mesh.vertices[mesh.triangles[i]];
            newUV[i] = mesh.uv[mesh.triangles[i]];
        }

        mesh.Clear();

        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;

        mesh.RecalculateNormals();

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.SaveAssets();
        }
    }

    void SetTile(Sprite s)
    {
        tile = s;

        Vector2 c = Vector2.zero;

        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;
        float maxY = Mathf.NegativeInfinity;

        foreach (Vector2 v in s.uv)
        {
            c += v;

            if (v.x < minX)
            {
                minX = v.x;
            }

            if (v.x > maxX)
            {
                maxX = v.x;
            }

            if (v.y < minY)
            {
                minY = v.y;
            }

            if (v.y > maxY)
            {
                maxY = v.y;
            }
        }

        tileCenter = c / 4f;

        tileHalfSize = new Vector2(maxX - minX, maxY - minY) / 2f;
    }

    void OnSelectionChange()
    {
        Repaint();
        tile = null;
        tileCenter = Vector3.zero;
        tileHalfSize = Vector2.zero;
    }

    void OnDestroy()
    {
        Undo.undoRedoPerformed -= UndoOrRedo;
        SceneView.duringSceneGui -= this.OnSceneGUI;
        Tools.hidden = false;
    }

    private void OnBecameVisible()
    {
        Undo.undoRedoPerformed -= UndoOrRedo;
        Undo.undoRedoPerformed += UndoOrRedo;
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
        shouldRepaint = true;
        Tools.hidden = true;
    }

    private void OnBecameInvisible()
    {
        Undo.undoRedoPerformed -= UndoOrRedo;
        SceneView.duringSceneGui -= this.OnSceneGUI;
        Tools.hidden = false;
    }

    void UndoOrRedo()
    {
        if (Selection.activeGameObject != null)
        {
            MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh.vertices = meshFilter.sharedMesh.vertices;
            meshFilter.sharedMesh.uv = meshFilter.sharedMesh.uv;
            meshFilter.sharedMesh.triangles = meshFilter.sharedMesh.triangles;
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else
        {
            UpdateMouseOverInfo();

            if (guiEvent.modifiers == EventModifiers.None && mouseOverQuad != null)
            {
                /* Prevent the current GameObject from being deselected in the Heirarchy by selecting a Default control. */
                if (guiEvent.type == EventType.Layout)
                {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }
                else
                {
                    HandleInput(guiEvent);
                }
            }
        }
    }

    void Draw()
    {
        if (mouseOverQuad != null)
        {
            Handles.color = Color.white;
            Handles.DrawLine(mouseOverQuad[0], mouseOverQuad[1]);
            Handles.DrawLine(mouseOverQuad[1], mouseOverQuad[2]);
            Handles.DrawLine(mouseOverQuad[2], mouseOverQuad[0]);
            Handles.DrawLine(mouseOverQuad[3], mouseOverQuad[4]);
            Handles.DrawLine(mouseOverQuad[4], mouseOverQuad[5]);
            Handles.DrawLine(mouseOverQuad[5], mouseOverQuad[3]);
        }
    }

    void UpdateMouseOverInfo()
    {
        mouseOverMeshCollider = null;
        mouseOverIndexes = null;
        mouseOverQuad = null;

        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        RaycastHit hit;
        if (!Physics.Raycast(mouseRay, out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null || Selection.activeGameObject != meshCollider.gameObject)
            return;

        mouseOverMeshCollider = meshCollider;

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        int triangleIndex = hit.triangleIndex * 3;

        Transform hitTransform = hit.collider.transform;

        int i0 = triangles[triangleIndex + 0];
        int i1 = triangles[triangleIndex + 1];
        int i2 = triangles[triangleIndex + 2];

        Vector3 p0 = hitTransform.TransformPoint(vertices[i0]);
        Vector3 p1 = hitTransform.TransformPoint(vertices[i1]);
        Vector3 p2 = hitTransform.TransformPoint(vertices[i2]);

        float maxDist = Vector3.Distance(p0, p1);
        Vector3 a = vertices[i0];
        Vector3 b = vertices[i1];

        if (maxDist < Vector3.Distance(p1, p2))
        {
            maxDist = Vector3.Distance(p1, p2);
            a = vertices[i1];
            b = vertices[i2];
        }

        if (maxDist < Vector3.Distance(p2, p0))
        {
            maxDist = Vector3.Distance(p2, p0);
            a = vertices[i2];
            b = vertices[i0];
        }

        for (int j = 0; j < triangles.Length; j += 3)
        {
            int j0 = triangles[j + 0];
            int j1 = triangles[j + 1];
            int j2 = triangles[j + 2];

            Vector3 v0 = vertices[j0];
            Vector3 v1 = vertices[j1];
            Vector3 v2 = vertices[j2];

            if (j != triangleIndex && (a == v0 || a == v1 || a == v2) && (b == v0 || b == v1 || b == v2))
            {
                mouseOverIndexes = new[] { i0, i1, i2, j0, j1, j2 };
                mouseOverQuad = new[] { p0, p1, p2, hitTransform.TransformPoint(v0), hitTransform.TransformPoint(v1), hitTransform.TransformPoint(v2) };
                break;
            }
        }
    }

    void HandleInput(Event guiEvent)
    {
        if ((guiEvent.type == EventType.MouseDown || guiEvent.type == EventType.MouseDrag) && guiEvent.button == 0)
        {
            MeshFilter meshFilter = mouseOverMeshCollider.GetComponent<MeshFilter>();

            Mesh mesh = meshFilter.sharedMesh;

            string path = AssetDatabase.GetAssetPath(mesh);

            if (IsNotSaveableMesh(path))
            {
                mesh = CopyMesh(meshFilter);
            }

            Undo.RecordObject(mesh, "Change UVs");

            Vector2[] uv = new Vector2[mesh.uv.Length];

            mesh.uv.CopyTo(uv, 0);

            Vector3 quadCenter = Vector3.zero;

            for (int i = 0; i < mouseOverQuad.Length; i++)
            {
                quadCenter += mouseOverQuad[i];
            }

            quadCenter /= mouseOverQuad.Length;

            for (int i = 0; i < mouseOverQuad.Length; i++)
            {
                uv[mouseOverIndexes[i]] = GetUV(mouseOverQuad[i] - quadCenter);
            }

            mesh.uv = uv;

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.SaveAssets();
            }
        }

        shouldRepaint = true;
    }

    bool IsNotSaveableMesh(string path)
    {
        return !string.IsNullOrEmpty(path) && (!path.StartsWith("Assets") || !path.EndsWith(".asset"));
    }

    Vector2 GetUV(Vector3 dir)
    {
        Vector3 forward = SceneView.lastActiveSceneView.camera.transform.forward;
        Vector3 v = Vector3.ProjectOnPlane(dir, forward);

        Vector3 up = SceneView.lastActiveSceneView.camera.transform.up;
        float angle = Vector3.SignedAngle(v, up, forward);

        Vector2 disp;

        if (angle >= 0 && angle < 90)
        {
            disp = new Vector2(tileHalfSize.x, tileHalfSize.y);
        }
        else if (angle >= -90 && angle < 0)
        {
            disp = new Vector2(-tileHalfSize.x, tileHalfSize.y);
        }
        else if (angle >= -180 && angle < -90)
        {
            disp = new Vector2(-tileHalfSize.x, -tileHalfSize.y);
        }
        else
        {
            disp = new Vector2(tileHalfSize.x, -tileHalfSize.y);
        }

        float rotationAngle = int.Parse(rotationOptions[rotationIndex]);
        Vector2 result = tileCenter + (Quaternion.AngleAxis(rotationAngle, Vector3.forward) * disp);
        return result;
    }

    void Update()
    {
        if (shouldRepaint)
        {
            SceneView.lastActiveSceneView.Repaint();
            shouldRepaint = false;
        }
    }
}