using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Effector : MonoBehaviour, IMemoryPool {
        [Header("Basic")]
        [SerializeField] protected string mpType;
        [SerializeField] protected string mpGroup = "Effect";
        protected ParticleSystem effect;
        protected float playTime;
        protected Transform myTransform;

        public string MPType { get { return mpType; } set { mpType = value; } }
        public string MPGroup { get { return mpGroup; } set { mpGroup = value; } }

        public virtual void MPStart() {
            ParticleSystem.MinMaxCurve startLifeTime;
            myTransform = transform;

            effect = GetComponent<ParticleSystem>();
            startLifeTime = effect.main.startLifetime;
            playTime = effect.main.duration;

            switch (startLifeTime.mode) {
                case ParticleSystemCurveMode.Constant:
                    playTime += startLifeTime.constant;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    playTime += startLifeTime.constantMax;
                    break;
            }
        }

        public void DestroyAfterPlay(Vector3 pos) {
            myTransform.position = pos;

            effect.time = 0;
            effect.Play();
            StartCoroutine(nameof(CoDestroy), playTime);
        }

        public void DestroyAfterPlayInLocalPosition(Vector3 pos) {
            myTransform.localPosition = pos;

            effect.time = 0;
            effect.Play();
            StartCoroutine(nameof(CoDestroy), playTime);
        }

        public void DestroyAfterPlay(Vector3 pos, Quaternion rot) {
            myTransform.rotation = rot;

            DestroyAfterPlay(pos);
        }

        public void DestroyAfterPlay(Vector3 pos, float time) {
            myTransform.position = pos;

            effect.time = 0;
            effect.Play();
            StartCoroutine(nameof(CoDestroy), time);
        }

        public void DestroyAfterPlay(Vector3 pos, Quaternion rot, float time) {
            myTransform.rotation = rot;
            DestroyAfterPlay(pos, time);
        }

        public void Play(Vector3 pos) {
            myTransform.position = pos;

            effect.time = 0;
            effect.Play();
        }

        private void PlayInParent(Vector3 pos) {
            myTransform.localPosition = pos;

            effect.time = 0;
            effect.Play();
        }

        public void Stop() {
            StartCoroutine(nameof(CoDestroy), playTime);
        }

        public void StopNow() {
            StartCoroutine(nameof(CoDestroy), 0);
        }

        IEnumerator CoDestroy(float playTime) {
            yield return new WaitForSeconds(playTime);

            PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
        }

        public static void PlayEffect(string type, Vector3 pos) {
            Effector effector = PoolManager.Instance.NewItem<Effector>("Effect", type);
            if (effector) {
                effector.DestroyAfterPlay(pos);
            }
        }

        public static void PlayEffect(string type, Vector3 pos, Transform parent) {
            Effector effector = PoolManager.Instance.NewItem<Effector>("Effect", type);
            if (effector) {
                effector.transform.SetParent(parent);
                effector.DestroyAfterPlayInLocalPosition(pos);
            }
        }

        public static void PlayEffect(string group, string type, Vector3 pos) {
            Effector effector = PoolManager.Instance.NewItem<Effector>(group, type);
            if (effector) {
                effector.DestroyAfterPlay(pos);
            }
        }

        public static void PlayEffect(string group, string type, Vector3 pos, Quaternion rot) {
            Effector effector = PoolManager.Instance.NewItem<Effector>(group, type);
            if (effector) {
                effector.DestroyAfterPlay(pos, rot);
            }
        }

        public static void PlayEffect(string type, Vector3 pos, Quaternion rot) {
            Effector effector = PoolManager.Instance.NewItem<Effector>("Effect", type);
            if (effector) {
                effector.DestroyAfterPlay(pos, rot);
            }
        }

        public static void PlayEffectWithTime(string type, Vector3 pos, Quaternion rot, float time) {
            Effector effector = PoolManager.Instance.NewItem<Effector>("Effect", type);
            if (effector) {
                effector.DestroyAfterPlay(pos, rot, time);
            }
        }

        public static void PlayEffectWithTime(string type, Vector3 pos, float time) {
            Effector effector = PoolManager.Instance.NewItem<Effector>("Effect", type);
            if (effector) {
                effector.DestroyAfterPlay(pos, time);
            }
        }

        // public void PlaySpecialMoveCollision(Vector3 pos, Vector3 lookAt, Vector3 rotationOffset, bool useLookAt, SpecialMoveParticleCollision.RotationType rotationType) {
        //     myTransform.position = pos;

        //     if (useLookAt) {
        //         myTransform.LookAt(lookAt);
        //     }

        //     switch (rotationType) {
        //         case SpecialMoveParticleCollision.RotationType.Assign:
        //             myTransform.rotation = Quaternion.Euler(rotationOffset);
        //             break;
        //         case SpecialMoveParticleCollision.RotationType.Multiply:
        //             myTransform.rotation *= Quaternion.Euler(rotationOffset);
        //             break;
        //     }

        //     effect.time = 0;
        //     effect.Play();
        //     StartCoroutine("CoDestroy", playTime);
        // }

        public static Effector Play(string type, Vector3 pos, Transform parent = null) {
            Effector effector = PoolManager.Instance.NewItem<Effector>("Effect", type);
            if (effector) {
                if (parent) {
                    effector.transform.SetParent(parent);
                    effector.PlayInParent(pos);
                } else {
                    effector.Play(pos);
                }

                return effector;
            }

            return null;
        }
    }
}
