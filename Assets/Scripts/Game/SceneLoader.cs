using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [HideInInspector]
    public string SceneName;

    private float loadingProgress = 0;

    public void LoadScene() => StartCoroutine(LoadSceneCo());

    private IEnumerator LoadSceneCo()
    {
        // Load scene asynchronously.
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneName);

        while (!operation.isDone)
        {
            // Make the progress go from 0 to 1, instead of 0 to 0.9
            loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);

            yield return null;
        }
    }
}
