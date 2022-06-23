using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unidice.SDK.System
{
    public class AppLoader : MonoBehaviour
    {
        [SerializeField] private int gameScene;
        [SerializeField] private TMP_Text outputText;

        public static bool Simulate => !Application.isMobilePlatform;
        public AppLoaderLoadStep[] loadSteps;
        
        public void Awake()
        {
            Application.logMessageReceived += OnLog;
            outputText.SetText("Loading...\n");
        }

        public async UniTask Start()
        {
            DontDestroyOnLoad(gameObject);

            foreach (var loadStep in loadSteps)
            {
                Debug.Log($"Loading {loadStep.Label}...");
                await loadStep.Execute(gameObject.GetCancellationTokenOnDestroy());
                Debug.Log($"...done loading {loadStep.Label}");
            }

            if (Simulate)
            // Load simulator
            {
                outputText.SetText("Loading simulator...\n");
                var loadOperation = SceneManager.LoadSceneAsync("Simulator", LoadSceneMode.Single);
                await UniTask.WaitUntil(() => loadOperation.isDone);
                Debug.Log("Done loading simulator.");
            }

            SceneManager.sceneLoaded += SceneLoaded;
            // Load game scene
            {
                outputText.SetText("Loading...\n");
                var loadOperation = SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Additive);
                if (loadOperation == null)
                {
                    Debug.LogError($"Simulator: Failed to load scene {gameScene}.", this);
                    return;
                }

                await UniTask.WaitUntil(() => loadOperation.isDone);
                Debug.Log("Done loading game.");
            }
            SceneManager.sceneLoaded -= SceneLoaded;

            if (Application.isMobilePlatform) QualitySettings.vSyncCount = 0;

            // Done
            Destroy(gameObject);
            Debug.Log("Done unloading loader.");
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
        }

        private void OnLog(string condition, string stacktrace, LogType type)
        {
            if (outputText)
            {
                var content = condition;

                switch (type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        content = $"<color=red>{content}</color>";
                        break;
                    case LogType.Warning:
                        content = $"<color=yellow>{content}</color>";
                        break;
                    default:
                        break;
                }

                outputText.SetText($"{outputText.text}\n{content}");
            }
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Simulate) return;

            // Only use target texture when in simulator
            var target = FindObjectOfType<AppRaycasterTarget>();
            if (target)
            {
                foreach (var targetCamera in target.cameras)
                {
                    targetCamera.targetTexture = null;
                }
            }
        }
    }
}
