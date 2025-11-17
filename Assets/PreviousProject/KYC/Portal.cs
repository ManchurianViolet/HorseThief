using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 이동할 씬 이름 (Inspector에서 설정)



    // 물리 충돌 방식: 플레이어와 충돌 시 지정된 씬으로 전환
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("HorseChest"))
        {
            Debug.Log("플레이어가 포털과 충돌했습니다! 이동할 씬: " + targetSceneName);
            LoadTargetScene();
        }
    }

    // 지정된 씬으로 전환하는 함수
    private void LoadTargetScene()
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