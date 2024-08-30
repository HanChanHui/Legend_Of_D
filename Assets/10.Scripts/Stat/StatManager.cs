using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class StatManager : MonoBehaviour {
        List<HeroStat> heroStats;
        Dictionary<string, HeroStat> dicHeroStat = new();

        Hero hero;

        public void Init() {
            LoadHeroStat();

            hero = GameManager.Instance.Hero;
        }

        void LoadHeroStat() {
            TextAsset textAsset = Resources.Load<TextAsset>(Constants.HeroStatFile);

            if (textAsset) {
                heroStats = JsonHelper.FromJsonList<HeroStat>(textAsset.text);

                for (int i = 0; i < heroStats.Count; i++) {
                    dicHeroStat.Add(heroStats[i].Name, heroStats[i]);
                }
            }
        }

        public HeroStat GetHeroStat(string name) {
            if (dicHeroStat.ContainsKey(name)) {
                return dicHeroStat[name];
            }

            return null;
        }

        public void AddPowerByRate(int rate) {
            hero.AddPowerByRate(rate);
        }

        public void AddMaxHealth(int value) {
            hero.AddMaxHealth(value);
        }

        public void AddHeroSpeedByRate(int rate) {
            hero.AddHeroSpeedByRate(rate);
        }

        public void AddEvasionRate(int value) {
            hero.AddEvasionRate(value);
        }

        public void AddCriticalRate(int value) {
            hero.AddCriticalRate(value);
        }

        public void AddCriticalDamageByRate(int value) {
            hero.AddCriticalDamageByRate(value);
        }
    }
}
