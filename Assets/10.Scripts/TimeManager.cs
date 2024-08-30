using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace HornSpirit {
    public class TimeManager : MonoBehaviour {
        public static TimeManager Instance = null;

        [Header("slow down")]
        [SerializeField] float slowdownFactor = 0.05f;
        [SerializeField] private float slowdownLength = 2f;

        public static float deltaRate = 1.0f;
        public static float fixedDeltaRate = 1.0f;
        public static bool paused = false;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(gameObject);
            }
        }

        public static float deltaTime {
            get { return Time.deltaTime * deltaRate; }
        }

        public static float fixedDeltaTime {
            get { return Time.fixedDeltaTime * fixedDeltaRate; }
        }

        public static float deltaTimeUI {
            get { return Time.deltaTime; }
        }

        public static void Pause() {
            deltaRate = 0f;
            fixedDeltaRate = 0f;
            paused = true;
        }

        public static void Resume() {
            deltaRate = 1.0f;
            fixedDeltaRate = 1.0f;
            paused = false;
        }

        public static void SetGameSpeed(float speed) {
            deltaRate = speed;
            fixedDeltaRate = speed;
        }

        public static void ResetGameSpeed() {
            deltaRate = 1.0f;
            fixedDeltaRate = 1.0f;
        }

        public void DoSlowmotion(float slowdownLength) {
            this.slowdownLength = slowdownLength;

            StartCoroutine(CoSlowmotion());
            // SlowmotionAsync();
        }

        private IEnumerator CoSlowmotion() {
            float slowdownRate = 1 / slowdownLength;
            float percent = 0;
            float fixedDeltaTime;

            percent = slowdownFactor;
            fixedDeltaTime = Time.fixedDeltaTime;

            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;

            while (percent < 1) {
                percent += slowdownRate * Time.unscaledDeltaTime;
                Time.timeScale = percent;

                yield return null;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = fixedDeltaTime;
        }

        async void SlowmotionAsync() {
            float slowdownRate = 1 / slowdownLength;
            float percent = 0;

            percent = slowdownFactor;
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;

            while (percent < 1) {
                percent += slowdownRate * Time.unscaledDeltaTime;
                Time.timeScale = percent;

                await Task.Yield();
            }

            Time.timeScale = 1f;
        }
    }
}
