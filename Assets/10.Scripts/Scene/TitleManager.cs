using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ControlFreak2;
using DarkTonic.MasterAudio;

namespace HornSpirit {
    public class TitleManager : MonoBehaviour {
        [SerializeField] string battleSceneName = "Battle";
        [SerializeField] float startDelay = 3.0f;
        [SerializeField] string startSFX;

        void Start() {
            StartCoroutine(nameof(CoCheckController));
        }

        public void OnStartGame() {
            StartCoroutine(CoLoadScene());
        }

        IEnumerator CoLoadScene() {
            AsyncOperation async = SceneManager.LoadSceneAsync(battleSceneName);
            async.allowSceneActivation = false;

            while (!async.isDone) {
                if (async.progress >= 0.9f) {
                    async.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        IEnumerator CoCheckController() {
            while (true) {
                CheckController();
                yield return null;
            }
        }

        void CheckController() {
            if (CF2Input.GetButtonDown("BottomFace")) {
                StopCoroutine(nameof(CoCheckController));
                MasterAudio.StopPlaylist();
                MasterAudio.PlaySound(startSFX);
                Invoke(nameof(OnStartGame), startDelay);
            }
        }
    }
}
