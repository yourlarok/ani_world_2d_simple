using System;
using System.Collections.Generic;
using AniWorld.Cards.Data;
using UnityEngine;

namespace AniWorld.Tokens
{
    [Serializable]
    public class ClassTokenVisualEntry
    {
        public ClassType classType;
        public Sprite tokenSprite;
        public GameToken tokenPrefab;
        public Sprite classIcon;
    }

    [CreateAssetMenu(fileName = "ClassTokenVisualDatabase", menuName = "FatBallKingdom/Class Token Visual Database")]
    public class ClassTokenVisualDatabase : ScriptableObject
    {
        [SerializeField] private List<ClassTokenVisualEntry> entries = new List<ClassTokenVisualEntry>();

        public Sprite GetTokenSprite(ClassType classType)
        {
            ClassTokenVisualEntry entry = GetEntry(classType);
            return entry != null ? entry.tokenSprite : null;
        }

        public GameToken GetTokenPrefab(ClassType classType)
        {
            ClassTokenVisualEntry entry = GetEntry(classType);
            return entry != null ? entry.tokenPrefab : null;
        }

        public Sprite GetClassIcon(ClassType classType)
        {
            ClassTokenVisualEntry entry = GetEntry(classType);
            return entry != null ? entry.classIcon : null;
        }

        private ClassTokenVisualEntry GetEntry(ClassType classType)
        {
            foreach (ClassTokenVisualEntry entry in entries)
            {
                if (entry != null && entry.classType == classType)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
