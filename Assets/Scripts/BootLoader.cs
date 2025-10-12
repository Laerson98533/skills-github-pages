using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizRush
{
    /// <summary>
    /// Minimal boot loader responsible for initializing services and loading the main scene.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private float minimumSplashTime = 1f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(minimumSplashTime);
            SceneManager.LoadSceneAsync("Main");
        }
    }
}
