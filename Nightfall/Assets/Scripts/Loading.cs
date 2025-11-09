using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{

    [SerializeField] GameObject loadingScreen;
    [SerializeField] Image loadingBarFill;

    public void LoadScene(int sceneId)
    {

        StartCoroutine(LoadSceneAsync(sceneId));

    }

    IEnumerator LoadSceneAsync(int sceneId)
    {

        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneId);

        loadingScreen.SetActive(true);

        while (!loading.isDone)
        {

            float progressValue = Mathf.Clamp01(loading.progress / 0.9f);

            loadingBarFill.fillAmount = progressValue;

            yield return null;

        }

    }

}
