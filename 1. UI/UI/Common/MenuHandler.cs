using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.UI.Common
{
    public class MenuHandler : MonoBehaviour
    {
        [SerializeField] private PauseHandler pauseHandler;
        
        public void SetPauseHandler(PauseHandler handler) => pauseHandler = handler;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) pauseHandler?.TogglePause();
        }
    }
}