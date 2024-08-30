using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;
using UnityEngine.Events;

namespace HornSpirit {
    public class Boss : Enemy {
        [Header("Parameter")]
        [SerializeField] ParticleSystem cloudEffect;
        [SerializeField] AttackPattern[] attackPatterns;
        [SerializeField] int attack_01_Count = 3;
        [SerializeField] int attack_03_Count = 5;
        [SerializeField] float attack_03_attackInterval = 0.3f;
        [SerializeField] int attack_06_Count = 10;
        [SerializeField] float attack_06_attackInterval = 0.15f;
        [SerializeField] float attack_02_angleRate = 0.1f;
        [SerializeField] float attack_04_angleRate = 0.2f;
        [SerializeField] string[] attackSFXNames;

        [Header("Phase")]
        [SerializeField] string furyAnimationName = "Fury";
        [SerializeField] float furyAnimationTime = 2.367f;
        [SerializeField] ParticleSystem furyEffect;
        [SerializeField] float furyEffectDelay = 0.4f;

        List<UnityAction> patternActions = new();
        int currentPatternIndex = 0;

        WeaponManager weaponManager;
        Coroutine attackCoroutine;

        bool doNextPhase;
        bool nextPhaseIsOn;

        List<Enemy> spawnedMonsters = new();

        public override void MPStart() {
            base.MPStart();
            weaponManager = GameManager.Instance.WeaponManager;

            foreach (AttackPattern attackPattern in attackPatterns) {
                attackPattern.Init(myAnimator);
            }

            InitPatterns();
        }

        void InitPatterns() {
            patternActions.Add(Attack_01);
            patternActions.Add(Attack_02);
            patternActions.Add(Attack_03);
        }

        protected override void StartMove() {
            base.StartMove();

            // for prototype
            myAnimator.SetBool("Walk", true);
            ResetHealth();
            gameManager.ShowBossHealthSlider(health, maxHealth);
            RunRandomPattern();
        }

        public override void Create(Vector3 pos, Quaternion rot) {
            myTransform.SetPositionAndRotation(pos, rot);

            ResetHealth();

            gameManager.ShowBossHealthSlider(health, maxHealth);
            // gameManager.AddTargetToGroup(myTransform, 1, 3);
        }

        void RandomAttack() {
            int randomIndex = Random.Range(0, attackPatterns.Length);
            attackPatterns[randomIndex].Attack(RandomAttack);
        }

        void RunRandomPattern() {
            int randomIndex = Random.Range(0, patternActions.Count);

            if (randomIndex == currentPatternIndex) {
                currentPatternIndex = (currentPatternIndex + 1) % patternActions.Count;
            } else {
                currentPatternIndex = randomIndex;
            }

            patternActions[currentPatternIndex]();
        }

        void ChangePhase() {
            patternActions.Clear();
            patternActions.Add(Attack_04);
            patternActions.Add(Attack_05);
            patternActions.Add(Attack_06);

            attack_02_angleRate = attack_04_angleRate;

            StartCoroutine(nameof(CoFury));
        }

        IEnumerator CoFury() {
            myAnimator.SetTrigger(furyAnimationName);
            aiPath.canMove = false;
            yield return new WaitForSeconds(furyEffectDelay);
            furyEffect.Play();

            yield return new WaitForSeconds(furyAnimationTime - furyEffectDelay);
            aiPath.canMove = true;
            doNextPhase = false;
            nextPhaseIsOn = true;
            SpawnMonsters();

            RunRandomPattern();
        }

        protected override void Dead() {
            DestroyMonsters();

            Effector.PlayEffect(deadEffectName, myTransform.position + deadEffectOffset);
            MasterAudio.PlaySound(deadSFXName);
            TimeManager.Instance.DoSlowmotion(2.5f);
            gameManager.HideBossHealthSlider();

            hero.DestroyPetOwl(1.5f);

            RelicItem relicItem = PoolManager.Instance.NewItem<RelicItem>("Object", "RelicItem");
            if (relicItem) {
                relicItem.Create(myTransform.position, Quaternion.identity);
            }
        }

        public override void TakeDamage(int damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false) {
            if (dead || doNextPhase) {
                return;
            }

            if (weaponManager.ExtraBossDamage) {
                damage = Mathf.CeilToInt(damage * (1 + weaponManager.ExtraBossDamageValue / 100));
            }

            base.TakeDamage(damage, obstacleDamage, isCritical, showLabel);
            CheckPhase();

            if (dead) {
                Dead();
                MyDestroy();
                return;
            }

            gameManager.UpdateBossHealthSlider(health);
        }

        void CheckPhase() {
            if (nextPhaseIsOn) {
                return;
            }

            if (health <= 0.5f * maxHealth) {
                doNextPhase = true;

                if (attackCoroutine != null) {
                    StopCoroutine(attackCoroutine);
                }

                ChangePhase();
            }
        }

        void SpawnMonsters() {
            int spawnCount = 4;
            Vector3[] positions = new Vector3[] {
                new(3, 0, 0),
                new(0, 0, -3),
                new(0, 0, 3),
                new(-3, 0, 0),
            };

            for (int i = 0; i < spawnCount; i++) {
                Enemy enemy = PoolManager.Instance.NewItem<Enemy>("Enemy", "InfectedKnight");
                if (enemy) {
                    enemy.Create(myTransform.position + positions[i], Quaternion.identity);
                    spawnedMonsters.Add(enemy);
                }
            }
        }

        void DestroyMonsters() {
            foreach (Enemy enemy in spawnedMonsters) {
                if (enemy.IsDead == false) {
                    enemy.ForceDead();
                }
            }

            spawnedMonsters.Clear();
        }

