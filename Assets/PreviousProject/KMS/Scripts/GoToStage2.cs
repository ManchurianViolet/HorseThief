using UnityEngine;
using UnityEngine.SceneManagement;
public class GoToStage2 : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 이동할 씬 이름 (Inspector에서 설정)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetSceneName = "HowToPlay Scene";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // 씬이 빌드 설정에 포함되어 있는지 확인
            if (IsSceneInBuild(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("지정한 씬 이름이 빌드 설정에 없습니다: " + targetSceneName);
            }
        }
        else
        {
            Debug.LogWarning("이동할 씬 이름이 설정되지 않았습니다. targetSceneName을 설정하세요.");
        }
    }

    // 씬이 빌드 설정에 포함되어 있는지 확인하는 함수
    private bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}
