namespace AniWorld.Cards.Data
{
    public enum RaceType
    {
        WildCat,
        Caravan,
        Snowman,
        FishMan,
        StoneMan,
        Insect
    }

    public enum ClassType
    {
        Warrior,
        Assassin,
        Mage,
        Tank,
        Archer,
        Support
    }

    public enum CardRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum CardCategory
    {
        Character,
        Food
    }

    public enum FoodType
    {
        Dessert,
        Spicy,
        Meat,
        Seafood,
        Plant,
        Alcohol
    }

    public enum TeamType
    {
        Player,
        Enemy,
        Neutral
    }

    public enum ComboEffectType
    {
        DamageSingle,
        DamageAOE,
        Heal,
        BuffAll,
        DebuffEnemy,
        Summon,
        TerrainChange,
        Special
    }

    public enum ComboTargetType
    {
        None,
        Self,
        FriendlyUnit,
        EnemyUnit,
        AnyUnit,
        EmptyCell,
        AnyCell,
        Area,
        Terrain
    }
}
