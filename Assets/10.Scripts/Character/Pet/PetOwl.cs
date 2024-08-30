using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class PetOwl : MonoBehaviour, IMemoryPool {
        [Header("Basic")]
        [SerializeField] string mpGroup;
        [SerializeField] string mpType;
        [SerializeField] Shooter shooter;
        [SerializeField] ParticleSystem shootParticle;
        [SerializeField] float attackRange = 20.0f;
        [SerializeField] float originSpeed;
        [SerializeField] float rotateSpeed = 100f;
        [SerializeField] float followDistance = 1.0f;
        [SerializeField] int startDegree = -30;
        [SerializeField] int stepDegree = 60;
        [SerializeField] float searchInterval = 0.1f;
        [SerializeField] LayerMask layerMask;

        [Header("Shoot Params")]
        [SerializeField] int power;
        [SerializeField] int rpm;

        [Header("Effect")]
        [SerializeField] string createEffect;
        [SerializeField] string destroyEffect;

        float interval;
        Vector3 posOffset;
        public int RPM { get { return rpm; } set { rpm = value; } }

        Transform myTransform;
        Transform playerTransform;
        Transform targetTransform;
        Enemy targetEnemy;
        float originYPos;

        public void MPStart() {
            myTransform = transform;
            originYPos = myTransform.position.y;
            playerTransform = GameManager.Instance.Hero.transform;

            shooter.Init();

            interval = 60f / rpm;
        }

        public void Create(int index, int power) {
            originSpeed = Random.Range(1.0f, 4.0f);

            this.power = power;
            posOffset = new Vector3(
                Mathf.Cos((index * stepDegree + startDegree) * Mathf.Deg2Rad),
                0,
                Mathf.Sin((index * stepDegree + startDegree) * Mathf.Deg2Rad)
            );

            myTransform.position = playerTransform.position + posOffset;

            SpawnEffect();

            StartCoroutine(CoMove());
            StartCoroutine(CoFindTarget());
            StartCoroutine(CoRotateToTarget());
        }

        IEnumerator CoMove() {
            float diff;
            Vector3 targetPos;

            while (true) {
                targetPos = playerTransform.position + posOffset;
                diff = Vector3.Distance(myTransform.position, targetPos);

                if (diff > followDistance) {
                    Vector3 newPos = Vector3.Lerp(myTransform.position, targetPos, originSpeed * Time.deltaTime);
                    newPos.y = originYPos;

                    myTransform.position = newPos;
                }

                yield return null;
            }
        }

        IEnumerator CoRotateToTarget() {
            while (true) {
                RotateToTarget();

                yield return null;
            }
        }

        void RotateToTarget() {
            if (targetTransform == null) {
                return;
            }

            Vector3 movement = targetTransform.position - myTransform.position;
            Quaternion rotate;

            if (movement != Vector3.zero) {
                movement.y = 0.0f;
                rotate = Quaternion.LookRotation(movement);
                myTransform.rotation = Quaternion.Slerp(myTransform.rotation, rotate, rotateSpeed * Time.deltaTime);
            }
        }

        Transform FindTarget() {
            Collider[] colliders = Physics.OverlapSphere(myTransform.position, attackRange, layerMask);

            if (colliders.Length == 1) {
                Enemy enemy = colliders[0].GetComponent<Enemy>();
                if (enemy && enemy.IsDead == false) {
                    targetEnemy = enemy;
                    return colliders[0].transform;
                }
            } else if (colliders.Length > 1) {
                int index = Random.Range(0, colliders.Length);
                Enemy enemy = colliders[index].GetComponent<Enemy>();
                if (enemy && enemy.IsDead == false) {
                    targetEnemy = enemy;
                    return colliders[index].transform;
                }
            }

            return null;
        }

        IEnumerator CoFindTarget() {
            while (true) {
                targetTransform = FindTarget();
                if (targetTransform) {
                    StartCoroutine(CoAttack());
                    yield break;
                }

                yield return new WaitForSeconds(searchInterval);
            }
        }

        public void StartAttack() {
            StartCoroutine(nameof(CoAttack));
        }

        public void StopAttack() {
            StopCoroutine(nameof(CoAttack));
        }

        IEnumerator CoAttack() {
            while (true) {
                if (targetEnemy == null || targetEnemy.IsDead) {
                    targetTransform = null;
                    StartCoroutine(CoFindTarget());
                    yield break;
                }

                shooter.Shoot(power, false);
                shootParticle.Play();

                yield return new WaitForSeconds(interval);
            }
        }

        void SpawnEffect() {
            Effector.PlayEffect(createEffect, myTransform.position + new Vector3(0, 0.6f, 0));
        }

        public void HideAndDestory() {
            Effector.PlayEffect(destroyEffect, myTransform.position + new Vector3(0, 0.6f, 0));
            MyDestory();
        }

        public void MyDestory() {
            PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
        }
    }
}
