using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerFlashlight : MonoBehaviour
    {
        private FlashlightUI flashlightUI;

        [Inject]
        public void Construct(FlashlightUI _flashlightUI)
        {
            flashlightUI = _flashlightUI;
        }

        [Header("Reveal Shader Settings")]
        [SerializeField] private float revealDistance = 15f;
        [SerializeField] private float revealAngle = 25f;
        [SerializeField] private float revealSmoothSpeed = 4f; // Скорость плавного появления
        [SerializeField] private float coneBackOffset = 1.5f; // Сдвиг конуса назад, чтобы видеть объекты вплотную

        [Header("Flashlight Base")]
        public Light flashlight;
        [SerializeField] private Animator anim;
        [SerializeField] private string trig_on = "enable";
        [SerializeField] private string trig_off = "disable";
        public bool light_active = false;

        [Header("Essence Settings")]
        [SerializeField] private float maxEssence = 99f;
        public float currentEssence = 5f;
        [SerializeField] private float essenceDrainPerSecond = 0.5f; // Сколько тратится в секунду

        private float currentShaderDistance = 0f;
        private int lastDisplayedEssence = -1; // Для оптимизации UI (чтобы не обновлять текст каждый кадр)

        // Публичные свойства для чтения другими скриптами
        public float RevealDistance => revealDistance;
        public float RevealAngle => revealAngle;

        private void Awake()
        {
            UpdateUI();

            // Если свет выключен со старта, сразу задаем шейдеру 0, чтобы без рывков
            currentShaderDistance = light_active ? revealDistance : 0f;
        }

        private void Update()
        {
            if (!flashlight.enabled) return;

            HandleInput();
            ProcessEssence();
            UpdateRevealShaderSmoothly();
        }

        private void HandleInput()
        {
            // Включаем, только если есть эссенция
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (light_active)
                {
                    TurnOffCrown();
                }
                else if (currentEssence > 0)
                {
                    TurnOnCrown();
                }
            }
        }

        private void ProcessEssence()
        {
            if (light_active)
            {
                // Плавно отнимаем эссенцию (работает стабильнее корутин)
                currentEssence -= essenceDrainPerSecond * Time.deltaTime;

                // Жесткий лимит от 0 до maxEssence
                currentEssence = Mathf.Clamp(currentEssence, 0f, maxEssence);

                UpdateUI();

                // Автоматическое выключение, если эссенция иссякла
                if (currentEssence <= 0)
                {
                    TurnOffCrown();
                }
            }
        }

        private void UpdateUI()
        {
            // Округляем в большую сторону, чтобы 0 показывался только когда ресурса реально нет
            int displayValue = Mathf.CeilToInt(currentEssence);

            // Оптимизация: меняем текст только если число реально изменилось
            if (displayValue != lastDisplayedEssence)
            {
                flashlightUI.SetText(displayValue.ToString());
                lastDisplayedEssence = displayValue;
            }
        }

        private void UpdateRevealShaderSmoothly()
        {
            // Целевая дистанция: если включен - revealDistance, если выключен - 0
            float targetDistance = light_active ? revealDistance : 0f;

            // Плавный переход
            currentShaderDistance = Mathf.Lerp(currentShaderDistance, targetDistance, Time.deltaTime * revealSmoothSpeed);

            // Тот самый хак для устранения "мертвой зоны" вплотную
            Vector3 virtualOrigin = flashlight.transform.position - (flashlight.transform.forward * coneBackOffset);

            Shader.SetGlobalVector("_CrownPos", virtualOrigin);
            Shader.SetGlobalVector("_CrownDir", flashlight.transform.forward.normalized);
            Shader.SetGlobalFloat("_CrownDistance", currentShaderDistance);

            float angleCos = Mathf.Cos(revealAngle * Mathf.Deg2Rad);
            Shader.SetGlobalFloat("_CrownAngle", angleCos);
        }

        public void TurnOnCrown()
        {
            flashlightUI.Open();
            anim.ResetTrigger(trig_off);
            anim.SetTrigger(trig_on);
            light_active = true;
        }

        public void TurnOffCrown()
        {
            flashlightUI.Close();
            anim.ResetTrigger(trig_on);
            anim.SetTrigger(trig_off);
            light_active = false;
        }

        // Публичный метод для добавления эссенции из других скриптов
        public void AddEssence(float amount)
        {
            currentEssence += amount;
            currentEssence = Mathf.Clamp(currentEssence, 0f, maxEssence);
            UpdateUI();
        }

        // Старые методы для совместимости
        public void TurnOn() => this.gameObject.SetActive(true);
        public void TurnOFF() => this.gameObject.SetActive(false);
        public bool IsActive() => this.gameObject.activeSelf;
        public void TurnOnLight() => flashlight.gameObject.SetActive(true);
        public void TurnOFFLight() => flashlight.gameObject.SetActive(false);
        public bool IsActiveLight() => flashlight.gameObject.activeSelf;

        public void StatueChase()
        {
            StatueController[] Statues = Object.FindObjectsByType<StatueController>(FindObjectsSortMode.None);
            foreach (StatueController statue in Statues)
            {
                statue.advance_pose();
            }
        }

        // Метод возвращает точку начала конуса (с учетом того самого отступа за спину)
        public Vector3 GetVirtualOrigin()
        {
            return flashlight.transform.position - (flashlight.transform.forward * coneBackOffset);
        }
    }
}