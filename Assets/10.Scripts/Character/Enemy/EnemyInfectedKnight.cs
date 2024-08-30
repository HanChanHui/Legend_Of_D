using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class EnemyInfectedKnight : Enemy {
        [Header("Parameter")]
        [SerializeField] int power;
        [SerializeField] AttackAction attackAction;
        [SerializeField] string animWalk = "IsWalk";
        [SerializeField] float walkMultiplier = 2f;
        [SerializeField] float runSpeed = 4f;
        [SerializeField] float runDuration = 3f;

        float defaultSpeed;
        float defaultWalkMultiplier;

        public override void MPStart() {
            base.MPStart();
            attackAction.Init();

            defaultSpeed = aiPath.maxSpeed;
            defaultWalkMultiplier = myAnimator.GetFloat("WalkSpeed");
        }

        public override void Create(Vector3 pos, Quaternion rot) {
            base.Create(pos, rot);

            StartMove();
        }

        protected override void StartMove() {
            base.StartMove();

            myAnimator.SetBool(animWalk, true);
            StartCoroutine(nameof(CoCheckDistance));
            StartCoroutine(nameof(CoRunLoop));
        }

        IEnumerator CoRunLoop() {
            while (true) {
                yield return new WaitForSeconds(runDuration);
                myAnimator.SetFloat("WalkSpeed", walkMultiplier);
                aiPath.maxSpeed = runSpeed;

                yield return new WaitForSeconds(runDuration);
                myAnimator.SetFloat("WalkSpeed", defaultWalkMultiplier);
                aiPath.maxSpeed = defaultSpeed;
            }
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
            aiPath.canMove = false;
            yield return new WaitForSeconds(action.PreDelay);

            AttackHero();
            yield return new WaitForSeconds(action.PostDelay);

            aiPath.canMove = true;
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
