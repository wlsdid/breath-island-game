using System.Collections.Generic;
using UnityEngine;

namespace BreathIsland
{
    public sealed class DefinitionCatalog
    {
        public List<FishDefinition> Fish { get; } = new();
        public Dictionary<FacilityId, FacilityDefinition> Facilities { get; } = new();
        public Dictionary<NpcId, NpcDefinition> Npcs { get; } = new();

        public static DefinitionCatalog CreateVerticalSlice()
        {
            var catalog = new DefinitionCatalog();
            catalog.Fish.Add(CreateFish("glimmer", "윤슬멸치", "Glimmer Anchovy", "햇빛을 따라 반짝이는 작은 물고기", "A small fish that follows sunlit ripples.", FishRarity.Common, new Color(.35f, .88f, .82f), 18, 1, 4f, 6, 18));
            catalog.Fish.Add(CreateFish("cloud", "구름복어", "Cloud Puffer", "놀라면 몽글몽글 부풀어 오른다", "It puffs up like a tiny cloud.", FishRarity.Uncommon, new Color(1f, .78f, .35f), 28, 2, 6f, 7, 20));
            catalog.Fish.Add(CreateFish("coral", "산호나비어", "Coral Butterfly", "얕은 산호 곁을 맴도는 온순한 물고기", "A gentle fish that circles shallow coral.", FishRarity.Uncommon, new Color(1f, .43f, .35f), 34, 2, 7f, 8, 17));
            catalog.Fish.Add(CreateFish("moon", "달빛가오리", "Moon Ray", "저녁 바다에서 푸른 빛을 남긴다", "It leaves a blue trail across evening water.", FishRarity.Rare, new Color(.5f, .48f, 1f), 56, 4, 9f, 17, 23));
            catalog.Fish.Add(CreateFish("pearl", "진주해마", "Pearl Seahorse", "해초 사이에 몸을 숨기는 희귀종", "A rare creature hiding among sea grass.", FishRarity.Rare, new Color(.95f, .55f, .76f), 64, 5, 10f, 6, 23));
            catalog.Fish.Add(CreateFish("breaker", "파도송곳상어", "Breaker Shark", "거친 파도를 가르는 외해의 포식자", "An open-sea predator that cuts through rough waves.", FishRarity.Predator, new Color(.2f, .55f, .66f), 0, 9, 13f, 0, 23));
            catalog.Facilities[FacilityId.Boatyard] = CreateFacility(FacilityId.Boatyard, "바람 조선소", "Wind Boatyard");
            catalog.Facilities[FacilityId.Aquarium] = CreateFacility(FacilityId.Aquarium, "파도 수족관", "Tide Aquarium");
            catalog.Npcs[NpcId.Nari] = CreateNpc(NpcId.Nari, "나리", "Nari", "조선공", "Boatwright", new Color(.15f, .65f, .58f),
                new[] { "바다는 기다릴 줄 아는 사람을 좋아해.", "조선소를 손보면 산호초까지 갈 수 있어.", "오늘 파도는 얌전하네. 줄 소리에 귀 기울여 봐." },
                new[] { "The sea likes people who know how to wait.", "Repair the boatyard and we can reach the reef.", "The waves are gentle today. Listen to your line." });
            catalog.Npcs[NpcId.Maru] = CreateNpc(NpcId.Maru, "마루", "Maru", "수족관지기", "Aquarium Keeper", new Color(.95f, .64f, .31f),
                new[] { "처음 만난 생물은 도감에 기록해 둬.", "수족관이 밝아지면 마을 사람들도 찾아올 거야.", "구름복어는 둥글지만 마음은 아주 용감하대." },
                new[] { "Record every creature you meet for the first time.", "A brighter aquarium will bring people back.", "Cloud puffers are round, but very brave." });
            return catalog;
        }

        private static FishDefinition CreateFish(string id, string ko, string en, string koDesc, string enDesc, FishRarity rarity, Color color, int coin, int material, float pull, int start, int end)
        {
            var value = ScriptableObject.CreateInstance<FishDefinition>();
            value.id = id; value.koreanName = ko; value.englishName = en; value.koreanDescription = koDesc; value.englishDescription = enDesc;
            value.rarity = rarity; value.color = color; value.coinValue = coin; value.materialValue = material; value.pullStrength = pull; value.startHour = start; value.endHour = end;
            return value;
        }

        private static FacilityDefinition CreateFacility(FacilityId id, string ko, string en)
        {
            var value = ScriptableObject.CreateInstance<FacilityDefinition>();
            value.id = id; value.koreanName = ko; value.englishName = en;
            return value;
        }

        private static NpcDefinition CreateNpc(NpcId id, string ko, string en, string koRole, string enRole, Color color, string[] koLines, string[] enLines)
        {
            var value = ScriptableObject.CreateInstance<NpcDefinition>();
            value.id = id; value.koreanName = ko; value.englishName = en; value.koreanRole = koRole; value.englishRole = enRole;
            value.color = color; value.koreanLines = koLines; value.englishLines = enLines;
            return value;
        }
    }
}
