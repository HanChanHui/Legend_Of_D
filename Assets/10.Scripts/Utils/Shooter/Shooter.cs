using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HornSpirit;
using DarkTonic.MasterAudio;

public class Shooter : MonoBehaviour {
    public enum ShootingType {
        None,
        Directional,
        Nway,
        Circle,
        RandomNway,
        RandomCircle,
        RollingNway,
        WavingNway,
        CircleWavingNway,
        Spreading,
        RandomSpreading,
        Overtaking,
        Multiple,
        Homing,
        DelayHoming,
        RandomHoming,
        Placed,
        Aiming,
        Cross,
        RandomDirectional,
        Arc,
        CustomShape,
    };

    [Header("Basic")]
    [SerializeField] ShootingType shootingType;
    [SerializeField] Transform muzzle;
    [SerializeField] bool showMuzzleFlash;
    [SerializeField] bool saveBullet;

    [Header("Bullet")]
    [SerializeField] string bulletGroup = "Bullet";
    [SerializeField] string bulletType;
    [SerializeField] string muzzleFlashEffect;
    [SerializeField] int power;
    [SerializeField] float speed;
    [SerializeField] float speedRate;
    [SerializeField] float angle;
    [SerializeField] float angleRate;
    [SerializeField] float startDistance;
    [SerializeField] float lifeTime;
    public string BulletType { get { return bulletType; } set { bulletType = value; } }

    [Header("Setting")]
    [SerializeField] bool enableExtraPain;
    [SerializeField] int extraPainCount;
    [SerializeField] int extraPainPower;
    [SerializeField] int extraPainRate;
    [SerializeField] bool enableExtraBomb;
    [SerializeField] int extraBombPower;
    [SerializeField] int extraBombRate;

    [Header("Sound")]
    [SerializeField] bool enableShootSfx;
    [SerializeField] string shootSfx;

    [Header("Custom Shape")]
    [SerializeField] float rot = 0f;
    [SerializeField, Range(3, 7)] int vertex = 3;
    [SerializeField, Range(1, 5)] float sup = 3;
    [SerializeField] bool rotateBulletCore = false;
    [SerializeField] Vector3 rotateBulletOffset;
    int m;
    float a;
    float phi;
    List<float> cs_v = new List<float>();
    List<float> cs_xx = new List<float>();

    Quaternion muzzleRot;

    bool isCritical;
    int currPower;
    public int CurrPower { set { this.currPower = value; } get { return this.currPower; } }
    public float BulletSpeed { set { this.speed = value; } get { return this.speed; } }
    public int BulletCount { get { return this.count; } set { this.count = value; } }
    public float BulletAngle { get { return this.angle; } set { this.angle = value; } }
    public float NwayAngle { get { return this.angleRange; } set { this.angleRange = value; } }
    public bool EnableExtraPain { get { return enableExtraPain; } set { enableExtraPain = value; } }
    public int ExtraPainCount { get { return extraPainCount; } set { extraPainCount = value; } }
    public int ExtraPainPower { get { return extraPainPower; } set { extraPainPower = value; } }
    public int ExtraPainRate { get { return extraPainRate; } set { extraPainRate = value; } }
    public bool EnableExtraBomb { get { return enableExtraBomb; } set { enableExtraBomb = value; } }
    public int ExtraBombPower { get { return extraBombPower; } set { extraBombPower = value; } }
    public int ExtraBombRate { get { return extraBombRate; } set { extraBombRate = value; } }
    public float AngleRate { get { return angleRate; } set { angleRate = value; } }
    public int Vertex { get { return vertex; } set { vertex = value; } }
    public float Sup { get { return sup; } set { sup = value; } }
    public float Rot { get { return rot; } set { rot = value; } }

    public float LifeTime { get { return lifeTime; } set { lifeTime = value; } }

    int prevPower;
    List<Bullet> bulletList = new();

    [Header("Nway")]
    [SerializeField] float angleRange;
    [SerializeField] int count;

    [Header("Multiple")]
    [SerializeField] float multipleStep;
    [SerializeField] float multipleRange;

    [Header("Homing")]
    [SerializeField] float turnSpeed;
    [SerializeField] bool decreaseHomingSpeed;
    [SerializeField] float homingSpeedRate;
    [SerializeField] float delayTime;

    [Header("Rolling N-way")]
    [SerializeField] int nWayCount;

    [Header("Waving N-way")]
    [SerializeField] float wavingAngleRange;
    [SerializeField] int cycle;

    [Header("Placed")]
    [SerializeField] float moveTime;
    [SerializeField] float stopTime;
    [SerializeField] float placedStopSpeed;

    [Header("Aiming")]
    [SerializeField] Transform targetTransform;

    [Header("Spreading")]
    [SerializeField] float groupSpeed;
    [SerializeField] float groupCount;
    [SerializeField] float speedRange;

