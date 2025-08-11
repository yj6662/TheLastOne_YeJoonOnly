using System;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame.Mission;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.HUD
{
    public class DistanceUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private PathAnimator pathAnimator;

        private Transform player;
        private Transform target;

        public static Transform CurrentTarget { get; private set; }
        public static event Action<Transform> OnTargetChanged;
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            SetTarget(null);
            gameObject.SetActive(false);
        }
        
        public override void ResetUI() { SetTarget(null); }
        
        public void SetTarget(Transform newTarget)
        {
            player = CoreManager.Instance.gameManager.Player.transform;
            target = newTarget;
            CurrentTarget = newTarget;
            OnTargetChanged?.Invoke(newTarget);

            if (CoreManager.Instance.uiManager.IsCutscene) return;
            
            if (newTarget) Show();
            else Hide();
        }
        
        private void Update()
        {
            if (!player || !target)
            {
                if (gameObject.activeSelf) Hide();
                return;
            }
            
            float distance = Vector3.Distance(player.position, target.position);
            distanceText.text = $"{distance:0.0}m";
        }
    }
     
}