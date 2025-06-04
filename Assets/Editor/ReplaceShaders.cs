using UnityEngine;
using UnityEditor;

public class ReplaceSpecificShader : EditorWindow
{
    private Shader targetShader;
    private Shader newShader;

    [MenuItem("Tools/Replace Specific Shaders")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceSpecificShader>("Replace Specific Shaders");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Specific Shader in Materials", EditorStyles.boldLabel);
        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);
        newShader = (Shader)EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false);

        if (GUILayout.Button("Replace Shaders"))
        {
            ReplaceShaderInAllMaterials();
        }
    }

    void ReplaceShaderInAllMaterials()
    {
        if (targetShader == null)
        {
            Debug.LogError("Please select a target shader before replacing.");
            return;
        }

        if (newShader == null)
        {
            Debug.LogError("Please select a new shader before replacing.");
            return;
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material");

        for (int i = 0; i < materialGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (material.shader == targetShader)
            {
                Undo.RecordObject(material, "Change Shader");
                material.shader = newShader;
                EditorUtility.SetDirty(material);
                Debug.Log("Changed shader for material: " + material.name);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Shader replacement complete.");
    }
}
