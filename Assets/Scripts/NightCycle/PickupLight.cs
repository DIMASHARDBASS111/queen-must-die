using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PickupLight : MonoBehaviour
    {
        [SerializeField] bool destroyOnPickup = true;
        [Inject] PlayerFlashlight flashlight;
        public void Pickup()
        {
            flashlight.TurnOn();
            flashlight.light_active = true;
            flashlight.Start_Essense_Decrease();

            if (destroyOnPickup)
            {
                gameObject.SetActive(false);
                destroyOnPickup = false;
            }
        }

        public void ChangeEssense(int value)
        {
            flashlight.Essense += value;

            flashlight.Essense = Mathf.Clamp(flashlight.Essense, 0, 99);

        }

    }
}
