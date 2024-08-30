using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class EnemyGoblinWitcher : Enemy {
        [Header("Parameter")]
        [SerializeField] Shooter shooter;
        [SerializeField] AttackAction attackAction;
        [SerializeField] string animWalk = "IsWalk";
        [SerializeField] float attackInterval = 3f;

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
            StartCoroutine(nameof(CoAttack), attackAction);
            StartCoroutine(nameof(CoCheckReachedDestination));
        }

        IEnumerator CoAttack(AttackAction action) {
            yield return new WaitForSeconds(attackInterval);

            aiPath.canMove = false;
            myAnimator.SetTrigger(action.Anim);
            yield return new WaitForSeconds(action.PreDelay);

            shooter.Shoot();
            yield return new WaitForSeconds(action.PostDelay);

            aiPath.canMove = true;
            StartCoroutine(nameof(CoAttack), action);
        }

        IEnumerator CoCheckReachedDestination() {
            while (true) {
                if (aiPath.reachedDestination) {
                    if (aiPath.canMove) {
                        aiPath.canMove = false;
                        myAnimator.SetBool(animWalk, false);
                        StartCoroutine(nameof(CoLookTarget));
                    }
                } else {
                    if (aiPath.canMove == false) {
                        aiPath.canMove = true;
                        myAnimator.SetBool(animWalk, true);
                        StopCoroutine(nameof(CoLookTarget));
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        IEnumerator CoLookTarget() {
            while (true) {
                LookTarget();
                yield return null;
            }
        }
    }
}