    [Header("Overtaking")]
    [SerializeField] float groupAngle;
    [SerializeField] float groupInterval;

    [Header("Arc")]
    [SerializeField] float arrivalTime;
    [SerializeField] float height;

    delegate void ShootFunc();
    ShootFunc shootFunc;
    bool isInitialized;

    int isCalculatedDamage;
    public int CalculatedDamage { get { return isCalculatedDamage; } set { isCalculatedDamage = value; } }
    public bool CheckOutBound { get; set; }
    bool penetration;
    int penetrationCount;

    public void Init(Transform muzzle = null, Transform target = null) {
        if (isInitialized) {
            return;
        }

        isCalculatedDamage = power;

        if (target) {
            this.targetTransform = target;
        }

        if (muzzle) {
            SetMuzzle(muzzle);
        }

        SetType(this.shootingType);

        isInitialized = true;

        if (shootingType == ShootingType.CustomShape) {
            InitCustomShape();
        }
    }

    public void SetMuzzle(Transform muzzle) {
        this.muzzle = muzzle;
    }

    public void SetType(ShootingType type) {
        this.shootingType = type;
        shootFunc = GetShootFunc(type);
    }

    public void ResetDamage() {
        currPower = power;
    }

    public void PowerUp(int rate) {
        prevPower = currPower;
        currPower = (int)(currPower * rate / 100);
    }

    public void EndPowerUP() {
        currPower = prevPower;
    }

    public void SetPower(int value) {
        currPower = value;
    }

    public void LevelupBulletPower(int rate, bool duringPowerup) {
        if (duringPowerup) {
            prevPower = (int)(prevPower * rate / 100);
        } else {
            PowerUp(rate);
        }
    }

    public void Shoot() {
        if (enableShootSfx) {
            MasterAudio.PlaySound(shootSfx);
        }

        if (showMuzzleFlash) {
            // Effector.PlayEffect(muzzleFlashEffect, muzzle.position, muzzle.rotation);
        }

        shootFunc();
    }

    // temp
    public void Shoot(int damage, bool isCritical) {
        isCalculatedDamage = damage;
        this.isCritical = isCritical;

        if (enableShootSfx) {
            MasterAudio.PlaySound(shootSfx);
        }

        shootFunc();
    }

    void BasicShoot(float speed, float speedRate, float angle, float angleRate, Vector3 pos) {
        Bullet bullet = PoolManager.Instance.NewItem<Bullet>(bulletGroup, bulletType);
        if (bullet) {
            if (penetration) {
                bullet.SetPenetration(true, penetrationCount);
            }

            bullet.CheckOutBound = this.CheckOutBound;
            bullet.Create(pos, muzzle.rotation, isCalculatedDamage, speed,
                    speedRate, angle, angleRate, isCritical, startDistance, lifeTime);

            if (rotateBulletCore) {
                var offset = rotateBulletOffset + new Vector3(0, angle, 0);
                bullet.RotateBulletCore(offset);
            }

            if (saveBullet) {
                bulletList.Add(bullet);
            }
        }
    }

    void BasicShoot() {
        BasicShoot(speed, speedRate, angle, angleRate, muzzle.position);
    }

    void NwayShoot(float speed, float speedRate, float angle, float angleRate, float angleRange, int count) {
        if (count > 1) {
            for (int i = 0; i < count; i++) {
                BasicShoot(speed, speedRate, angle + angleRange * ((float)i / (count - 1) - 0.5f), angleRate, muzzle.position);
            }
        } else {
            BasicShoot(speed, speedRate, angle, angleRate, muzzle.position);
        }
    }

    void NwayShoot() {
        NwayShoot(speed, speedRate, angle, angleRate, angleRange, count);
    }

    void CircleShoot() {
        NwayShoot(speed, speedRate, angle, angleRate, angleRange - angleRange / count, count);
    }

    void RandomNwayShoot() {
        for (int i = 0; i < count; i++) {
            BasicShoot(speed, speedRate, angle + angleRange * (Random.Range(0, 1.0f) - 0.5f), angleRate, muzzle.position);
        }
    }

    void RandomCircleShoot() {
        for (int i = 0; i < count; i++) {
            BasicShoot(speed, speedRate, Random.Range(0, 360f), angleRate, muzzle.position);
        }
    }

    void MultipleShoot() {
        if (count > 1) {
            for (int i = 0; i < count; i++) {
                Vector3 pos = muzzle.position;
                pos.x += multipleRange * ((float)i / (count - 1) - 0.5f);
                pos = RotatePointAroundPivot(pos, muzzle.position, muzzle.rotation.eulerAngles);

                BasicShoot(speed, speedRate, angle, angleRate, pos);
            }
        } else {
            BasicShoot();
        }
    }

