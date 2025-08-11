using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Quests.Core;
using _1.Scripts.Quests.Data;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.InGame.Quest;
using UnityEngine;

namespace _1.Scripts.UI.InGame.HUD
{
    public class QuestUI : UIBase
    {
        [SerializeField] private Transform questSlotContainer;
        [SerializeField] private GameObject questSlotPrefab;

        private readonly List<QuestSlot> questSlots = new();
        private List<QuestData> questListCache = new();
        private Dictionary<int, List<ObjectiveProgress>> objectiveDictCache = new();
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            
            LoadQuestData();
            SetQuestSlots();
            SetMainQuestNavigation();
            Refresh();
            gameObject.SetActive(false);
            Service.Log("Initialized Quest UI");
        }

        public override void ResetUI()
        {
            ClearAll();
            questListCache.Clear();
            objectiveDictCache.Clear();
        }
        
        private void LoadQuestData()
        {
            var questManager = CoreManager.Instance.questManager;
            questListCache = questManager.activeQuests.Values.Select(q => q.data).ToList();
            objectiveDictCache = questManager.activeQuests.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Objectives.Values.ToList()
            );
        }
        
        private void SetQuestSlots()
        {
            if (questSlots.Count > 0) ClearAll();
            foreach (var questData in questListCache)
            {
                var go = Instantiate(questSlotPrefab, questSlotContainer, false);
                if (!go.TryGetComponent(out QuestSlot questSlot)) { Destroy(go); return; }
                if (objectiveDictCache.TryGetValue(questData.questID, out var obj))
                {
                    questSlot.Initialize(questData, obj);
                    questSlots.Add(questSlot);
                }
                else Destroy(go);
            }
        }

        private void ClearAll()
        {
            foreach (var slot in questSlots.Where(slot => slot)) Destroy(slot.gameObject);
            questSlots.Clear();
        }

        public void Refresh()
        {
            for (int i = 0; i < questSlots.Count; i++)
            {
                var questSlot = questSlots[i];
                if (i >= questListCache.Count ||
                    !objectiveDictCache.TryGetValue(questListCache[i].questID, out var objectives)) continue;
                questSlot.RefreshObjectiveSlots();
            }
        }

        private void SetMainQuestNavigation()
        {
            var questManager = CoreManager.Instance.questManager;
            if (!questManager.activeQuests.TryGetValue(0, out var mainQuest)) return;
            var targetObjective = mainQuest.Objectives.Values.FirstOrDefault(obj => !obj.IsCompleted);
            if (targetObjective != null)
                QuestTargetBinder.Instance.SetCurrentTarget(targetObjective.data.targetID);
        }

        public void ToggleObjectiveSlot()
        {
            foreach (var slot in questSlots)
                slot.ToggleObjectiveSlot();
        }
    }
}
