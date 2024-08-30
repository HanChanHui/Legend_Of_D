using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ControlFreak2;
using TMPro;
using UnityEngine.Events;

namespace HornSpirit {
    public class Hero : LivingEntity {
        const string anim_run = "IsRun";

        [Header("Parameter")]
        [SerializeField] string heroName;
        [SerializeField] float speed = 5f;
        [SerializeField] float speedWhenAttack = 2f;
        [SerializeField] float walkMultiplierWhenAttack = 1f;
        [SerializeField] float rotateSpeed = 100f;
        [SerializeField] float gravity = 9.8f;
        [SerializeField] Animator myAnimator;
        [SerializeField] int initialExp = 100;

        [Header("UI")]
        [SerializeField] Slider healthSlider;
        [SerializeField] Slider expSlider;
        [SerializeField] TextMeshProUGUI levelText;

        CharacterController controller;
        FollowingCamera followingCamera;
        WeaponManager weaponManager;
        RelicManager relicManager;

        Transform myTransform;
        Coroutine coMoveFixedUpdate;

        HeroStat myStat;
        Weapon myWeapon;

        int level;
        int exp;
        int maxExp;

        float speedUnit;

        int originPower;
        int forceModePower;

        UnityAction CheckHealthStatus;
        int checkHealthRate;
        int checkHealthValue;
        int checkHealthDamageRate;
        int battleFlagDamage;
        bool battleFlagIsOn;

        PetOwl petOwl;
        public bool UsePetOwl { get; set; }
        public HeroStat Stat { get => myStat; }

        float defaultWalkMultiplier;
        float currentSpeed;

        public void Init() {
            myTransform = transform;
            controller = GetComponent<CharacterController>();
            weaponManager = GameManager.Instance.WeaponManager;
            relicManager = GameManager.Instance.RelicManager;

            // StartMove();

            followingCamera = GameManager.Instance.FollowingCamera;
            followingCamera.Init();
            followingCamera.StartFollowing(myTransform);

            speedUnit = speed / 100;
            InitStat();
            SetWeapon(weaponManager.EquippedWeapon);

            if (healthSlider != null) {
                healthSlider.maxValue = health;
                healthSlider.value = health;
            }

            if (expSlider != null) {
                expSlider.maxValue = 100;
                expSlider.value = 0;
            }

            exp = 0;
            maxExp = initialExp;
            levelText.text = "0";

            defaultWalkMultiplier = myAnimator.GetFloat("WalkSpeed");
            currentSpeed = speed;
        }

        void InitStat() {
            StatManager statManager = GameManager.Instance.StatManager;
            myStat = statManager.GetHeroStat(heroName);
            if (myStat != null) {
                SetHealth(myStat.Health);
            }
        }

        void ApplyStat() {
            if (myStat != null) {

            }
        }

        void MoveController() {
            Vector3 move = new(CF2Input.GetAxisRaw("Horizontal"), 0, CF2Input.GetAxisRaw("Vertical"));

            SetAnimation(move);

            move.Normalize();
            move = Quaternion.Euler(0, 45, 0) * move;

            if (controller.isGrounded && move.y < 0) {
                move.y = 0;
            } else {
                move.y -= gravity * Time.deltaTime;
            }

            controller.Move(currentSpeed * Time.deltaTime * move);
        }

        void RotateController() {
            Vector3 move = new(CF2Input.GetAxisRaw("Mouse X"), 0, CF2Input.GetAxisRaw("Mouse Y"));
            move = Quaternion.Euler(0, 45, 0) * move;

            if (move != Vector3.zero) {
                Quaternion lookRotate = Quaternion.LookRotation(move);

                myTransform.rotation = Quaternion.Lerp(myTransform.rotation,
                        lookRotate, Time.deltaTime * rotateSpeed);
            }
        }

        void SetAnimation(Vector3 move) {
            if (move == Vector3.zero) {
                myAnimator.SetBool(anim_run, false);
            } else {
                myAnimator.SetBool(anim_run, true);
            }
        }

        IEnumerator CoMoveFixedUpdate() {
            while (true) {
                MoveController();
                RotateController();
                yield return new WaitForFixedUpdate();
            }
        }

        public void Attack() {
            myWeapon.Attack();
        }

        public void AttackPress() {
            currentSpeed = speedWhenAttack;
            myAnimator.SetFloat("WalkSpeed", walkMultiplierWhenAttack);
            // myWeapon.StartAttack();
        }

        public void AttackRelease() {
            currentSpeed = speed;
            myAnimator.SetFloat("WalkSpeed", defaultWalkMultiplier);
            // myWeapon.StopAttack();
        }

