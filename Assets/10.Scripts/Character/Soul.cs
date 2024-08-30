using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Soul : MonoBehaviour, IMemoryPool {
        [Header("Parameter")]
        [SerializeField] int exp = 5;
        [SerializeField] string mpType = "Soul";
        [SerializeField] string mpGroup = "Object";
        [SerializeField] float speed = 1.0f;
        [SerializeField] float spreadSpeed = 1.0f;
        [SerializeField] float spreadTime = 0.3f;
        [SerializeField] float spreadDistance = 1.0f;
        [SerializeField] float spreadHeight = 2.0f;
        [SerializeField] float spreadDelay = 0.2f;

        Transform myTransform;
        Transform heroTransform;
        Hero hero;

        public void MPStart() {
            myTransform = transform;
            hero = GameManager.Instance.Hero;
            heroTransform = hero.transform;
        }

        public void Create(Vector3 pos, Quaternion rot, int exp = 5) {
            myTransform.SetPositionAndRotation(pos, rot);
            StartCoroutine(CoSpread());
        }

        IEnumerator CoSpread() {
            Vector3 targetPos = new(Random.Range(-spreadDistance, spreadDistance),
                    Random.Range(0, spreadDistance), Random.Range(-spreadDistance, spreadDistance));
            targetPos = targetPos.normalized;

            float spreadRate = 1 / spreadTime;
            float percent = 0;

            while (percent < 1) {
                myTransform.Translate(spreadSpeed * Time.deltaTime * targetPos);
                percent += spreadRate * Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }

            spreadRate = 1 / spreadDelay;
            percent = 0;

            while (percent < 1) {
                myTransform.Translate(5f * Time.deltaTime * targetPos);
                percent += spreadRate * Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }

            StartCoroutine(CoFollowing());
        }

        IEnumerator CoFollowing() {
            while (true) {
                myTransform.position = Vector3.Lerp(myTransform.position, heroTransform.position, speed * Time.deltaTime);

                yield return new WaitForFixedUpdate();
            }
        }

        void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Hero")) {
                hero.AddExp(exp);
                MyDestroy();
            }
        }

        void MyDestroy() {
            PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
        }
    }
}
