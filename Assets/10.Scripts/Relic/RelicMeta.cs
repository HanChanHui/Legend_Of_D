namespace HornSpirit {
    [System.Serializable]
    public class RelicMeta {
        public string Name;
        public string ID;
        public string Description;
        public Relic.Type Type;
        public Constants.Grade Grade;
        public int Price;
        public int MaxQuantity;
        public int[] Values;
    }
}
