using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HornSpirit {
    public class RelicManager : MonoBehaviour {
        [Header("UI")]
        [SerializeField] Transform relicUIParent;
        [SerializeField] RelicCardUI relicCardUI;

        Dictionary<string, Relic> dicRelics = new();

        delegate void RelicAction(RelicMeta meta);
        Dictionary<string, RelicAction> dicRelicActions = new();
        Dictionary<string, RelicUI> dicRelicUIs = new();
        Dictionary<string, RelicMeta> dicRelicMetas = new();
        List<RelicMeta> relicMetas;

        Hero hero;

        bool soulEater;
        int soulEaterValue;
        public bool SoulEater { get => soulEater; }
        public int SoulEaterValue { get => soulEaterValue; }

        bool berserkerNecklace;

        public void Init() {
            hero = GameManager.Instance.Hero;

            relicCardUI.Init(this);
            LoadData();
            InitActions();
        }

        void LoadData() {
            string path = "JSON/RelicMeta";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset) {
                relicMetas = JsonHelper.FromJsonList<RelicMeta>(textAsset.text);

                for (int i = 0; i < relicMetas.Count; i++) {
                    dicRelicMetas.Add(relicMetas[i].ID, relicMetas[i]);
                }
            }
        }

        void InitActions() {
            dicRelicActions.Add("Relic_01", RelicActionSoulEater);
            dicRelicActions.Add("Relic_02", RelicActionBattleFlag);
            dicRelicActions.Add("Relic_03", RelicActionBerserkerNecklace);
            dicRelicActions.Add("Relic_04", RelicActionPetOwl);
        }

        public RelicMeta GetRandomRelicMeta() {
            return relicMetas[0];   // for test
        }

        public RelicMeta GetRelicMeta(int index) {
            return relicMetas[index];
        }

        public void AddRelic(string id) {
            if (dicRelics.ContainsKey(id)) {
                Relic relic = dicRelics[id];

                if (relic.Quantity >= relic.Meta.MaxQuantity) {
                    Debug.Log($"Relic [{relic.Meta.Name}] is already max quantity.");
                    return;
                }

                relic.AddQuantity(1);

                RelicUI relicUI = dicRelicUIs[id];
                relicUI.UpdateQuantity(relic.Quantity);
            } else {
                Relic relic = new();
                relic.Init(dicRelicMetas[id]);
                relic.AddQuantity(1);
                dicRelics.Add(id, relic);

                RelicUI relicUI = PoolManager.Instance.NewItem<RelicUI>("UI", "RelicUI");
                relicUI.transform.SetParent(relicUIParent, false);
                relicUI.UpdateUI(relic);
                dicRelicUIs.Add(id, relicUI);
            }

            UseRelic(id);
        }

        void UseRelic(string id) {
            if (dicRelics.ContainsKey(id)) {
                Relic relic = dicRelics[id];
                dicRelicActions[id](relic.Meta);
            }
        }

        void RelicActionSoulEater(RelicMeta meta) {
            Effector.PlayEffect("StarAura", new Vector3(0, 1, 0), hero.transform);
            soulEater = true;
            soulEaterValue = meta.Values[0];
        }

        void RelicActionBattleFlag(RelicMeta meta) {
            Effector.PlayEffect("HealAura", new Vector3(0, 1, 0), hero.transform);
            hero.EnableBattleFlag(meta);
        }

        void RelicActionBerserkerNecklace(RelicMeta meta) {
            berserkerNecklace = true;
        }

        void RelicActionPetOwl(RelicMeta meta) {
            hero.UsePetOwl = true;
        }

        public void ApplyBersekerNecklace() {
            if (berserkerNecklace) {
                RelicMeta meta = dicRelicMetas["Relic_02"];
                hero.EnableForceMode(meta.Values[0], meta.Values[1]);
            }
        }

        public void ShowRelicCard(RelicMeta meta, UnityAction action = null) {
            relicCardUI.UpdateUI(meta);
            relicCardUI.Show(action);
        }
    }
}
