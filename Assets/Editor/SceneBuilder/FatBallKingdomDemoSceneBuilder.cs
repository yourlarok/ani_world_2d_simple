#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using AniWorld.Board;
using AniWorld.Board.Generation;
using AniWorld.Cards.Data;
using AniWorld.Cards.Runtime;
using AniWorld.Cards.UI;
using AniWorld.Combos;
using AniWorld.Food;
using AniWorld.GameFlow;
using AniWorld.Tokens;
using AniWorld.UI.GameFlow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace AniWorld.Editor.SceneBuilder
{
    public static class FatBallKingdomDemoSceneBuilder
    {
        private const string DemoRoot = "Assets/Demo";
        private const string ScenePath = DemoRoot + "/Scenes/Demo_Battle.unity";
        private const string SpriteFolder = DemoRoot + "/Generated/Sprites";
        private const string TileFolder = DemoRoot + "/Generated/Tiles";
        private const string AssetFolder = DemoRoot + "/Generated/Data";
        private const string PrefabFolder = DemoRoot + "/Generated/Prefabs";

        private static readonly Dictionary<TerrainType, TileBase> TerrainTiles = new Dictionary<TerrainType, TileBase>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        [MenuItem("Tools/FatBallKingdom/Build Demo Battle Scene")]
        public static void BuildDemoBattleScene()
        {
            EnsureFolders();
            TerrainTiles.Clear();
            Sprites.Clear();

            CreateSprites();
            CreateTiles();

            BoardTileDatabase tileDatabase = CreateBoardTileDatabase();
            BoardGenerationConfig generationConfig = CreateBoardGenerationConfig();
            CardVisualStyle cardVisualStyle = CreateCardVisualStyle();
            ClassTokenVisualDatabase classVisualDatabase = CreateClassTokenVisualDatabase();
            List<CharacterCardData> characterCards = CreateCharacterCards();
            List<FoodCardData> foodCards = CreateFoodCards();
            List<FoodComboData> combos = CreateCombos(characterCards);
            GameToken tokenPrefab = CreateTokenPrefab();
            CardView cardViewPrefab = CreateCardViewPrefab(cardVisualStyle);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Demo_Battle";

            Camera mainCamera = CreateCamera();
            CreateLightweightEnvironment();

            BoardSceneRefs board = CreateBoard(tileDatabase, generationConfig, tokenPrefab, classVisualDatabase, mainCamera);
            CardSceneRefs cards = CreateCardRuntime(characterCards, foodCards, board);
            FoodSceneRefs food = CreateFoodRuntime();
            ComboSceneRefs combo = CreateComboRuntime(combos, board, cards);
            GameFlowSceneRefs flow = CreateGameFlow(board, cards, food, combo);
            CreateCanvas(flow, board, cards, food, combo, cardViewPrefab, cardVisualStyle, classVisualDatabase);
            CreateEventSystem();

            WireCrossReferences(board, cards, food, combo, flow);
            PaintEditorPreviewBoard(board.boardManager, generationConfig);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("FatBall Kingdom", "Demo_Battle scene generated.\n\nOpen Assets/Demo/Scenes/Demo_Battle.unity and press Play.", "OK");
        }

        private static void EnsureFolders()
        {
            EnsureFolder(DemoRoot);
            EnsureFolder(DemoRoot + "/Scenes");
            EnsureFolder(DemoRoot + "/Generated");
            EnsureFolder(SpriteFolder);
            EnsureFolder(TileFolder);
            EnsureFolder(AssetFolder);
            EnsureFolder(PrefabFolder);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void CreateSprites()
        {
            CreateDiamondSprite("Tile_Grass", new Color(0.36f, 0.62f, 0.28f), new Color(0.18f, 0.34f, 0.15f), false);
            CreateDiamondSprite("Tile_Forest", new Color(0.16f, 0.42f, 0.22f), new Color(0.08f, 0.24f, 0.12f), false);
            CreateDiamondSprite("Tile_Snow", new Color(0.86f, 0.92f, 0.92f), new Color(0.55f, 0.66f, 0.70f), false);
            CreateDiamondSprite("Tile_Desert", new Color(0.78f, 0.61f, 0.25f), new Color(0.50f, 0.36f, 0.14f), false);
            CreateDiamondSprite("Tile_Water", new Color(0.18f, 0.72f, 0.78f), new Color(0.08f, 0.38f, 0.52f), false);
            CreateDiamondSprite("Tile_Swamp", new Color(0.28f, 0.40f, 0.22f), new Color(0.12f, 0.22f, 0.10f), false);
            CreateDiamondSprite("Tile_Stone", new Color(0.52f, 0.50f, 0.43f), new Color(0.27f, 0.27f, 0.25f), false);
            CreateDiamondSprite("Tile_Mud", new Color(0.42f, 0.30f, 0.18f), new Color(0.22f, 0.14f, 0.08f), false);
            CreateDiamondSprite("Tile_Unknown", new Color(0.12f, 0.12f, 0.11f), new Color(0.04f, 0.04f, 0.04f), false);

            CreateDiamondSprite("Highlight_Selected", new Color(1f, 0.85f, 0.18f, 0.42f), new Color(1f, 0.64f, 0.05f, 0.9f), true);
            CreateDiamondSprite("Highlight_Move", new Color(0.15f, 0.70f, 1f, 0.34f), new Color(0.0f, 0.55f, 1f, 0.85f), true);
            CreateDiamondSprite("Highlight_Attack", new Color(1f, 0.18f, 0.12f, 0.34f), new Color(1f, 0.0f, 0.0f, 0.85f), true);

            CreateRectSprite("UI_ParchmentPanel", 256, 160, new Color(0.74f, 0.57f, 0.34f), new Color(0.35f, 0.21f, 0.10f), 14);
            CreateRectSprite("UI_WoodButton", 180, 54, new Color(0.40f, 0.22f, 0.10f), new Color(0.20f, 0.10f, 0.04f), 8);
            CreateRectSprite("UI_DarkPanel", 256, 160, new Color(0.16f, 0.12f, 0.09f, 0.92f), new Color(0.55f, 0.34f, 0.12f), 10);
            CreateRectSprite("UI_GoldBadge", 64, 64, new Color(0.93f, 0.67f, 0.14f), new Color(0.42f, 0.25f, 0.04f), 7);
            CreateRectSprite("UI_APBadge", 64, 64, new Color(0.45f, 0.68f, 0.95f), new Color(0.10f, 0.24f, 0.48f), 7);
            CreateRectSprite("UI_DisabledOverlay", 128, 180, new Color(0.03f, 0.03f, 0.03f, 0.55f), new Color(0.03f, 0.03f, 0.03f, 0.65f), 2);
            CreateRectSprite("UI_PlayableGlow", 128, 180, new Color(1f, 0.83f, 0.18f, 0.12f), new Color(1f, 0.74f, 0.12f, 0.75f), 5);
            CreateRectSprite("UI_SelectedOverlay", 128, 180, new Color(0.95f, 0.86f, 0.32f, 0.18f), new Color(1f, 0.82f, 0.06f, 0.95f), 5);

            CreateRectSprite("Card_CharacterBg", 128, 180, new Color(0.62f, 0.44f, 0.24f), new Color(0.22f, 0.12f, 0.06f), 8);
            CreateRectSprite("Card_FoodBg", 128, 180, new Color(0.55f, 0.37f, 0.18f), new Color(0.18f, 0.10f, 0.04f), 8);
            CreateRectSprite("Frame_Common", 128, 180, new Color(0.55f, 0.52f, 0.45f, 0.25f), new Color(0.54f, 0.50f, 0.42f), 6);
            CreateRectSprite("Frame_Rare", 128, 180, new Color(0.18f, 0.36f, 0.70f, 0.25f), new Color(0.20f, 0.48f, 0.95f), 6);
            CreateRectSprite("Frame_Epic", 128, 180, new Color(0.42f, 0.18f, 0.70f, 0.25f), new Color(0.66f, 0.24f, 0.95f), 6);
            CreateRectSprite("Frame_Legendary", 128, 180, new Color(0.86f, 0.58f, 0.12f, 0.25f), new Color(1f, 0.75f, 0.18f), 6);

            CreateShapeSprite("Token_Warrior_Circle", TokenShape.Circle, new Color(0.82f, 0.62f, 0.22f), new Color(0.28f, 0.18f, 0.06f));
            CreateShapeSprite("Token_Assassin_Diamond", TokenShape.Diamond, new Color(0.44f, 0.40f, 0.55f), new Color(0.14f, 0.12f, 0.20f));
            CreateShapeSprite("Token_Mage_Star", TokenShape.Star, new Color(0.35f, 0.55f, 0.95f), new Color(0.10f, 0.18f, 0.42f));
            CreateShapeSprite("Token_Tank_Square", TokenShape.Square, new Color(0.58f, 0.58f, 0.52f), new Color(0.22f, 0.22f, 0.20f));
            CreateShapeSprite("Token_Archer_Triangle", TokenShape.Triangle, new Color(0.42f, 0.72f, 0.35f), new Color(0.12f, 0.28f, 0.10f));
            CreateShapeSprite("Token_Support_Cross", TokenShape.Cross, new Color(0.78f, 0.42f, 0.70f), new Color(0.32f, 0.10f, 0.26f));

            CreateRectSprite("Portrait_Cat", 128, 128, new Color(0.30f, 0.26f, 0.22f), new Color(0.95f, 0.75f, 0.25f), 6);
            CreateRectSprite("Portrait_Mage", 128, 128, new Color(0.28f, 0.42f, 0.70f), new Color(0.80f, 0.92f, 1f), 6);
            CreateRectSprite("Portrait_Food", 128, 128, new Color(0.72f, 0.42f, 0.22f), new Color(0.95f, 0.76f, 0.36f), 6);
        }

        private static void CreateTiles()
        {
            TerrainTiles[TerrainType.Grass] = CreateTile("Tile_Grass", Sprites["Tile_Grass"]);
            TerrainTiles[TerrainType.Forest] = CreateTile("Tile_Forest", Sprites["Tile_Forest"]);
            TerrainTiles[TerrainType.Snow] = CreateTile("Tile_Snow", Sprites["Tile_Snow"]);
            TerrainTiles[TerrainType.Desert] = CreateTile("Tile_Desert", Sprites["Tile_Desert"]);
            TerrainTiles[TerrainType.Water] = CreateTile("Tile_Water", Sprites["Tile_Water"]);
            TerrainTiles[TerrainType.Swamp] = CreateTile("Tile_Swamp", Sprites["Tile_Swamp"]);
            TerrainTiles[TerrainType.Stone] = CreateTile("Tile_Stone", Sprites["Tile_Stone"]);
            TerrainTiles[TerrainType.Mud] = CreateTile("Tile_Mud", Sprites["Tile_Mud"]);
            TerrainTiles[TerrainType.Unknown] = CreateTile("Tile_Unknown", Sprites["Tile_Unknown"]);

            CreateTile("Highlight_Selected", Sprites["Highlight_Selected"]);
            CreateTile("Highlight_Move", Sprites["Highlight_Move"]);
            CreateTile("Highlight_Attack", Sprites["Highlight_Attack"]);
        }

        private static BoardTileDatabase CreateBoardTileDatabase()
        {
            BoardTileDatabase database = CreateAsset<BoardTileDatabase>(AssetFolder + "/Demo_BoardTileDatabase.asset");
            SerializedObject serialized = new SerializedObject(database);
            SerializedProperty sets = serialized.FindProperty("terrainTileSets");
            sets.ClearArray();

            foreach (KeyValuePair<TerrainType, TileBase> pair in TerrainTiles)
            {
                int index = sets.arraySize;
                sets.InsertArrayElementAtIndex(index);
                SerializedProperty entry = sets.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("terrainType").enumValueIndex = (int)pair.Key;
                SerializedProperty tiles = entry.FindPropertyRelative("tiles");
                tiles.ClearArray();
                tiles.InsertArrayElementAtIndex(0);
                tiles.GetArrayElementAtIndex(0).objectReferenceValue = pair.Value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
            return database;
        }

        private static BoardGenerationConfig CreateBoardGenerationConfig()
        {
            BoardGenerationConfig config = CreateAsset<BoardGenerationConfig>(AssetFolder + "/Demo_BoardGenerationConfig.asset");
            config.origin = new Vector2Int(-5, -4);
            config.width = 11;
            config.height = 9;
            config.fallbackTerrain = TerrainType.Grass;
            config.useFixedSeed = true;
            config.fixedSeed = 20260509;
            config.fillRemainingWithWeightedRules = true;
            config.terrainRules.Clear();
            config.terrainRules.Add(new TerrainSpawnRule
            {
                terrainType = TerrainType.Water,
                minCount = 9,
                maxCount = 12,
                clusterSeedCount = 2,
                clusterGrowthChance = 0.92f,
                regionBias = TerrainRegionBiasMode.Right,
                regionBiasStrength = 1.4f,
                weightedFillWeight = 0.05f
            });
            config.terrainRules.Add(new TerrainSpawnRule
            {
                terrainType = TerrainType.Snow,
                minCount = 12,
                maxCount = 16,
                clusterSeedCount = 2,
                clusterGrowthChance = 0.85f,
                regionBias = TerrainRegionBiasMode.Top,
                regionBiasStrength = 1.5f,
                weightedFillWeight = 0.1f
            });
            config.terrainRules.Add(new TerrainSpawnRule
            {
                terrainType = TerrainType.Forest,
                targetRatio = 0.18f,
                clusterSeedCount = 3,
                clusterGrowthChance = 0.82f,
                regionBias = TerrainRegionBiasMode.Left,
                regionBiasStrength = 1.2f,
                weightedFillWeight = 0.35f
            });
            config.terrainRules.Add(new TerrainSpawnRule
            {
                terrainType = TerrainType.Desert,
                targetRatio = 0.12f,
                clusterSeedCount = 2,
                clusterGrowthChance = 0.75f,
                regionBias = TerrainRegionBiasMode.Right,
                regionBiasStrength = 0.9f,
                weightedFillWeight = 0.25f
            });
            config.terrainRules.Add(new TerrainSpawnRule
            {
                terrainType = TerrainType.Stone,
                minCount = 6,
                maxCount = 8,
                clusterSeedCount = 2,
                clusterGrowthChance = 0.55f,
                regionBias = TerrainRegionBiasMode.Center,
                regionBiasStrength = 0.6f,
                weightedFillWeight = 0.15f
            });
            EditorUtility.SetDirty(config);
            return config;
        }

        private static CardVisualStyle CreateCardVisualStyle()
        {
            CardVisualStyle style = CreateAsset<CardVisualStyle>(AssetFolder + "/Demo_CardVisualStyle.asset");
            style.characterBackground = Sprites["Card_CharacterBg"];
            style.foodBackground = Sprites["Card_FoodBg"];
            style.commonFrame = Sprites["Frame_Common"];
            style.rareFrame = Sprites["Frame_Rare"];
            style.epicFrame = Sprites["Frame_Epic"];
            style.legendaryFrame = Sprites["Frame_Legendary"];
            style.characterTypeFrame = Sprites["Frame_Rare"];
            style.foodTypeFrame = Sprites["Frame_Common"];
            style.goldCostBadge = Sprites["UI_GoldBadge"];
            style.apCostBadge = Sprites["UI_APBadge"];
            style.disabledOverlay = Sprites["UI_DisabledOverlay"];
            style.playableGlow = Sprites["UI_PlayableGlow"];
            style.selectedOverlay = Sprites["UI_SelectedOverlay"];
            EditorUtility.SetDirty(style);
            return style;
        }

        private static ClassTokenVisualDatabase CreateClassTokenVisualDatabase()
        {
            ClassTokenVisualDatabase database = CreateAsset<ClassTokenVisualDatabase>(AssetFolder + "/Demo_ClassTokenVisualDatabase.asset");
            SerializedObject serialized = new SerializedObject(database);
            SerializedProperty entries = serialized.FindProperty("entries");
            entries.ClearArray();
            AddClassVisual(entries, ClassType.Warrior, "Token_Warrior_Circle");
            AddClassVisual(entries, ClassType.Assassin, "Token_Assassin_Diamond");
            AddClassVisual(entries, ClassType.Mage, "Token_Mage_Star");
            AddClassVisual(entries, ClassType.Tank, "Token_Tank_Square");
            AddClassVisual(entries, ClassType.Archer, "Token_Archer_Triangle");
            AddClassVisual(entries, ClassType.Support, "Token_Support_Cross");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
            return database;
        }

        private static void AddClassVisual(SerializedProperty entries, ClassType classType, string spriteKey)
        {
            int index = entries.arraySize;
            entries.InsertArrayElementAtIndex(index);
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("classType").enumValueIndex = (int)classType;
            entry.FindPropertyRelative("tokenSprite").objectReferenceValue = Sprites[spriteKey];
            entry.FindPropertyRelative("classIcon").objectReferenceValue = Sprites[spriteKey];
        }

        private static List<CharacterCardData> CreateCharacterCards()
        {
            List<CharacterCardData> cards = new List<CharacterCardData>();
            cards.Add(CreateCharacterCard("Cat Warrior", RaceType.WildCat, ClassType.Warrior, CardRarity.Common, 2, 2, 25, 8, 0, 5, 2, 3, 10, "A sturdy frontline cat.", "Basic melee strike.", Sprites["Portrait_Cat"], Sprites["Token_Warrior_Circle"]));
            cards.Add(CreateCharacterCard("Fire Mage", RaceType.Snowman, ClassType.Mage, CardRarity.Rare, 4, 4, 12, 2, 12, 2, 5, 2, 15, "A fragile caster with high magic.", "Fire combo specialist.", Sprites["Portrait_Mage"], Sprites["Token_Mage_Star"]));
            cards.Add(CreateCharacterCard("Herbalist", RaceType.Insect, ClassType.Support, CardRarity.Common, 2, 2, 20, 3, 5, 4, 4, 3, 5, "A food-friendly support unit.", "Improves recovery combos.", Sprites["Portrait_Food"], Sprites["Token_Support_Cross"]));
            cards.Add(CreateCharacterCard("Shield Guard", RaceType.StoneMan, ClassType.Tank, CardRarity.Common, 3, 3, 35, 4, 0, 8, 6, 2, 5, "A square defensive token.", "Holds the line.", Sprites["Portrait_Cat"], Sprites["Token_Tank_Square"]));
            cards.Add(CreateCharacterCard("Shadow Rogue", RaceType.WildCat, ClassType.Assassin, CardRarity.Rare, 3, 3, 15, 10, 0, 3, 2, 4, 25, "A fast diamond token.", "High rage attacks.", Sprites["Portrait_Cat"], Sprites["Token_Assassin_Diamond"]));
            cards.Add(CreateCharacterCard("Hawk Archer", RaceType.FishMan, ClassType.Archer, CardRarity.Common, 3, 3, 18, 9, 0, 4, 3, 3, 20, "A balanced ranged unit.", "Triangle token pressure.", Sprites["Portrait_Mage"], Sprites["Token_Archer_Triangle"]));
            return cards;
        }

        private static CharacterCardData CreateCharacterCard(string name, RaceType race, ClassType classType, CardRarity rarity, int gold, int ap, int hp, int patk, int matk, int pdef, int mdef, int mov, int rage, string desc, string skill, Sprite portrait, Sprite token)
        {
            CharacterCardData card = CreateAsset<CharacterCardData>(AssetFolder + "/Cards/" + name.Replace(" ", "_") + ".asset");
            card.cardName = name;
            card.race = race;
            card.classType = classType;
            card.rarity = rarity;
            card.goldCost = gold;
            card.apCost = ap;
            card.baseHP = hp;
            card.basePATK = patk;
            card.baseMATK = matk;
            card.basePDEF = pdef;
            card.baseMDEF = mdef;
            card.baseMOV = mov;
            card.baseRAGE = rage;
            card.description = desc;
            card.skillDescription = skill;
            card.artwork = portrait;
            card.tokenSprite = token;
            EditorUtility.SetDirty(card);
            return card;
        }

        private static List<FoodCardData> CreateFoodCards()
        {
            List<FoodCardData> foods = new List<FoodCardData>();
            foods.Add(CreateFoodCard("Honey Cake", FoodType.Dessert, 1, 5, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, "Sweet recovery food."));
            foods.Add(CreateFoodCard("Pepper Skewer", FoodType.Spicy, 1, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, "Spicy rage food."));
            foods.Add(CreateFoodCard("Roast Beef", FoodType.Meat, 1, 0, 3, 0, 0, 0, 0, 0, 2, 0, 0, 0, "Physical attack food."));
            foods.Add(CreateFoodCard("Grilled Fish", FoodType.Seafood, 1, 0, 0, 3, 0, 0, 0, 0, 0, 2, 0, 0, "Magic attack food."));
            foods.Add(CreateFoodCard("Berries", FoodType.Plant, 1, 0, 0, 0, 3, 2, 0, 0, 0, 0, 2, 2, "Defensive food."));
            foods.Add(CreateFoodCard("Strong Ale", FoodType.Alcohol, 2, 0, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, "Explosive but chaotic."));
            return foods;
        }

        private static FoodCardData CreateFoodCard(string name, FoodType foodType, int gold, int instantHp, int instantPatk, int instantMatk, int instantPdef, int instantMdef, int instantRage, int perHp, int perPatk, int perMatk, int perPdef, int perMdef, string desc)
        {
            FoodCardData food = CreateAsset<FoodCardData>(AssetFolder + "/Cards/" + name.Replace(" ", "_") + ".asset");
            food.cardName = name;
            food.foodType = foodType;
            food.goldCost = gold;
            food.artwork = Sprites["Portrait_Food"];
            food.description = desc;
            food.instantHP = instantHp;
            food.instantPATK = instantPatk;
            food.instantMATK = instantMatk;
            food.instantPDEF = instantPdef;
            food.instantMDEF = instantMdef;
            food.instantRAGE = instantRage;
            food.perTurnHP = perHp;
            food.perTurnPATK = perPatk;
            food.perTurnMATK = perMatk;
            food.perTurnPDEF = perPdef;
            food.perTurnMDEF = perMdef;
            EditorUtility.SetDirty(food);
            return food;
        }

        private static List<FoodComboData> CreateCombos(List<CharacterCardData> characterCards)
        {
            List<FoodComboData> combos = new List<FoodComboData>();
            FoodComboData damage = CreateAsset<FoodComboData>(AssetFolder + "/Combos/Demo_PowerSnack.asset");
            damage.comboName = "Power Snack";
            damage.description = "A spicy meat dessert attack.";
            damage.recipe = new[] { FoodType.Dessert, FoodType.Spicy, FoodType.Meat };
            damage.apCost = 2;
            damage.effectType = ComboEffectType.DamageSingle;
            damage.targetType = ComboTargetType.EnemyUnit;
            damage.range = 4;
            damage.damage = 12;
            combos.Add(damage);

            FoodComboData terrain = CreateAsset<FoodComboData>(AssetFolder + "/Combos/Demo_GardenBloom.asset");
            terrain.comboName = "Garden Bloom";
            terrain.description = "Turns a small area into forest.";
            terrain.recipe = new[] { FoodType.Plant, FoodType.Dessert, FoodType.Seafood };
            terrain.apCost = 2;
            terrain.effectType = ComboEffectType.TerrainChange;
            terrain.targetType = ComboTargetType.Terrain;
            terrain.range = 5;
            terrain.aoeRadius = 1;
            terrain.terrainChange = TerrainType.Forest;
            combos.Add(terrain);

            FoodComboData summon = CreateAsset<FoodComboData>(AssetFolder + "/Combos/Demo_TabletopSummon.asset");
            summon.comboName = "Tabletop Summon";
            summon.description = "Summons a helper token.";
            summon.recipe = new[] { FoodType.Alcohol, FoodType.Meat, FoodType.Plant };
            summon.apCost = 2;
            summon.effectType = ComboEffectType.Summon;
            summon.targetType = ComboTargetType.EmptyCell;
            summon.range = 3;
            summon.summonCharacter = characterCards.Count > 2 ? characterCards[2] : null;
            combos.Add(summon);

            foreach (FoodComboData combo in combos)
            {
                EditorUtility.SetDirty(combo);
            }

            return combos;
        }

        private static GameToken CreateTokenPrefab()
        {
            GameObject prefabRoot = new GameObject("Demo_GameToken");
            SpriteRenderer spriteRenderer = prefabRoot.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprites["Token_Warrior_Circle"];
            spriteRenderer.sortingOrder = 20;
            GameToken token = prefabRoot.AddComponent<GameToken>();
            SetObject(token, "tokenSpriteRenderer", spriteRenderer);
            string path = PrefabFolder + "/Demo_GameToken.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            Object.DestroyImmediate(prefabRoot);
            return prefab.GetComponent<GameToken>();
        }

        private static CardView CreateCardViewPrefab(CardVisualStyle visualStyle)
        {
            GameObject root = CreateUIObject("Demo_CardView", null, new Vector2(128f, 180f));
            Image background = root.AddComponent<Image>();
            background.sprite = visualStyle.characterBackground;
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            CardView view = root.AddComponent<CardView>();

            Image rarityFrame = AddImageChild(root.transform, "RarityFrame", visualStyle.commonFrame, Vector2.zero, new Vector2(128f, 180f));
            Image typeFrame = AddImageChild(root.transform, "TypeFrame", visualStyle.characterTypeFrame, Vector2.zero, new Vector2(118f, 170f));
            Image art = AddImageChild(root.transform, "Artwork", Sprites["Portrait_Cat"], new Vector2(0f, 30f), new Vector2(86f, 70f));
            Image goldBadge = AddImageChild(root.transform, "GoldBadge", visualStyle.goldCostBadge, new Vector2(-42f, 70f), new Vector2(32f, 32f));
            Image apBadge = AddImageChild(root.transform, "APBadge", visualStyle.apCostBadge, new Vector2(42f, 70f), new Vector2(32f, 32f));
            Text name = AddTextChild(root.transform, "NameText", "Card", 14, new Vector2(0f, -20f), new Vector2(110f, 24f), TextAnchor.MiddleCenter);
            Text gold = AddTextChild(root.transform, "GoldText", "1", 14, new Vector2(-42f, 70f), new Vector2(32f, 24f), TextAnchor.MiddleCenter);
            Text ap = AddTextChild(root.transform, "APText", "1", 14, new Vector2(42f, 70f), new Vector2(32f, 24f), TextAnchor.MiddleCenter);
            Text desc = AddTextChild(root.transform, "DescriptionText", "Description", 10, new Vector2(0f, -58f), new Vector2(110f, 42f), TextAnchor.UpperCenter);
            Text blocked = AddTextChild(root.transform, "BlockReasonText", "", 9, new Vector2(0f, -82f), new Vector2(110f, 20f), TextAnchor.MiddleCenter);
            Image disabled = AddImageChild(root.transform, "DisabledOverlay", visualStyle.disabledOverlay, Vector2.zero, new Vector2(128f, 180f));
            Image glow = AddImageChild(root.transform, "PlayableGlow", visualStyle.playableGlow, Vector2.zero, new Vector2(136f, 188f));
            Image selected = AddImageChild(root.transform, "SelectedOverlay", visualStyle.selectedOverlay, Vector2.zero, new Vector2(136f, 188f));
            disabled.enabled = false;
            selected.enabled = false;

            SetObject(view, "visualStyle", visualStyle);
            SetObject(view, "canvasGroup", canvasGroup);
            SetObject(view, "background", background);
            SetObject(view, "rarityFrame", rarityFrame);
            SetObject(view, "cardTypeFrame", typeFrame);
            SetObject(view, "artwork", art);
            SetObject(view, "goldCostBadge", goldBadge);
            SetObject(view, "apCostBadge", apBadge);
            SetObject(view, "disabledOverlay", disabled);
            SetObject(view, "playableGlow", glow);
            SetObject(view, "selectedOverlay", selected);
            SetObject(view, "nameText", name);
            SetObject(view, "goldCostText", gold);
            SetObject(view, "apCostText", ap);
            SetObject(view, "descriptionText", desc);
            SetObject(view, "blockReasonText", blocked);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/Demo_CardView.prefab");
            Object.DestroyImmediate(root);
            return prefab.GetComponent<CardView>();
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.8f;
            camera.transform.position = new Vector3(0f, -0.45f, -10f);
            camera.backgroundColor = new Color(0.18f, 0.12f, 0.07f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.tag = "MainCamera";
            return camera;
        }

        private static void CreateLightweightEnvironment()
        {
            GameObject root = new GameObject("TabletopBackdrop");
            SpriteRenderer table = root.AddComponent<SpriteRenderer>();
            table.sprite = Sprites["UI_DarkPanel"];
            table.drawMode = SpriteDrawMode.Sliced;
            table.size = new Vector2(18f, 11f);
            table.sortingOrder = -50;
            root.transform.position = new Vector3(0f, -0.1f, 1f);
        }

        private static BoardSceneRefs CreateBoard(BoardTileDatabase tileDatabase, BoardGenerationConfig generationConfig, GameToken tokenPrefab, ClassTokenVisualDatabase classVisualDatabase, Camera camera)
        {
            BoardSceneRefs refs = new BoardSceneRefs();
            GameObject boardRoot = new GameObject("BoardRoot");
            refs.boardRoot = boardRoot;

            GameObject gridObject = new GameObject("Grid");
            gridObject.transform.SetParent(boardRoot.transform);
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, 0.5f, 1f);

            refs.terrainTilemap = CreateTilemap(gridObject.transform, "TerrainTilemap", 0);
            refs.decorationTilemap = CreateTilemap(gridObject.transform, "DecorationTilemap", 1);
            refs.highlightTilemap = CreateTilemap(gridObject.transform, "HighlightTilemap", 5);
            refs.overlayTilemap = CreateTilemap(gridObject.transform, "OverlayTilemap", 6);
            refs.terrainTilemap.gameObject.AddComponent<BoardTilemapCollisionSetup>();

            refs.unitsRoot = new GameObject("UnitsRoot").transform;
            refs.unitsRoot.SetParent(boardRoot.transform);

            refs.boardManager = boardRoot.AddComponent<BoardManager>();
            refs.boardHighlighter = boardRoot.AddComponent<BoardHighlighter>();
            refs.boardInputController = boardRoot.AddComponent<BoardInputController>();
            refs.boardRandomGenerator = boardRoot.AddComponent<BoardRandomGenerator>();
            refs.tokenSpawner = boardRoot.AddComponent<TokenSpawner>();
            refs.deploymentZone = boardRoot.AddComponent<DeploymentZone>();

            SetObject(refs.boardManager, "terrainTilemap", refs.terrainTilemap);
            SetObject(refs.boardManager, "decorationTilemap", refs.decorationTilemap);
            SetObject(refs.boardManager, "highlightTilemap", refs.highlightTilemap);
            SetObject(refs.boardManager, "overlayTilemap", refs.overlayTilemap);
            SetObject(refs.boardManager, "tileDatabase", tileDatabase);
            SetBool(refs.boardManager, "generateOnStart", false);

            SetObject(refs.boardHighlighter, "highlightTilemap", refs.highlightTilemap);
            SetObject(refs.boardHighlighter, "selectedTile", AssetDatabase.LoadAssetAtPath<TileBase>(TileFolder + "/Highlight_Selected.asset"));
            SetObject(refs.boardHighlighter, "moveRangeTile", AssetDatabase.LoadAssetAtPath<TileBase>(TileFolder + "/Highlight_Move.asset"));
            SetObject(refs.boardHighlighter, "attackRangeTile", AssetDatabase.LoadAssetAtPath<TileBase>(TileFolder + "/Highlight_Attack.asset"));

            SetObject(refs.boardInputController, "mainCamera", camera);
            SetObject(refs.boardInputController, "boardManager", refs.boardManager);
            SetObject(refs.boardInputController, "highlighter", refs.boardHighlighter);
            SetObject(refs.boardInputController, "terrainTilemap", refs.terrainTilemap);

            SetObject(refs.boardRandomGenerator, "boardManager", refs.boardManager);
            SetObject(refs.boardRandomGenerator, "generationConfig", generationConfig);
            SetBool(refs.boardRandomGenerator, "generateOnStart", false);
            SetBool(refs.boardRandomGenerator, "loadDefaultMapOnStart", false);

            SetObject(refs.tokenSpawner, "boardManager", refs.boardManager);
            SetObject(refs.tokenSpawner, "unitsRoot", refs.unitsRoot);
            SetObject(refs.tokenSpawner, "defaultTokenPrefab", tokenPrefab);
            SetObject(refs.tokenSpawner, "classVisualDatabase", classVisualDatabase);
            SetObject(refs.tokenSpawner, "deploymentZone", refs.deploymentZone);

            SetBool(refs.deploymentZone, "useRect", true);
            SetVector2Int(refs.deploymentZone, "rectOrigin", new Vector2Int(-5, -4));
            SetInt(refs.deploymentZone, "width", 4);
            SetInt(refs.deploymentZone, "height", 9);

            return refs;
        }

        private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            GameObject tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parent);
            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }

        private static CardSceneRefs CreateCardRuntime(List<CharacterCardData> characters, List<FoodCardData> foods, BoardSceneRefs board)
        {
            GameObject root = new GameObject("CardRuntime");
            CardSceneRefs refs = new CardSceneRefs();
            refs.root = root;
            refs.cardPool = root.AddComponent<CardPool>();
            refs.handManager = root.AddComponent<HandManager>();
            refs.goldManager = root.AddComponent<GoldManager>();
            refs.apManager = root.AddComponent<APManager>();
            refs.cardPlayValidator = root.AddComponent<CardPlayValidator>();
            refs.cardDragController = root.AddComponent<CardDragController>();

            SerializedObject pool = new SerializedObject(refs.cardPool);
            FillObjectList(pool.FindProperty("allCharacterCards"), characters);
            FillObjectList(pool.FindProperty("allFoodCards"), foods);
            pool.FindProperty("buildOnAwake").boolValue = false;
            pool.FindProperty("characterTargetCount").intValue = 20;
            pool.ApplyModifiedPropertiesWithoutUndo();

            SetBool(refs.handManager, "initializeOnStart", false);
            SetObject(refs.handManager, "cardPool", refs.cardPool);
            SetObject(refs.handManager, "goldManager", refs.goldManager);
            SetObject(refs.handManager, "apManager", refs.apManager);
            SetObject(refs.handManager, "tokenSpawner", board.tokenSpawner);
            SetObject(refs.handManager, "validator", refs.cardPlayValidator);

            SetObject(refs.cardPlayValidator, "boardManager", board.boardManager);
            SetObject(refs.cardPlayValidator, "goldManager", refs.goldManager);
            SetObject(refs.cardPlayValidator, "apManager", refs.apManager);
            SetObject(refs.cardPlayValidator, "tokenSpawner", board.tokenSpawner);

            SetObject(refs.cardDragController, "mainCamera", Camera.main);
            SetObject(refs.cardDragController, "boardManager", board.boardManager);
            SetObject(refs.cardDragController, "highlighter", board.boardHighlighter);
            SetObject(refs.cardDragController, "terrainTilemap", board.terrainTilemap);
            SetObject(refs.cardDragController, "handManager", refs.handManager);
            SetObject(refs.cardDragController, "validator", refs.cardPlayValidator);

            return refs;
        }

        private static FoodSceneRefs CreateFoodRuntime()
        {
            GameObject root = new GameObject("FoodRuntime");
            return new FoodSceneRefs { root = root, foodBar = root.AddComponent<FoodBar>() };
        }

        private static ComboSceneRefs CreateComboRuntime(List<FoodComboData> combos, BoardSceneRefs board, CardSceneRefs cards)
        {
            GameObject root = new GameObject("ComboRuntime");
            ComboSceneRefs refs = new ComboSceneRefs();
            refs.root = root;
            refs.comboManager = root.AddComponent<ComboManager>();
            refs.comboResolver = root.AddComponent<ComboResolver>();
            refs.comboTargetingController = root.AddComponent<ComboTargetingController>();

            SerializedObject manager = new SerializedObject(refs.comboManager);
            FillObjectList(manager.FindProperty("commonCombos"), combos);
            manager.ApplyModifiedPropertiesWithoutUndo();

            SetObject(refs.comboResolver, "boardManager", board.boardManager);
            SetObject(refs.comboResolver, "apManager", cards.apManager);
            SetObject(refs.comboResolver, "tokenSpawner", board.tokenSpawner);

            SetObject(refs.comboTargetingController, "boardManager", board.boardManager);
            SetObject(refs.comboTargetingController, "highlighter", board.boardHighlighter);
            SetObject(refs.comboTargetingController, "resolver", refs.comboResolver);
            SetObject(refs.comboTargetingController, "tokenSpawner", board.tokenSpawner);
            return refs;
        }

        private static GameFlowSceneRefs CreateGameFlow(BoardSceneRefs board, CardSceneRefs cards, FoodSceneRefs food, ComboSceneRefs combo)
        {
            GameObject root = new GameObject("GameFlowRuntime");
            GameFlowSceneRefs refs = new GameFlowSceneRefs();
            refs.root = root;
            refs.gameStateManager = root.AddComponent<GameStateManager>();
            refs.runSetupManager = root.AddComponent<RunSetupManager>();
            refs.turnManager = root.AddComponent<TurnManager>();
            refs.victoryConditionManager = root.AddComponent<VictoryConditionManager>();
            refs.bootstrap = root.AddComponent<GameFlowBootstrap>();

            SetObject(refs.gameStateManager, "runSetupManager", refs.runSetupManager);
            SetObject(refs.gameStateManager, "turnManager", refs.turnManager);

            SetObject(refs.runSetupManager, "boardManager", board.boardManager);
            SetObject(refs.runSetupManager, "boardRandomGenerator", board.boardRandomGenerator);
            SetBool(refs.runSetupManager, "useRandomGenerator", true);
            SetObject(refs.runSetupManager, "cardPool", cards.cardPool);
            SetObject(refs.runSetupManager, "handManager", cards.handManager);
            SetObject(refs.runSetupManager, "goldManager", cards.goldManager);
            SetObject(refs.runSetupManager, "apManager", cards.apManager);
            SetObject(refs.runSetupManager, "foodBar", food.foodBar);
            SetObject(refs.runSetupManager, "tokenSpawner", board.tokenSpawner);
            SetObject(refs.runSetupManager, "turnManager", refs.turnManager);

            SetObject(refs.turnManager, "goldManager", cards.goldManager);
            SetObject(refs.turnManager, "apManager", cards.apManager);
            SetObject(refs.turnManager, "handManager", cards.handManager);
            SetObject(refs.turnManager, "tokenSpawner", board.tokenSpawner);
            SetBool(refs.turnManager, "grantResourcesOnFirstPlayerTurn", false);

            SetObject(refs.victoryConditionManager, "tokenSpawner", board.tokenSpawner);
            SetObject(refs.bootstrap, "gameStateManager", refs.gameStateManager);
            SetBool(refs.bootstrap, "goToMainMenuOnStart", true);
            SetBool(refs.bootstrap, "startRunOnStart", false);
            return refs;
        }

        private static void CreateCanvas(GameFlowSceneRefs flow, BoardSceneRefs board, CardSceneRefs cards, FoodSceneRefs food, ComboSceneRefs combo, CardView cardViewPrefab, CardVisualStyle style, ClassTokenVisualDatabase classVisualDatabase)
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1366f, 768f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject mainMenu = CreatePanel(canvasObject.transform, "MainMenuPanel", Sprites["UI_DarkPanel"], Vector2.zero, new Vector2(1366f, 768f), true);
            GameObject battleHud = CreatePanel(canvasObject.transform, "BattleHUDPanel", null, Vector2.zero, new Vector2(1366f, 768f), false);
            GameObject pauseMenu = CreatePanel(canvasObject.transform, "PauseMenuPanel", Sprites["UI_DarkPanel"], Vector2.zero, new Vector2(1366f, 768f), false);
            GameObject resultPanel = CreatePanel(canvasObject.transform, "ResultPanel", Sprites["UI_DarkPanel"], Vector2.zero, new Vector2(1366f, 768f), false);

            GameStateViewRouter router = canvasObject.AddComponent<GameStateViewRouter>();
            SetObject(router, "gameStateManager", flow.gameStateManager);
            SetObject(router, "mainMenuPanel", mainMenu);
            SetObject(router, "battleHudPanel", battleHud);
            SetObject(router, "pauseMenuPanel", pauseMenu);
            SetObject(router, "resultPanel", resultPanel);

            BuildMainMenu(mainMenu.transform, flow.gameStateManager);
            BuildBattleHud(battleHud.transform, flow, cards);
            BuildHandPanel(battleHud.transform, cards, cardViewPrefab);
            BuildFoodBarPanel(battleHud.transform, food, style);
            BuildCharacterPanel(battleHud.transform, board, combo, classVisualDatabase);
            BuildComboReadyPanel(battleHud.transform, combo);
            BuildPauseMenu(pauseMenu.transform, flow.gameStateManager);
            BuildResultPanel(resultPanel.transform, flow.gameStateManager);
        }

        private static void BuildMainMenu(Transform parent, GameStateManager gameStateManager)
        {
            Image logo = AddImageChild(parent, "TitleLogo", Sprites["UI_ParchmentPanel"], new Vector2(0f, 210f), new Vector2(520f, 120f));
            Text title = AddTextChild(logo.transform, "TitleText", "FatBall Kingdom", 44, Vector2.zero, new Vector2(500f, 90f), TextAnchor.MiddleCenter);
            title.color = new Color(0.18f, 0.09f, 0.02f);
            Button start = AddButton(parent, "StartButton", "Start Game", new Vector2(0f, 40f));
            Button options = AddButton(parent, "OptionsButton", "Options", new Vector2(0f, -40f));
            Button quit = AddButton(parent, "QuitButton", "Quit", new Vector2(0f, -120f));
            GameObject optionsPanel = CreatePanel(parent, "OptionsPanel", Sprites["UI_ParchmentPanel"], new Vector2(360f, -40f), new Vector2(300f, 200f), false);
            AddTextChild(optionsPanel.transform, "OptionsText", "Options placeholder", 20, Vector2.zero, new Vector2(260f, 100f), TextAnchor.MiddleCenter);

            MainMenuView view = parent.gameObject.AddComponent<MainMenuView>();
            SetObject(view, "gameStateManager", gameStateManager);
            SetObject(view, "startButton", start);
            SetObject(view, "optionsButton", options);
            SetObject(view, "quitButton", quit);
            SetObject(view, "optionsPanel", optionsPanel);
            SetObject(view, "backgroundImage", parent.GetComponent<Image>());
            SetObject(view, "titleLogo", logo);
        }

        private static void BuildBattleHud(Transform parent, GameFlowSceneRefs flow, CardSceneRefs cards)
        {
            GameObject hudFrame = CreatePanel(parent, "HUDFrame", Sprites["UI_ParchmentPanel"], new Vector2(470f, -305f), new Vector2(260f, 130f), true);
            Text turn = AddTextChild(hudFrame.transform, "TurnText", "Turn 1", 18, new Vector2(0f, 38f), new Vector2(210f, 24f), TextAnchor.MiddleCenter);
            Text phase = AddTextChild(hudFrame.transform, "PhaseText", "MainMenu", 16, new Vector2(0f, 10f), new Vector2(210f, 24f), TextAnchor.MiddleCenter);
            Text gold = AddTextChild(hudFrame.transform, "GoldText", "Gold 10", 14, new Vector2(-58f, -20f), new Vector2(100f, 24f), TextAnchor.MiddleCenter);
            Text ap = AddTextChild(hudFrame.transform, "APText", "AP 8/10", 14, new Vector2(58f, -20f), new Vector2(100f, 24f), TextAnchor.MiddleCenter);
            Button endTurn = AddButton(hudFrame.transform, "EndTurnButton", "End Turn", new Vector2(0f, -52f), new Vector2(150f, 34f));
            Button pause = AddButton(parent, "PauseButton", "II", new Vector2(625f, 330f), new Vector2(56f, 44f));

            BattleHUDView view = parent.gameObject.AddComponent<BattleHUDView>();
            SetObject(view, "gameStateManager", flow.gameStateManager);
            SetObject(view, "turnManager", flow.turnManager);
            SetObject(view, "goldManager", cards.goldManager);
            SetObject(view, "apManager", cards.apManager);
            SetObject(view, "turnText", turn);
            SetObject(view, "phaseText", phase);
            SetObject(view, "goldText", gold);
            SetObject(view, "apText", ap);
            SetObject(view, "endTurnButton", endTurn);
            SetObject(view, "pauseButton", pause);
            SetObject(view, "hudPanelFrame", hudFrame.GetComponent<Image>());
        }

        private static void BuildHandPanel(Transform parent, CardSceneRefs cards, CardView cardViewPrefab)
        {
            GameObject panel = CreatePanel(parent, "HandPanel", null, new Vector2(0f, -295f), new Vector2(760f, 210f), true);
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = -18f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            HandView handView = panel.AddComponent<HandView>();
            SetObject(handView, "handManager", cards.handManager);
            SetObject(handView, "validator", cards.cardPlayValidator);
            SetObject(handView, "dragController", cards.cardDragController);
            SetObject(handView, "cardViewPrefab", cardViewPrefab);
            SetObject(handView, "cardContainer", panel.transform);
        }

        private static void BuildFoodBarPanel(Transform parent, FoodSceneRefs food, CardVisualStyle style)
        {
            GameObject panel = CreatePanel(parent, "FoodBarPanel", Sprites["UI_ParchmentPanel"], new Vector2(-520f, -310f), new Vector2(260f, 96f), true);
            FoodBarView view = panel.AddComponent<FoodBarView>();
            SetObject(view, "foodBar", food.foodBar);
            SetObject(view, "visualStyle", style);
            SetObject(view, "barBackground", panel.GetComponent<Image>());

            List<Image> icons = new List<Image>();
            List<Image> frames = new List<Image>();
            List<Image> empty = new List<Image>();
            List<Text> labels = new List<Text>();
            for (int i = 0; i < 6; i++)
            {
                float x = -102f + i * 41f;
                Image frame = AddImageChild(panel.transform, "FoodSlotFrame_" + i, Sprites["Frame_Common"], new Vector2(x, 6f), new Vector2(36f, 36f));
                Image icon = AddImageChild(panel.transform, "FoodIcon_" + i, null, new Vector2(x, 6f), new Vector2(28f, 28f));
                Image emptyIcon = AddImageChild(panel.transform, "FoodEmpty_" + i, Sprites["UI_DisabledOverlay"], new Vector2(x, 6f), new Vector2(28f, 28f));
                Text label = AddTextChild(panel.transform, "FoodLabel_" + i, "", 8, new Vector2(x, -28f), new Vector2(38f, 16f), TextAnchor.MiddleCenter);
                frames.Add(frame);
                icons.Add(icon);
                empty.Add(emptyIcon);
                labels.Add(label);
            }

            SetImageList(view, "slotImages", icons);
            SetImageList(view, "slotFrames", frames);
            SetImageList(view, "slotEmptyIcons", empty);
            SetTextList(view, "slotLabels", labels);
        }

        private static void BuildCharacterPanel(Transform parent, BoardSceneRefs board, ComboSceneRefs combo, ClassTokenVisualDatabase classVisualDatabase)
        {
            GameObject panel = CreatePanel(parent, "CharacterPanel", Sprites["UI_ParchmentPanel"], new Vector2(-530f, 165f), new Vector2(260f, 330f), false);
            CharacterPanelView view = panel.AddComponent<CharacterPanelView>();
            SetObject(view, "comboTargetingController", combo.comboTargetingController);
            SetObject(view, "classVisualDatabase", classVisualDatabase);
            SetObject(view, "panelFrame", panel.GetComponent<Image>());
            SetObject(view, "portrait", AddImageChild(panel.transform, "Portrait", Sprites["Portrait_Cat"], new Vector2(0f, 105f), new Vector2(90f, 90f)));
            SetObject(view, "nameText", AddTextChild(panel.transform, "NameText", "Token", 18, new Vector2(0f, 48f), new Vector2(220f, 24f), TextAnchor.MiddleCenter));
            SetObject(view, "hpText", AddTextChild(panel.transform, "HPText", "HP", 13, new Vector2(-70f, 12f), new Vector2(90f, 20f), TextAnchor.MiddleLeft));
            SetObject(view, "patkText", AddTextChild(panel.transform, "PATKText", "PATK", 13, new Vector2(60f, 12f), new Vector2(90f, 20f), TextAnchor.MiddleLeft));
            SetObject(view, "matkText", AddTextChild(panel.transform, "MATKText", "MATK", 13, new Vector2(-70f, -14f), new Vector2(90f, 20f), TextAnchor.MiddleLeft));
            SetObject(view, "pdefText", AddTextChild(panel.transform, "PDEFText", "PDEF", 13, new Vector2(60f, -14f), new Vector2(90f, 20f), TextAnchor.MiddleLeft));
            SetObject(view, "mdefText", AddTextChild(panel.transform, "MDEFText", "MDEF", 13, new Vector2(-70f, -40f), new Vector2(90f, 20f), TextAnchor.MiddleLeft));
            SetObject(view, "movText", AddTextChild(panel.transform, "MOVText", "MOV", 13, new Vector2(60f, -40f), new Vector2(90f, 20f), TextAnchor.MiddleLeft));
            SetObject(view, "rageText", AddTextChild(panel.transform, "RAGEText", "RAGE", 13, new Vector2(0f, -66f), new Vector2(160f, 20f), TextAnchor.MiddleCenter));
            SetObject(view, "comboNameText", AddTextChild(panel.transform, "ComboNameText", "", 12, new Vector2(0f, -105f), new Vector2(200f, 22f), TextAnchor.MiddleCenter));
            SetObject(view, "fastEatButton", AddButton(panel.transform, "FastEatButton", "Fast Eat", new Vector2(0f, -135f), new Vector2(120f, 30f)));

            BoardSelectionPanelController selectionController = panel.AddComponent<BoardSelectionPanelController>();
            SetObject(selectionController, "boardInputController", board.boardInputController);
            SetObject(selectionController, "characterPanelView", view);
        }

        private static void BuildComboReadyPanel(Transform parent, ComboSceneRefs combo)
        {
            GameObject panel = CreatePanel(parent, "ComboReadyPanel", Sprites["UI_ParchmentPanel"], new Vector2(515f, 175f), new Vector2(260f, 150f), false);
            ComboReadyView view = panel.AddComponent<ComboReadyView>();
            SetObject(view, "comboTargetingController", combo.comboTargetingController);
            SetObject(view, "panelFrame", panel.GetComponent<Image>());
            SetObject(view, "comboIcon", AddImageChild(panel.transform, "ComboIcon", Sprites["UI_APBadge"], new Vector2(-90f, 35f), new Vector2(48f, 48f)));
            SetObject(view, "comboNameText", AddTextChild(panel.transform, "ComboNameText", "Combo Ready", 16, new Vector2(25f, 45f), new Vector2(160f, 24f), TextAnchor.MiddleCenter));
            SetObject(view, "comboDescriptionText", AddTextChild(panel.transform, "ComboDescriptionText", "", 10, new Vector2(25f, 12f), new Vector2(170f, 40f), TextAnchor.MiddleCenter));
            SetObject(view, "apCostText", AddTextChild(panel.transform, "APCostText", "2", 14, new Vector2(-90f, -28f), new Vector2(48f, 24f), TextAnchor.MiddleCenter));
            SetObject(view, "targetTypeText", AddTextChild(panel.transform, "TargetTypeText", "", 10, new Vector2(25f, -28f), new Vector2(170f, 20f), TextAnchor.MiddleCenter));
            SetObject(view, "fastEatButton", AddButton(panel.transform, "FastEatButton", "Cast", new Vector2(25f, -58f), new Vector2(100f, 28f)));
        }

        private static void BuildPauseMenu(Transform parent, GameStateManager gameStateManager)
        {
            GameObject panel = CreatePanel(parent, "PauseInnerPanel", Sprites["UI_ParchmentPanel"], Vector2.zero, new Vector2(360f, 260f), true);
            Button resume = AddButton(panel.transform, "ResumeButton", "Resume", new Vector2(0f, 35f));
            Button menu = AddButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -45f));
            PauseMenuView view = parent.gameObject.AddComponent<PauseMenuView>();
            SetObject(view, "gameStateManager", gameStateManager);
            SetObject(view, "resumeButton", resume);
            SetObject(view, "mainMenuButton", menu);
            SetObject(view, "backgroundImage", parent.GetComponent<Image>());
            SetObject(view, "panelFrame", panel.GetComponent<Image>());
        }

        private static void BuildResultPanel(Transform parent, GameStateManager gameStateManager)
        {
            GameObject panel = CreatePanel(parent, "ResultInnerPanel", Sprites["UI_ParchmentPanel"], Vector2.zero, new Vector2(420f, 300f), true);
            Text title = AddTextChild(panel.transform, "TitleText", "Victory", 34, new Vector2(0f, 82f), new Vector2(360f, 48f), TextAnchor.MiddleCenter);
            Text message = AddTextChild(panel.transform, "MessageText", "", 18, new Vector2(0f, 25f), new Vector2(340f, 42f), TextAnchor.MiddleCenter);
            Button retry = AddButton(panel.transform, "RetryButton", "Retry", new Vector2(0f, -45f));
            Button menu = AddButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -115f));
            ResultView view = parent.gameObject.AddComponent<ResultView>();
            SetObject(view, "gameStateManager", gameStateManager);
            SetObject(view, "titleText", title);
            SetObject(view, "messageText", message);
            SetObject(view, "retryButton", retry);
            SetObject(view, "mainMenuButton", menu);
            SetObject(view, "backgroundImage", parent.GetComponent<Image>());
            SetObject(view, "panelFrame", panel.GetComponent<Image>());
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static void WireCrossReferences(BoardSceneRefs board, CardSceneRefs cards, FoodSceneRefs food, ComboSceneRefs combo, GameFlowSceneRefs flow)
        {
            SetObject(cards.handManager, "foodBar", food.foodBar);
            SetObject(cards.cardPlayValidator, "foodBar", food.foodBar);
        }

        private static void PaintEditorPreviewBoard(BoardManager boardManager, BoardGenerationConfig config)
        {
            BoardGenerationResult result = BoardRandomMapBuilder.Generate(config);
            if (result != null)
            {
                result.ApplyTo(boardManager, true);
            }
        }

        private static TileBase CreateTile(string name, Sprite sprite)
        {
            string path = TileFolder + "/" + name + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace("\\", "/"));
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        private static void CreateDiamondSprite(string name, Color fill, Color border, bool transparent)
        {
            int width = 128;
            int height = 64;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Vector2 center = new Vector2(width / 2f, height / 2f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs(x - center.x) / (width / 2f);
                    float dy = Mathf.Abs(y - center.y) / (height / 2f);
                    float d = dx + dy;
                    if (d <= 1f)
                    {
                        texture.SetPixel(x, y, d > 0.88f ? border : fill);
                    }
                    else
                    {
                        texture.SetPixel(x, y, transparent ? clear : clear);
                    }
                }
            }

            SaveSpriteTexture(name, texture, 64f, Vector4.zero);
        }

        private static void CreateRectSprite(string name, int width, int height, Color fill, Color border, int borderSize)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x < borderSize || x >= width - borderSize || y < borderSize || y >= height - borderSize;
                    texture.SetPixel(x, y, isBorder ? border : fill);
                }
            }

            SaveSpriteTexture(name, texture, 100f, new Vector4(borderSize, borderSize, borderSize, borderSize));
        }

        private enum TokenShape
        {
            Circle,
            Diamond,
            Star,
            Square,
            Triangle,
            Cross
        }

        private static void CreateShapeSprite(string name, TokenShape shape, Color fill, Color border)
        {
            int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsInsideShape(shape, x, y, center, size);
                    bool inner = IsInsideShape(shape, x, y, center, size - 14);
                    texture.SetPixel(x, y, inside ? (inner ? fill : border) : clear);
                }
            }

            SaveSpriteTexture(name, texture, 100f, Vector4.zero);
        }

        private static bool IsInsideShape(TokenShape shape, int x, int y, Vector2 center, int size)
        {
            float half = size / 2f;
            float dx = Mathf.Abs(x - center.x);
            float dy = Mathf.Abs(y - center.y);
            switch (shape)
            {
                case TokenShape.Circle:
                    return dx * dx + dy * dy <= half * half;
                case TokenShape.Diamond:
                    return dx + dy <= half;
                case TokenShape.Square:
                    return dx <= half && dy <= half;
                case TokenShape.Triangle:
                    return y >= center.y - half && y <= center.y + half && dx <= (y - (center.y - half)) / size * half;
                case TokenShape.Cross:
                    return dx < half * 0.28f || dy < half * 0.28f;
                case TokenShape.Star:
                    return dx + dy <= half || dx < half * 0.22f || dy < half * 0.22f;
                default:
                    return false;
            }
        }

        private static void SaveSpriteTexture(string name, Texture2D texture, float pixelsPerUnit, Vector4 border)
        {
            texture.Apply();
            string path = SpriteFolder + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.spriteBorder = border;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            Sprites[name] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreatePanel(Transform parent, string name, Sprite sprite, Vector2 anchoredPosition, Vector2 size, bool active)
        {
            GameObject panel = CreateUIObject(name, parent, size);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            Image image = panel.AddComponent<Image>();
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.type = Image.Type.Sliced;
            panel.SetActive(active);
            return panel;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            return obj;
        }

        private static Image AddImageChild(Transform parent, string name, Sprite sprite, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject obj = CreateUIObject(name, parent, size);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            Image image = obj.AddComponent<Image>();
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.type = Image.Type.Sliced;
            return image;
        }

        private static Text AddTextChild(Transform parent, string name, string text, int fontSize, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment)
        {
            GameObject obj = CreateUIObject(name, parent, size);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            Text label = obj.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = new Color(0.16f, 0.08f, 0.03f);
            return label;
        }

        private static Button AddButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            return AddButton(parent, name, label, anchoredPosition, new Vector2(180f, 54f));
        }

        private static Button AddButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject obj = CreateUIObject(name, parent, size);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            Image image = obj.AddComponent<Image>();
            image.sprite = Sprites["UI_WoodButton"];
            image.type = Image.Type.Sliced;
            Button button = obj.AddComponent<Button>();
            Text text = AddTextChild(obj.transform, "Text", label, 18, Vector2.zero, size, TextAnchor.MiddleCenter);
            text.color = new Color(0.95f, 0.84f, 0.58f);
            return button;
        }

        private static void FillObjectList<T>(SerializedProperty property, List<T> values) where T : Object
        {
            property.ClearArray();
            for (int i = 0; i < values.Count; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetImageList(Object target, string fieldName, List<Image> images)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            property.ClearArray();
            for (int i = 0; i < images.Count; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetTextList(Object target, string fieldName, List<Text> texts)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            property.ClearArray();
            for (int i = 0; i < texts.Count; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = texts[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObject(Object target, string fieldName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetBool(Object target, string fieldName, bool value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.boolValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetInt(Object target, string fieldName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.intValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetVector2Int(Object target, string fieldName, Vector2Int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.vector2IntValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private sealed class BoardSceneRefs
        {
            public GameObject boardRoot;
            public Transform unitsRoot;
            public Tilemap terrainTilemap;
            public Tilemap decorationTilemap;
            public Tilemap highlightTilemap;
            public Tilemap overlayTilemap;
            public BoardManager boardManager;
            public BoardHighlighter boardHighlighter;
            public BoardInputController boardInputController;
            public BoardRandomGenerator boardRandomGenerator;
            public TokenSpawner tokenSpawner;
            public DeploymentZone deploymentZone;
        }

        private sealed class CardSceneRefs
        {
            public GameObject root;
            public CardPool cardPool;
            public HandManager handManager;
            public GoldManager goldManager;
            public APManager apManager;
            public CardPlayValidator cardPlayValidator;
            public CardDragController cardDragController;
        }

        private sealed class FoodSceneRefs
        {
            public GameObject root;
            public FoodBar foodBar;
        }

        private sealed class ComboSceneRefs
        {
            public GameObject root;
            public ComboManager comboManager;
            public ComboResolver comboResolver;
            public ComboTargetingController comboTargetingController;
        }

        private sealed class GameFlowSceneRefs
        {
            public GameObject root;
            public GameStateManager gameStateManager;
            public RunSetupManager runSetupManager;
            public TurnManager turnManager;
            public VictoryConditionManager victoryConditionManager;
            public GameFlowBootstrap bootstrap;
        }
    }
}
#endif
