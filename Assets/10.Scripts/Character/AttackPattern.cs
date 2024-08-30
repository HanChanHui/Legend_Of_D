using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HornSpirit {
    [RequireComponent(typeof(Shooter))]
    public class AttackPattern : MonoBehaviour {
        [SerializeField] string animationName;
        [SerializeField] AttackAction attackAction;
        [SerializeField] float attackDelay;

        Shooter shooter;
        Animator animator;

        public AttackAction AttackAction { get => attackAction; }
        public float AttackDelay { get => attackDelay; }
        public Shooter Shooter { get => shooter; }

        public void Init(Animator animator) {
            this.animator = animator;

            shooter = GetComponent<Shooter>();
            shooter.Init();
            attackAction.Init();
        }

        public void Attack(UnityAction action = null) {
            StartCoroutine(CoAttack(attackAction, action));
        }

        IEnumerator CoAttack(AttackAction attack, UnityAction action) {
            animator.SetTrigger(attack.Anim);
            yield return new WaitForSeconds(attack.PreDelay);

            shooter.Shoot();
            yield return new WaitForSeconds(attack.PostDelay + attackDelay);

            action?.Invoke();
        }
    }
}
