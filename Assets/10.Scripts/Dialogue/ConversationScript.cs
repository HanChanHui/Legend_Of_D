using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class ConversationScript : MonoBehaviour {
        [SerializeField] List<Dialogue> first;
        [SerializeField] List<Dialogue> beforeBoss;
        [SerializeField] List<Dialogue> afterBoss;

        readonly Dictionary<string, List<Dialogue>> dicConversations = new();

        public void Init() {
            dicConversations.Add("First", first);
            dicConversations.Add("BeforeBoss", beforeBoss);
            dicConversations.Add("AfterBoss", afterBoss);
        }

        public List<Dialogue> GetConversation(string key) {
            if (dicConversations.ContainsKey(key)) {
                return dicConversations[key];
            }

            return null;
        }
    }
}
