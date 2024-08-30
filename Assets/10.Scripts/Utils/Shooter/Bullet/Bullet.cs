using System.Collections;
using System.Collections.Generic;
using HornSpirit;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour, IMemoryPool {
    [Header("Basic")]
    [SerializeField] protected string mpType;
    [SerializeField] protected string mpGroup = "Bullet";
    [SerializeField] protected string hitEffect;
    [SerializeField] protected string muzzleFlash;
    [SerializeField] protected float radius = 0.5f;
    [SerializeField] protected bool rangeAttack = false;
    [SerializeField] protected float range = 5f;
    [SerializeField] protected Transform bulletCore;
    [SerializeField] protected Vector3 rotateOffset;
    [SerializeField] protected int obstacleDamage = 1;
    public LayerMask colliderMask;
    public LayerMask enemyColliderMask;
    protected int power;
    protected float speed;
    protected float speedRate;
    protected float angle;
    protected float angleRate;
    public string MPType { get { return mpType; } set { mpType = value; } }
    public string MPGroup { get { return mpGroup; } set { mpGroup = value; } }
    public bool RangeAttack { get { return rangeAttack; } set { rangeAttack = value; } }
    public float Range { get { return range; } set { range = value; } }
    protected ParticleSystem[] particleSystems;

    public float lifeTime = 3.0f;
    protected bool isPlaced;
    protected float moveTime;
    protected float stopTime;
    protected float placedStopSpeed;
    protected float originSpeed;

    // GameManager gameManager;
    protected Transform myTransform;
    protected bool isCritical;
    protected bool isDead;
    public bool CheckOutBound { get; set; }

    UnityAction<string> ObjectDisappear;
    protected delegate void DelHitObject(Collider c, Vector3 hitPoint);
    protected DelHitObject OnHitObject;

    protected UnityAction CheckBulletCollision;
    event UnityAction<Constants.EnemyType> EnemyExtraDamageAction;
    bool isPenetration;
    int penetrationCount;

    WeaponManager weaponManager;
    float bossExtraDamageRate;
    bool extraBossDamage;

    public virtual void MPStart() {
        this.MyStart();
    }

    protected virtual void MyStart() {
        weaponManager = GameManager.Instance.WeaponManager;
        myTransform = transform;
        ObjectDisappear = (string tag) => {
            Disappear();
        };

        OnHitObject = DefaultHitObject;

        if (RangeAttack) {
            CheckBulletCollision = CheckRangeCollision;
        } else {
            CheckBulletCollision = CheckCollisions;
        }

        // particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    public void InitPlaced(float moveTime, float stopTime, float placedStopSpeed) {
        isPlaced = true;
        this.moveTime = moveTime;
        this.stopTime = stopTime;
        this.placedStopSpeed = placedStopSpeed;
    }

    public void RotateBulletCore(Vector3 rot) {
        bulletCore.rotation = Quaternion.Euler(myTransform.rotation.eulerAngles + rot + rotateOffset);
    }

    public virtual void Create(Vector3 pos, Quaternion rot, int power, float speed, float speedRate,
            float angle, float angleRate, bool isCritical = false, float startDistance = 0, float lifeTime = 0) {
        myTransform.SetPositionAndRotation(pos, rot);
        this.isCritical = isCritical;
        this.power = power;
        this.speed = speed;
        this.speedRate = speedRate;
        this.angle = angle;
        this.angleRate = angleRate;

        isDead = false;
        Effector.PlayEffect("MuzzleFlash", muzzleFlash, pos, rot);

        if (startDistance > 0) {
            myTransform.Translate(Vector3.Normalize(Quaternion.AngleAxis(angle, Vector3.up)
                    * Vector3.forward) * startDistance);
        }

        Collider[] initialCollisions = Physics.OverlapSphere(myTransform.position, 0.1f,
                                     colliderMask);

        if (initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0], myTransform.position);
        }

        if (gameObject.activeSelf) {
            MoveStart();

            if (lifeTime > 0) {
                Invoke(nameof(Disappear), lifeTime);
            }
        }
    }

    protected virtual void MoveStart() {
        StartCoroutine(nameof(CoUpdate));

        if (isPlaced) {
            StartCoroutine(nameof(CoPlaced));
        }
    }

    IEnumerator CoUpdate() {
        while (true) {
            Move();

            yield return null;
        }
    }

    IEnumerator CoPlaced() {
        originSpeed = speed;
        yield return new WaitForSeconds(moveTime);
        speed = placedStopSpeed;
        yield return new WaitForSeconds(stopTime);
        speed = originSpeed;
    }

    protected virtual void Move() {
        float moveDistance = speed * Time.deltaTime;

        CheckBulletCollision();
        myTransform.Translate(Vector3.Normalize(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward) * moveDistance);

        speed += speedRate;
        angle += angleRate;
    }

    protected void CheckRangeCollision() {
        Collider[] hitColliders = Physics.OverlapSphere(myTransform.position, radius, colliderMask, QueryTriggerInteraction.Collide);
        if (hitColliders.Length > 0) {
            Collider[] colliders = Physics.OverlapSphere(myTransform.position, range, enemyColliderMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < colliders.Length; i++) {
                OnHitObject(colliders[i], myTransform.position);
            }

            if (colliders.Length == 0) {
                Disappear();
            }
        }
    }

    protected void CheckCollisions() {
        Collider[] hitColliders = Physics.OverlapSphere(myTransform.position, radius, colliderMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hitColliders.Length; i++) {
            OnHitObject(hitColliders[i], myTransform.position);
        }
    }

    public virtual void DefaultHitObject(Collider c, Vector3 hitPoint) {
        if (c.TryGetComponent<LivingEntity>(out var entity)) {
            entity.TakeDamage(c, hitPoint, power, obstacleDamage, isCritical, true);
        }

        ObjectDisappear(c.tag);
    }

    public void Stop() {
        if (isDead) {
            return;
        }

        Disappear();
    }

    protected virtual void Disappear() {
        Effector.PlayEffect("Explosion", hitEffect, myTransform.position);
        MyDestroy();
    }

    protected virtual void PenetrationDisappear(string tag) {
        if (tag.Equals("Enemy") == false) {
            if (penetrationCount > 0) {
                penetrationCount--;
            } else {
                Disappear();
            }
        }
    }

    protected virtual void MyDestroy() {
        isDead = true;
        isPlaced = false;
        ObjectDisappear = (string tag) => {
            Disappear();
        };

        CancelInvoke();
        StopAllCoroutines();
        // ResetParticle();
        PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
    }

    public void EnableRangeAttack(bool enable, float range) {
        RangeAttack = enable;
        this.range = range;

        if (enable) {
            CheckBulletCollision = CheckRangeCollision;
        } else {
            CheckBulletCollision = CheckCollisions;
        }
    }

    public void SetPenetration(bool enable, int count) {
        isPenetration = enable;
        penetrationCount = count;

        ObjectDisappear = (string tag) => {
            if (tag.Equals("Enemy")) {
                if (penetrationCount > 0) {
                    penetrationCount--;
                } else {
                    Disappear();
                }
            } else {
                Disappear();
            }
        };
    }

    // void ResetParticle() {
    //     foreach (ParticleSystem particle in particleSystems) {
    //         particle.Clear();
    //     }
    // }
}