    void HomingShoot() {
        // HomingBullet bullet = PoolManager.Instance.NewItem<HomingBullet>(bulletGroup, bulletType);
        // if (bullet) {
        //     bullet.Init(turnSpeed, targetTransform, homingSpeedRate, decreaseHomingSpeed);
        //     bullet.CheckOutBound = this.CheckOutBound;
        //     bullet.Create(muzzle.position, muzzle.rotation, isCalculatedDamage,
        //             speed, speedRate, angle, angleRate, isCritical, startDistance, lifeTime);

        //     if (saveBullet) {
        //         bulletList.Add(bullet);
        //     }
        // }
    }

    void DelayHomingShoot() {
        // DelayHomingBullet bullet = PoolManager.Instance.NewItem<DelayHomingBullet>(bulletGroup, bulletType);
        // if (bullet) {
        //     bullet.Init(turnSpeed, delayTime);
        //     bullet.Create(muzzle.position, muzzle.rotation, isCalculatedDamage,
        //             speed, speedRate, angle, angleRate, isCritical, startDistance, lifeTime);
        // }
    }

    void RandomHomingShoot() {
        // HomingBullet bullet = PoolManager.Instance.NewItem<HomingBullet>(bulletGroup, bulletType);
        // if (bullet) {
        //     bullet.Init(turnSpeed, null, homingSpeedRate, decreaseHomingSpeed);
        //     bullet.CheckOutBound = this.CheckOutBound;
        //     bullet.Create(muzzle.position, muzzle.rotation, isCalculatedDamage,
        //             speed, speedRate, angle + angleRange * (Random.Range(0, 1.0f) - 0.5f), angleRate, isCritical, startDistance, lifeTime);

        //     if (saveBullet) {
        //         bulletList.Add(bullet);
        //     }
        // }
    }

    void RandomDirectionalShoot() {
        BasicShoot(speed, speedRate, angle + angleRange * (Random.Range(0, 1.0f) - 0.5f), angleRate, muzzle.position);
    }

    void RollingNway() {
        for (int i = 0; i < nWayCount; i++) {
            NwayShoot(speed, speedRate, angle + (float)(i * 360) / nWayCount, angleRate, angleRange, count);
        }
    }

    void WavingNwayShoot() {
        NwayShoot(speed, speedRate, angle + wavingAngleRange * Mathf.Sin(Time.time * cycle),
                angleRate, angleRange, count);
    }

    void WavingCircleShoot() {
        NwayShoot(speed, speedRate, angle + wavingAngleRange * Mathf.Sin(Time.time * cycle),
                angleRate, 360f - 360f / count, count);
    }

    void PlacedShoot() {
        Bullet bullet = PoolManager.Instance.NewItem<Bullet>(bulletGroup, bulletType);
        if (bullet) {
            bullet.InitPlaced(moveTime, stopTime, placedStopSpeed);
            bullet.CheckOutBound = this.CheckOutBound;
            bullet.Create(muzzle.position, muzzle.rotation, isCalculatedDamage, speed,
                    speedRate, angle, angleRate, isCritical, startDistance, lifeTime);

            if (saveBullet) {
                bulletList.Add(bullet);
            }
        }
    }

    void AimingShoot() {
        float angle = AngleBetweenTransform(muzzle, targetTransform);
        BasicShoot(speed, speedRate, -angle, angleRate, muzzle.position);
    }

    void SpreadingShoot() {
        for (int i = 0; i < groupCount; i++) {
            NwayShoot(speed + groupSpeed * i, speedRate, angle, angleRate, angleRange, count);
        }
    }

    void RandomSpreadingShoot() {
        for (int i = 0; i < count; i++) {
            BasicShoot(speed + speedRange * Random.Range(0, 1.0f), 0,
                angle + angleRange * (Random.Range(0, 1.0f) - 0.5f), 0, muzzle.position);
        }
    }

    void OvertakingShoot() {
        StartCoroutine("CoOvertakingShoot");
    }

    IEnumerator CoOvertakingShoot() {
        for (int i = 0; i < groupCount; i++) {
            NwayShoot(speed + groupSpeed * i, 0, angle + groupAngle * i, 0, angleRange, count);

            yield return new WaitForSeconds(groupInterval);
        }
    }

    void CrossShoot() {
        // ShapeBullet bullet = PoolManager.Instance.NewItem<ShapeBullet>(bulletGroup, bulletType);
        // if (bullet) {
        //     bullet.Create(muzzle.position, muzzle.rotation, isCalculatedDamage,
        //             speed, isCritical);
        // }
    }

    void ArcShoot() {
        // ArcBullet bullet = PoolManager.Instance.NewItem<ArcBullet>(bulletGroup, bulletType);
        // if (bullet) {
        //     bullet.Create(muzzle.position, targetTransform.position, isCalculatedDamage, arrivalTime, height);
        // }
    }