        public void StopMove() {
            myAnimator.SetBool(anim_run, false);

            if (coMoveFixedUpdate != null) {
                StopCoroutine(coMoveFixedUpdate);
            }
        }

        public void StartMove() {
            coMoveFixedUpdate = StartCoroutine(nameof(CoMoveFixedUpdate));
        }

        public override void TakeDamage(int damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false) {
            if (dead) {
                return;
            }

            if (myStat.EvasionRate > 0) {
                if (Random.Range(0, 100) < myStat.EvasionRate) {
                    Debug.Log("Evasion!");
                    return;
                }
            }

            base.TakeDamage(damage, obstacleDamage, isCritical, showLabel);

            healthSlider.value = health;
            CheckHealthStatus?.Invoke();

            if (dead) {
                Dead();
            }
        }

        void Dead() {
            StopMove();
            myAnimator.SetTrigger("Dead");
            GameManager.Instance.ShowDeadPanel();
        }

        public void UpdateLevelUI(int level) {
            levelText.text = level.ToString();
        }

        public void AddExp(int value) {
            exp += value;

            if (exp >= maxExp) {
                exp -= maxExp;
                maxExp += 50;       // temp
                expSlider.maxValue = maxExp;

                level++;
                UpdateLevelUI(level);

                weaponManager.ShowDeck();
            }

            expSlider.value = exp;
        }

        public void EnableBattleFlag(RelicMeta meta) {
            checkHealthRate = meta.Values[0];
            checkHealthDamageRate = meta.Values[1];

            // TODO: consider "checkHealthValue" when update health value
            checkHealthValue = health * checkHealthRate / 100;

            // TODO: check various damage case
            CheckHealthStatus += () => {
                if (!battleFlagIsOn && health <= checkHealthValue) {
                    battleFlagIsOn = true;
                    // battleFlagDamage = shooter.CalculatedDamage * checkHealthDamageRate / 100;
                    // shooter.CalculatedDamage += battleFlagDamage;
                } else {
                    battleFlagIsOn = false;
                    // shooter.CalculatedDamage -= battleFlagDamage;
                }
            };
        }

        public void EnableForceMode(float duration, int rate) {
            StartForceMode(rate);
            Invoke(nameof(StopForceMode), duration);
        }

        void StartForceMode(int rate) {
            // originPower = shooter.CalculatedDamage;
            // forceModePower = originPower + (originPower * rate / 100);
            // shooter.CalculatedDamage = forceModePower;
        }

        void StopForceMode() {
            // shooter.CalculatedDamage = originPower;
        }

        public void SpawnPet() {
            if (UsePetOwl) {
                petOwl = PoolManager.Instance.NewItem<PetOwl>("Object", "PetOwl");
                if (petOwl) {
                    petOwl.Create(0, 10);
                }
            }
        }

        public void DestroyPet() {
            if (UsePetOwl) {
                petOwl.HideAndDestory();
            }
        }

        public void DestroyPetOwl(float delay) {
            Invoke(nameof(DestroyPet), delay);
        }

        public void SetWeapon(Weapon weapon) {
            myWeapon = weapon;
            myWeapon.SetStat(myStat);
        }

        public void AddPowerByRate(int rate) {
            int addPower = Mathf.CeilToInt(myWeapon.Power * (float)rate / 100);
            myWeapon.SetPower(myWeapon.Power + addPower);
        }

        public void AddMaxHealth(int value) {
            myStat.Health += value;
            maxHealth = myStat.Health;
            health += value;
            if (health > maxHealth) {
                health = maxHealth;
            }

            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

        public void AddHealth(int value) {
            health += value;
            if (health > maxHealth) {
                health = maxHealth;
            }

            healthSlider.value = health;
        }

        public void AddHeroSpeedByRate(int rate) {
            speed += speedUnit * rate;
            float animation = myAnimator.GetFloat("WalkSpeed");
            animation += 0.01f * rate;

            myAnimator.SetFloat("WalkSpeed", animation);
        }

        public void AddEvasionRate(int value) {
            myStat.EvasionRate += value;
        }

        public void AddRPMbyRate(int rate) {
            myWeapon.AddRPMbyRate(rate);
        }

        public void AddCriticalRate(int value) {
            myStat.CriticalRate += value;
        }

        public void AddCriticalDamageByRate(int value) {
            int addPower = Mathf.CeilToInt(myStat.CriticalPower * (float)value / 100);
            myStat.CriticalPower += addPower;
        }
    }
}
