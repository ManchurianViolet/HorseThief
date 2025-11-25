using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRestart : MonoBehaviour
{



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // 현재 씬 인덱스 가져온 후 재시작
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
