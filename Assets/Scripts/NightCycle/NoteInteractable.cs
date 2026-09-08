using Core;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    public class NoteInteractable : MonoBehaviour, IFocusable
    {
        public Sprite noteImage;

        [TextArea(5, 20)]
        public string[] noteText;

        public bool trigger_once = true;
        public UnityEvent noteEvent;
        public EventReference interact_sound;
        public bool trigger_sound_once = false;
        private bool soundFlag = true;

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void SetEvent(bool trigger)
        {
            this.trigger_once = trigger;
        }

        public void OnEnterFocus()
        {
            if (soundFlag)
            {
                _audioService.PlayFMODEvent(interact_sound, transform.position);
                if (trigger_sound_once)
                {
                    soundFlag = false;
                }
            }
            NoteController.Instance.ShowNote(this);
        }

        public void OnExitFocus()
        {
            NoteController.Instance.CloseNote();
            TriggerNoteEvent();
        }

        public void TriggerNoteEvent()
        {
            if (trigger_once)
            {
                noteEvent?.Invoke();
                trigger_once = false;
            }
        }

    }
}