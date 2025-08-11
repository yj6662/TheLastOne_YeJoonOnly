using UnityEngine;

namespace _1.Scripts.UI.InGame.Minigame
{
    public class WireConnectionUI : UIBase
    {
        [field: Header("Wire Connection UI")]
        [field: SerializeField] public RectTransform Panel { get; private set; }
        [field: SerializeField] public RectTransform Top { get; private set; }
        [field: SerializeField] public RectTransform Bottom { get; private set; }
        [field: SerializeField] public RectTransform WireContainer { get; private set; }

        private void Reset()
        {
            if (!Panel) Panel = this.TryGetComponent<RectTransform>();
            if (!Top) Top = this.TryGetChildComponent<RectTransform>("Top");
            if (!Bottom) Bottom = this.TryGetChildComponent<RectTransform>("Bottom");
            if (!WireContainer) WireContainer = this.TryGetChildComponent<RectTransform>("WireContainer");
        }
    }
}