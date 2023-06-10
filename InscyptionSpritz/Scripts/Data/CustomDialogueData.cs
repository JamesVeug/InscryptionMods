using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace SpritzMod.Scripts.Data
{
    [Serializable]
    public class CustomDialogueDataMessage
    {
        public string message;
        public Emotion emotion;
        public TextDisplayer.LetterAnimation letterAnimation;
    }
    
    [Serializable]
    public class CustomDialogueData : AData
    {
        [SerializeField]
        public List<CustomDialogueDataMessage> dialogue = new List<CustomDialogueDataMessage>()
        {
            new CustomDialogueDataMessage()
        };
    }
}