using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class EnemyGoblin : Enemy {
        [Header("Parameter")]
        [SerializeField] int power;
        [SerializeField] AttackAction attackAction;
        [SerializeField] string animWalk = "IsWalk";

        public override void MPStart() {
            base.MPStart();
            attackAction.Init();
        }

        public override void Create(Vector3 pos, Quaternion rot) {
            base.Create(pos, rot);

            StartMove();
        }

        protected override void StartMove() {
            base.StartMove();

            myAnimator.SetBool(animWalk, true);
            StartCoroutine(nameof(CoCheckDistance));
        }

        IEnumerator CoCheckDistance() {
            while (true) {
                float distance = Vector3.Distance(myTransform.position, heroTransform.position);

                if (distance <= attackRange) {
                    StartCoroutine(nameof(CoAttack), attackAction);
                    yield break;
                }

                yield return null;
            }
        }

        IEnumerator CoAttack(AttackAction action) {
            myAnimator.SetTrigger(action.Anim);
            yield return new WaitForSeconds(action.PreDelay);

            AttackHero();
            yield return new WaitForSeconds(action.PostDelay);

            StartCoroutine(nameof(CoCheckDistance));
        }

        protected void AttackHero() {
            float distance = Vector3.Distance(myTransform.position, heroTransform.position);

            if (distance <= attackRange) {
                hero.TakeDamage(power);
            }
        }
    }
}