    void CustomShapeShoot() {
        float dir = rot;

        for (int r = 0; r < vertex; r++) {
            for (int i = 1; i <= m; i++) {
                CreateCustomShapeBullet(muzzle.position, dir + cs_xx[i], cs_v[i] * speed / sup);
                CreateCustomShapeBullet(muzzle.position, dir - cs_xx[i], cs_v[i] * speed / sup);
                CreateCustomShapeBullet(muzzle.position, dir, speed);

                dir += 360 / vertex;
            }
        }
    }

    public void SetCustomShapeParameter(float rot, int vertex, float sup) {
        this.rot = rot;
        this.vertex = vertex;
        this.sup = sup;

        InitCustomShape();
    }

    void CreateCustomShapeBullet(Vector3 startPos, float rot, float speed) {
        Bullet bullet = PoolManager.Instance.NewItem<Bullet>(bulletGroup, bulletType);
        if (bullet) {
            bullet.CheckOutBound = this.CheckOutBound;

            bullet.Create(startPos, muzzle.rotation, isCalculatedDamage, speed,
                speedRate, rot, angleRate, isCritical, startDistance, lifeTime);

            if (rotateBulletCore) {
                bullet.RotateBulletCore(new Vector3(0, rot, 0));
            }
        }
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    float GetAngle(Vector3 from, Vector3 to) {
        Vector3 diff = to - from;
        return Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
    }

    float AngleBetweenTransform(Transform from, Transform to) {
        Vector3 targetDir = to.position - from.position;
        Vector3 forward = from.forward;
        return Vector3.SignedAngle(targetDir, forward, Vector3.up);
    }

    public void RemoveAllBullet() {
        if (saveBullet) {
            for (int i = 0; i < bulletList.Count; i++) {
                bulletList[i].Stop();
            }

            bulletList.Clear();
        }
    }

    public void UpdateNwayOption(float angleRange, int count) {
        this.angleRange = angleRange;
        this.count = count;
    }

    public void InitCustomShape() {
        cs_v.Clear();
        cs_xx.Clear();

        m = (int)Mathf.Floor(sup / 2);
        a = 2 * Mathf.Sin(Mathf.PI / vertex);
        phi = ((Mathf.PI / 2f) * (vertex - 2f)) / vertex;
        cs_v.Add(0);
        cs_xx.Add(0);

        for (int i = 1; i <= m; i++) {
            cs_v.Add(Mathf.Sqrt(sup * sup - 2 * a * Mathf.Cos(phi) * i * sup + a * a * i * i));
        }

        for (int i = 1; i <= m; i++) {
            cs_xx.Add(Mathf.Rad2Deg * (Mathf.Asin(a * Mathf.Sin(phi) * i / cs_v[i])));
        }
    }

    public void AddBullet() {
        switch (shootingType) {
            case ShootingType.Multiple:
                count++;
                multipleRange += multipleStep;
                break;
            case ShootingType.Nway:
                count += 2;
                break;
            case ShootingType.RandomSpreading:
                count++;
                lifeTime += 0.05f;
                break;
            case ShootingType.RandomNway:
                count++;
                break;
        }
    }

    public void SetPenetration(bool enable, int count) {
        penetration = enable;
        penetrationCount = count;
    }

    ShootFunc GetShootFunc(ShootingType type) {
        switch (type) {
            case ShootingType.Directional:
                return BasicShoot;
            case ShootingType.Nway:
                return NwayShoot;
            case ShootingType.Circle:
                return CircleShoot;
            case ShootingType.RandomNway:
                return RandomNwayShoot;
            case ShootingType.RandomCircle:
                return RandomCircleShoot;
            case ShootingType.Multiple:
                return MultipleShoot;
            case ShootingType.Homing:
                return HomingShoot;
            case ShootingType.DelayHoming:
                return DelayHomingShoot;
            case ShootingType.RandomHoming:
                return RandomHomingShoot;
            case ShootingType.RollingNway:
                return RollingNway;
            case ShootingType.WavingNway:
                return WavingNwayShoot;
            case ShootingType.CircleWavingNway:
                return WavingCircleShoot;
            case ShootingType.Placed:
                return PlacedShoot;
            case ShootingType.Aiming:
                return AimingShoot;
            case ShootingType.Spreading:
                return SpreadingShoot;
            case ShootingType.RandomSpreading:
                return RandomSpreadingShoot;
            case ShootingType.Overtaking:
                return OvertakingShoot;
            case ShootingType.Cross:
                return CrossShoot;
            case ShootingType.RandomDirectional:
                return RandomDirectionalShoot;
            case ShootingType.Arc:
                return ArcShoot;
            case ShootingType.CustomShape:
                return CustomShapeShoot;
            default:
                return null;
        }
    }
}
