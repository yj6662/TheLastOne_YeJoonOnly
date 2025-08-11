using UnityEngine;

namespace _1.Scripts.UI.InGame.Minigame
{
    public class ChargeBarUI : UIBase
    {
        [field: Header("Charge Bar UI")]
        [field: SerializeField] public RectTransform Panel { get; private set; }
        [field: SerializeField] public RectTransform BarLayout { get; private set; }
        [field: SerializeField] public RectTransform ControlLayout { get; private set; }
        [field: SerializeField] public RectTransform TargetObj { get; private set; }
        [field: SerializeField] public RectTransform ControlObj { get; private set; }

        private void Reset()
        {
            if (!Panel) Panel = this.TryGetComponent<RectTransform>();
            if (!BarLayout) BarLayout = this.TryGetChildComponent<RectTransform>("BarLayout");
            if (!ControlLayout) ControlLayout = this.TryGetChildComponent<RectTransform>("ControlLayout");
            if (!TargetObj) TargetObj = this.TryGetChildComponent<RectTransform>("TargetObj");
            if (!ControlObj) ControlObj = this.TryGetChildComponent<RectTransform>("ControlObj");
        }
    }
}