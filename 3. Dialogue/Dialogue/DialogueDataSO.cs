using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Dialogue
{
    [CreateAssetMenu(menuName = "ScriptableObjects/DialogueDataSO", fileName = "DialogueDataSO")]
    public class DialogueDataSO : ScriptableObject
    {
        public int dialogueKey;
        public List<DialogueData> sequence;
    }
}