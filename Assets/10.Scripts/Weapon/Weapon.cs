using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Weapon : MonoBehaviour {
        [Header("Parameter")]
        [SerializeField] string weaponName;
        [SerializeField] int power;
        [SerializeField] int magazine = 24;
        [SerializeField] float reloadTime = 1.5f;
        [SerializeField] ParticleSystem muzzleFlash;
        [SerializeField] int rpm = 120;

        public string WeaponName { get => weaponName; }
        public int Magazine { get => magazine; }
        public float ReloadTime { get => reloadTime; }
        public int Power { get => power; }

        WeaponManager weaponManager;
        Shooter shooter;
        int currentMagazine;
        bool isReloading;
        HeroStat myStat;
        bool isCritical;
        float attackInterval;
        float nextShotTime;

        public void Init() {
            weaponManager = GameManager.Instance.WeaponManager;

            shooter = GetComponent<Shooter>();
            shooter.Init();

            currentMagazine = magazine;
            attackInterval = 60f / rpm;
        }

        public void SetMagazine(int value) {
            magazine = value;
            currentMagazine = magazine;
            weaponManager.UpdateMagazine(currentMagazine);
        }

        public void Attack() {
            if (isReloading) {
                return;
            }

            if (Time.time > nextShotTime && currentMagazine > 0) {
                nextShotTime = Time.time + attackInterval;

                shooter.Shoot(CalculatedDamage(), isCritical);
                muzzleFlash.Play();
                currentMagazine--;
                weaponManager.UpdateMagazine(currentMagazine);

                if (currentMagazine == 0) {
                    StartReload();
                }
            }
        }

        public void StartAttack() {
            StartCoroutine(nameof(CoStartAttack));
        }

        IEnumerator CoStartAttack() {
            while (true) {
                Attack();
                yield return new WaitForSeconds(attackInterval);
            }
        }

        public void StopAttack() {
            StopCoroutine(nameof(CoStartAttack));
        }

        int CalculatedDamage() {
            int damage = Random.Range(power, power + myStat.Potentiality);

            if (Random.Range(0, 100) < myStat.CriticalRate) {
                damage += myStat.CriticalPower;
                isCritical = true;
            } else {
                isCritical = false;
            }

            return damage;
        }

        void StartReload() {
            isReloading = true;
            weaponManager.Reload();
            Invoke(nameof(CompleteReload), reloadTime);
        }

        void CompleteReload() {
            currentMagazine = magazine;
            weaponManager.UpdateMagazine(currentMagazine);
            isReloading = false;
        }

        public void SetPower(int value) {
            power = value;
        }

        public void AddBullet() {
            shooter.AddBullet();
        }

        public void SetStat(HeroStat stat) {
            myStat = stat;
            power += myStat.Power;
            SetPower(power);
        }

        public void AddRPMbyRate(int rate) {
            rpm = (int)(rpm * (1 + rate / 100f));
            attackInterval = 60f / rpm;
        }
    }
}
