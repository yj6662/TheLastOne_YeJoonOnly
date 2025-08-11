using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Minigame
{
    public class AlphabetCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Animator animator;

        public void SetChar(char c)
        {
            text.text = c.ToString();
        }

        public void PlayCorrectAnim()
        {
            if (animator) animator.SetTrigger("Correct");
        }

        public void PlayWrongAnim()
        {
            if (animator) animator.SetTrigger("Wrong");
        }

        public void ResetCell()
        {
            text.text = "";
            if (animator) animator.Rebind();
        }
    }
}