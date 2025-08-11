using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame.HUD;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    [Serializable] public class QuestTargetBinding
    {
        public int questID;
        public Transform target;
    }
    
    public class QuestTargetBinder : MonoBehaviour
    {
        [Header("Quest Target Bindings")]
        [SerializeField] private List<QuestTargetBinding> bindings;
        
        private QuestManager questManager;
        
        // Singleton
        public static QuestTargetBinder Instance { get; private set; }

        private void Awake()
        {
            if (!Instance) Instance = this;
            else { if(Instance != this) Destroy(gameObject); }
        }
        
        public void SetCurrentTarget(int questId)
        {
            var distanceUI = CoreManager.Instance.uiManager.GetUI<DistanceUI>();
            
            var target = bindings.FirstOrDefault(x => x.questID == questId)?.target;
            distanceUI?.SetTarget(target ? target : null);
        }
    }
}