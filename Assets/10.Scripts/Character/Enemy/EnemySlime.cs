using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class EnemySlime : Enemy {
        [Header("Parameter")]
        [SerializeField] int damage;
        [SerializeField] float attackDelay;
        [SerializeField] float afterAttackDelay;
        [SerializeField] string animWalk = "IsWalk";
        [SerializeField] string animAttack01 = "Attack01";

        public override void Create(Vector3 pos, Quaternion rot) {
            base.Create(pos, rot);

            StartMove();
            StartCoroutine(nameof(CoCheckDistance));
        }

        protected override void StartMove() {
            base.StartMove();

            myAnimator.SetBool(animWalk, true);
        }

        IEnumerator CoCheckDistance() {
            while (true) {
                float distance = Vector3.Distance(myTransform.position, heroTransform.position);

                if (distance <= attackRange) {
                    myAnimator.SetTrigger(animAttack01);
                    StartCoroutine(nameof(CoAttack));

                    yield break;
                }

                yield return null;
            }
        }

        IEnumerator CoAttack() {
            // aiPath.isStopped = true;
            // aiPath.maxSpeed = 0;

            yield return new WaitForSeconds(attackDelay);

            // effect
            AttackHero();

            yield return new WaitForSeconds(afterAttackDelay);

            // aiPath.isStopped = false;
            // aiPath.maxSpeed = speed;
            StartCoroutine(nameof(CoCheckDistance));
        }

        protected void AttackHero() {
            float distance = Vector3.Distance(myTransform.position, heroTransform.position);

            if (distance <= attackRange) {
                // attack effect
                // hero.TakeDamage(damage)
            }
        }
    }
}
