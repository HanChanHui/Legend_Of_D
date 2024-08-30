using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HornSpirit {
    public class NotificationPanel : MonoBehaviour {
        [Header("Parameters")]
        [SerializeField] string titleSceneName;

        GameController gameController;

        public void Init() {
            gameController = GameManager.Instance.GameController;
        }

        public void Show() {
            gameObject.SetActive(true);
            gameController.EnableAction(GameController.Type.NotificationPanel);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void OnConfirmButton() {
            StartCoroutine(CoLoadScene(titleSceneName));
        }

        IEnumerator CoLoadScene(string sceneName) {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            while (!async.isDone) {
                if (async.progress >= 0.9f) {
                    async.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}
