using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class EnemyGoblinSkull : Enemy {
        [Header("Parameter")]
        [SerializeField] Shooter shooter;
        [SerializeField] int power;
        [SerializeField] AttackAction attackAction;
        [SerializeField] string animWalk = "Walk";
        [SerializeField] float attackInterval = 3f;
        [SerializeField] float rangedAttackRange = 5f;

        bool rangedAttackIsReady;

        public override void MPStart() {
            base.MPStart();
            shooter.Init();
            attackAction.Init();
        }

        public override void Create(Vector3 pos, Quaternion rot) {
            base.Create(pos, rot);

            StartMove();
        }

        protected override void StartMove() {
            base.StartMove();

            myAnimator.SetBool(animWalk, true);
            StartCoroutine(nameof(CoCheckDistance), attackAction);
            Invoke(nameof(RangeAttackReady), attackInterval);
        }

        IEnumerator CoCheckDistance() {
            while (true) {
                float distance = Vector3.Distance(myTransform.position, heroTransform.position);

                if (distance <= attackRange) {
                    StartCoroutine(nameof(CoMeleeAttack), attackAction);
                    yield break;
                } else if (rangedAttackIsReady && distance > rangedAttackRange) {
                    StartCoroutine(nameof(CoRangedAttack), attackAction);
                    yield break;
                }

                yield return null;
            }
        }

        void RangeAttackReady() {
            rangedAttackIsReady = true;
        }

        IEnumerator CoMeleeAttack(AttackAction action) {
            aiPath.canMove = false;
            myAnimator.SetTrigger(action.Anim);
            yield return new WaitForSeconds(action.PreDelay);

            AttackHero();
            yield return new WaitForSeconds(action.PostDelay);

            aiPath.canMove = true;
            StartCoroutine(nameof(CoCheckDistance), action);
        }

        IEnumerator CoRangedAttack(AttackAction action) {
            rangedAttackIsReady = false;
            aiPath.canMove = false;
            myAnimator.SetTrigger(action.Anim);
            yield return new WaitForSeconds(action.PreDelay);

            shooter.Shoot();
            yield return new WaitForSeconds(action.PostDelay);

            aiPath.canMove = true;
            StartCoroutine(nameof(CoCheckDistance), action);
            Invoke(nameof(RangeAttackReady), attackInterval);
        }

        protected void AttackHero() {
            float distance = Vector3.Distance(myTransform.position, heroTransform.position);

            if (distance <= attackRange) {
                hero.TakeDamage(power);
            }
        }
    }
}

