using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Minigame
{
    public class AlphabetMatchingUI : UIBase
    {
        [Header("AlphabetMatchingUI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform cellRoot;
        [SerializeField] private GameObject cellPrefab;
        
        private List<AlphabetCell> cells = new();
        
        public override void Hide() { base.Hide(); HideAll(); }
        
        public void CreateAlphabet(string s)
        {
            foreach (var cell in cells)
                Destroy(cell.gameObject);
            cells.Clear();
            foreach (var t in s)
            {
                var go = Instantiate(cellPrefab, cellRoot);
                if (!go.TryGetComponent(out AlphabetCell cell)) continue;
                cell.SetChar(t);
                cells.Add(cell);
            }
        }

        public void ShowAlphabet(bool active)
        {
            foreach (var cell in cells) cell.gameObject.SetActive(active);
        }
        
        public void AlphabetAnim(int index, bool correct)
        {
            if (index < 0 || index >= cells.Count) return;
            
            if (correct) cells[index].PlayCorrectAnim();
            else cells[index].PlayWrongAnim();
        }
        
        private void HideAll()
        {
            foreach (var cell in cells) Destroy(cell.gameObject);
            cells.Clear();
        }
    }
}