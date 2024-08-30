using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using DarkTonic.MasterAudio;
using UnityEngine.Assertions.Must;
using DG.Tweening;

namespace HornSpirit {
    public class Enemy : LivingEntity, IMemoryPool {
        [Header("Basic")]
        [SerializeField] protected Constants.EnemyType enemyType;
        [SerializeField] protected string mpGroup;
        [SerializeField] protected string mpType;
        [SerializeField] protected Animator myAnimator;
        [SerializeField] protected float rotateSpeed = 10;
        [SerializeField] protected float speed = 5;
        [SerializeField] protected float attackRange = 2;
        [SerializeField] protected bool enableEnemyLabel = true;
        [SerializeField] protected Vector3 labelOffset;
        [SerializeField] protected Vector3 deadEffectOffset;
        [SerializeField] protected Vector3 spawnEffectOffset;
        [SerializeField] protected int exp = 10;
        [SerializeField] protected string soulName = "SoulCrimson";
        [SerializeField] protected int defaultSoulCount = 1;
        [SerializeField] protected float hitBlinkTime = 0.1f;

        [Header("Effect")]
        [SerializeField] protected string deadEffectName;
        [SerializeField] protected string spawnEffectName;
        [SerializeField] protected string deadSFXName;

        [Header("Prototype")]
        [SerializeField] protected bool isPrototype;

        protected Hero hero;
        protected Rigidbody myRigidbody;
        protected Transform myTransform;
        protected Transform heroTransform;
        protected CameraCtrl cameraCtrl;
        protected Room myRoom;
        protected Material myMaterial;

        // A* Pathfinding
        protected AIDestinationSetter setter;
        protected AIPath aiPath;

        protected EnemyLabel enemyLabel;
        public Constants.EnemyType EnemyType { get => enemyType; }

        protected RelicManager relicManager;
        protected GameManager gameManager;
        protected int soulCount;

        public virtual void MPStart() {
            hero = GameManager.Instance.Hero;
            heroTransform = hero.transform;
            relicManager = GameManager.Instance.RelicManager;
            gameManager = GameManager.Instance;

            myRigidbody = GetComponent<Rigidbody>();
            myTransform = transform;
            myMaterial = GetComponentInChildren<SkinnedMeshRenderer>().material;

            InitAStar();
        }

        protected void InitAStar() {
            setter = GetComponent<AIDestinationSetter>();
            aiPath = GetComponent<AIPath>();

            setter.target = heroTransform;
        }

        public virtual void Create(Vector3 pos, Quaternion rot) {
            myTransform.SetPositionAndRotation(pos, rot);

            ResetHealth();
            EnableSoulEater();

            if (!isPrototype) {
                Effector.PlayEffect(spawnEffectName, myTransform.position + spawnEffectOffset);
            }

            if (enableEnemyLabel) {
                enemyLabel = PoolManager.Instance.NewItem<EnemyLabel>("Label", "EnemyLabel");
                enemyLabel.Create(myTransform, labelOffset, maxHealth);
            }
        }

        protected virtual void StartMove() {
            if (aiPath.isStopped) {
                aiPath.isStopped = false;
            }

            aiPath.maxSpeed = speed;
        }

        public override void SetHealth(int health) {
            base.SetHealth(health);

            UpdateHealthLabel();
        }

        void UpdateHealthLabel() {
            if (enableEnemyLabel) {
                enemyLabel.UpdateLabel(health);
            }
        }

        protected void LookTarget() {
            Vector3 move = heroTransform.position - myTransform.position;
            move.y = 0;

            if (move != Vector3.zero) {
                Quaternion newRot = Quaternion.LookRotation(move);
                myTransform.rotation = Quaternion.Slerp(myTransform.rotation, newRot, Time.deltaTime * rotateSpeed);
            }
        }

        protected void LookTarget(Transform target) {
            Vector3 direction = target.position - myTransform.position;
            myTransform.rotation = Quaternion.LookRotation(direction);
        }

        protected virtual void Dead() {
            if (myRoom) {
                myRoom.DefeatEnemy();
            }

            Effector.PlayEffect(deadEffectName, myTransform.position + deadEffectOffset);
            MasterAudio.PlaySound(deadSFXName);

            SpawnSoul();
        }

        public void ForceDead() {
            Effector.PlayEffect(deadEffectName, myTransform.position + deadEffectOffset);
            MyDestroy();
        }

        public override void TakeDamage(int damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false) {
            if (dead) {
                return;
            }

            base.TakeDamage(damage, obstacleDamage, isCritical, showLabel);
            gameManager.SpawnEnemyDamageNumber(myTransform.position, damage, isCritical);

            if (dead) {
                Dead();
                if (isPrototype) {
                    gameObject.SetActive(false);
                } else {
                    MyDestroy();
                }
                return;
            }

            HitBlinkEffect();
            if (enableEnemyLabel) {
                enemyLabel.UpdateLabel(health);
            }
        }

        public void MyDestroy() {
            if (enableEnemyLabel) {
                enemyLabel.MyDestroy();
            }

            PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
        }

        public void SpawnSoul() {
            for (int i = 0; i < soulCount; i++) {
                Soul soul = PoolManager.Instance.NewItem<Soul>("Object", soulName);
                soul.Create(myTransform.position, Quaternion.identity);
            }
        }

        public void SetRoom(Room room) {
            myRoom = room;
        }

        public void EnableSoulEater() {
            if (relicManager.SoulEater) {
                int extra = defaultSoulCount * relicManager.SoulEaterValue / 100;
                soulCount = defaultSoulCount + extra;
            } else {
                soulCount = defaultSoulCount;
            }
        }

        public void StartObject() {
            gameObject.SetActive(true);
            StartMove();
        }

        public void StopObject() {
            aiPath.isStopped = true;
            gameObject.SetActive(false);
        }

        protected void HitBlinkEffect() {
            if (myMaterial == null) {
                return;
            }

            StartCoroutine(nameof(CoHitBlinkEffect));
        }

        IEnumerator CoHitBlinkEffect() {
            myMaterial.SetInt("_Hit", 1);
            yield return new WaitForSeconds(hitBlinkTime);
            myMaterial.SetInt("_Hit", 0);
        }
    }
}