        void Attack_01() {
            attackCoroutine = StartCoroutine(CoAttack_01(attackPatterns[0]));
        }

        void Attack_02() {
            attackCoroutine = StartCoroutine(CoAttack_02(attackPatterns[1]));
        }

        void Attack_03() {
            attackCoroutine = StartCoroutine(CoAttack_03(attackPatterns[2]));
        }

        void Attack_04() {
            attackCoroutine = StartCoroutine(CoAttack_04(attackPatterns[3]));
        }

        void Attack_05() {
            attackCoroutine = StartCoroutine(CoAttack_02(attackPatterns[4]));
        }

        void Attack_06() {
            attackCoroutine = StartCoroutine(CoAttack_06(attackPatterns[5]));
        }

        IEnumerator CoAttack_01(AttackPattern pattern) {
            AttackAction action = pattern.AttackAction;

            yield return new WaitForSeconds(pattern.AttackDelay);

            for (int i = 0; i < attack_01_Count; i++) {
                aiPath.canMove = false;
                StartCoroutine(nameof(CoLookTarget));

                myAnimator.SetTrigger("Attack01");
                yield return new WaitForSeconds(action.PreDelay);

                pattern.Shooter.Shoot();
                MasterAudio.PlaySound(attackSFXNames[0]);

                yield return new WaitForSeconds(action.PostDelay);
                StopCoroutine(nameof(CoLookTarget));
                aiPath.canMove = true;

                yield return new WaitForSeconds(0.5f);
            }

            RunRandomPattern();
        }

        IEnumerator CoAttack_02(AttackPattern pattern) {
            Shooter shooter = pattern.Shooter;

            yield return new WaitForSeconds(pattern.AttackDelay);

            StartCoroutine(nameof(CoLookTarget));

            aiPath.canMove = false;
            myAnimator.SetTrigger("Attack02");

            yield return new WaitForSeconds((float)32 / 30);   // first shoot

            shooter.AngleRate = -attack_02_angleRate;
            shooter.Shoot();
            MasterAudio.PlaySound(attackSFXNames[1]);
            yield return new WaitForSeconds((float)28 / 30);    // second shoot

            shooter.AngleRate = attack_02_angleRate;
            shooter.Shoot();
            MasterAudio.PlaySound(attackSFXNames[1]);
            yield return new WaitForSeconds(1f);         // third shoot (last)

            shooter.AngleRate = -attack_02_angleRate;
            shooter.Shoot();
            MasterAudio.PlaySound(attackSFXNames[1]);
            yield return new WaitForSeconds((float)46 / 30);    // after shoot

            StopCoroutine(nameof(CoLookTarget));
            aiPath.canMove = true;

            RunRandomPattern();
        }

        IEnumerator CoAttack_03(AttackPattern pattern) {
            yield return new WaitForSeconds(pattern.AttackDelay);

            Shooter shooter = pattern.Shooter;
            AttackAction action = pattern.AttackAction;

            int circleShooterAngle = 0;

            aiPath.canMove = false;
            myAnimator.SetTrigger("Attack03");

            yield return new WaitForSeconds(action.PreDelay);

            for (int i = 0; i < attack_03_Count; i++) {
                circleShooterAngle = (circleShooterAngle + 15) % 360;
                shooter.BulletAngle = circleShooterAngle;
                shooter.Shoot();
                MasterAudio.PlaySound(attackSFXNames[2]);

                yield return new WaitForSeconds(attack_03_attackInterval);
            }

            yield return new WaitForSeconds(action.PostDelay - attack_03_attackInterval * attack_03_Count);

            aiPath.canMove = true;

            RunRandomPattern();
        }

        IEnumerator CoAttack_04(AttackPattern pattern) {
            AttackAction action = pattern.AttackAction;

            yield return new WaitForSeconds(pattern.AttackDelay);

            for (int i = 0; i < attack_01_Count; i++) {
                aiPath.canMove = false;
                StartCoroutine(nameof(CoLookTarget));

                myAnimator.SetTrigger("Attack01");
                yield return new WaitForSeconds(action.PreDelay);

                pattern.Shooter.Shoot();
                MasterAudio.PlaySound(attackSFXNames[0]);

                yield return new WaitForSeconds(action.PostDelay);
                StopCoroutine(nameof(CoLookTarget));
                aiPath.canMove = true;

                yield return new WaitForSeconds(0.25f);
            }

            RunRandomPattern();
        }

        IEnumerator CoAttack_06(AttackPattern pattern) {
            yield return new WaitForSeconds(pattern.AttackDelay);

            Shooter shooter = pattern.Shooter;
            AttackAction action = pattern.AttackAction;

            int circleShooterAngle = 0;

            aiPath.canMove = false;
            myAnimator.SetTrigger("Attack03");

            yield return new WaitForSeconds(action.PreDelay);

            for (int i = 0; i < attack_06_Count; i++) {
                circleShooterAngle = (circleShooterAngle + 15) % 360;
                shooter.BulletAngle = circleShooterAngle;
                shooter.Shoot();
                MasterAudio.PlaySound(attackSFXNames[2]);

                yield return new WaitForSeconds(attack_06_attackInterval);
            }

            yield return new WaitForSeconds(action.PostDelay - attack_06_attackInterval * attack_06_Count);

            aiPath.canMove = true;

            RunRandomPattern();
        }

        IEnumerator CoLookTarget() {
            while (true) {
                LookTarget();
                yield return null;
            }
        }

        public void ShowCloudEffect() {
            cloudEffect.Play();
        }

        public void HideCloudEffect() {
            cloudEffect.Stop();
        }
    }
}
