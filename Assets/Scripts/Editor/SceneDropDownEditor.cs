using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneLoader))]
public class SceneDropDownEditor : Editor
{
    private string[] scenes;
    private int choiceIndex;

    private SceneLoader sceneLoader;

    private void Awake()
    {
        if (scenes == null)
            GetAllScenes();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        sceneLoader = (SceneLoader)target;

        choiceIndex = System.Array.IndexOf(scenes, sceneLoader.SceneName);
        choiceIndex = EditorGUILayout.Popup("Scene to load", choiceIndex, scenes);
        sceneLoader.SceneName = scenes[choiceIndex];

        // Notify Editor that changes has been made. Therefore allowing user to save.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(sceneLoader);
            EditorSceneManager.MarkSceneDirty(sceneLoader.gameObject.scene);
        }
    }

    private void GetAllScenes()
    {
        // Load all scene names from build settings.
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        scenes = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
        }
    }
}
