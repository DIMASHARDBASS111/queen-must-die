using System.Collections;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerFlashlight : MonoBehaviour
    {

        //test
        //[Inject] private SaveSystem _saveSystem;
        [Header("Reveal Shader Settings")]
        [SerializeField] private float revealDistance = 15f;
        [SerializeField] private float revealAngle = 25f;
        //test
        //[SerializeField] Light flashlight;
        public Light flashlight;
        private bool can_shimmer = false;
        [SerializeField] float baseIntensity = 1.0f;

        [SerializeField] float swayAmount = 0.04f;
        [SerializeField] float swaySmooth = 8f;

        [SerializeField] Animator anim;
        [SerializeField] string trig_on;
        [SerializeField] string trig_off;

        public bool light_active = false;

        public int Essense = 5;
        public int decreaseStep = -1;
        public float decreaseTime = 2.0f;

        Vector3 initialLocalPos;


        void Awake()
        {
            initialLocalPos = transform.localPosition;
            baseIntensity = flashlight.intensity;
        }

        private void UpdateRevealShader()
        {
            // используем light_active, чтобы понимать, включена ли корона
            if (light_active)
            {

                // ѕередаем мировые координаты и вектор направлени€ пр€мо от объекта фонар€
                Shader.SetGlobalVector("_CrownPos", flashlight.transform.position);
                Shader.SetGlobalVector("_CrownDir", flashlight.transform.forward.normalized);
                Shader.SetGlobalFloat("_CrownDistance", revealDistance);

                // Dot Product в шейдере оперирует косинусами. 
                // „тобы видеокарте не приходилось считать углы, мы считаем косинус один раз на процессоре.
                float angleCos = Mathf.Cos(revealAngle * Mathf.Deg2Rad);
                Shader.SetGlobalFloat("_CrownAngle", angleCos);

                //Debug.Log($"[Shader Debug] Pos: {Shader.GetGlobalVector("_CrownPos")}, Dist: {Shader.GetGlobalFloat("_CrownDistance")}, AngleCos: {Shader.GetGlobalFloat("_CrownAngle")}");
            }
            else
            {
                // ≈сли корона выключена, обнул€ем дистанцию про€влени€, скрыва€ всех монстров и руны
                Shader.SetGlobalFloat("_CrownDistance", 0f);
            }
        }
        private void Update()
        {
            Debug.Log(Essense);

            Debug.Log(flashlight.intensity);
            if (!flashlight.enabled) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            CheckEssense();

            if (Input.GetKeyDown(KeyCode.F) && Essense > 0)
            {
                //if (IsActiveLight())
                if(light_active)
                {
                    
                    play_disable();
                    //can_shimmer = false;
                    light_active = false;
                    Stop_Essense_Decrease();
                }
                else
                {
                    //TurnOnLight();
                    //Debug.Log("en");
                    play_enable();
                    //can_shimmer = true;
                    light_active = true;
                    Start_Essense_Decrease();
                }
            }

            UpdateRevealShader();

        }

        private void play_enable()
        {
            anim.ResetTrigger(trig_off);
            anim.SetTrigger(trig_on);
        }

        private void play_disable()
        {
            //Debug.Log("%^%^%^%^%^%^%^%^%%%%%%%%%%%%%%%%%%%%%%%");
            anim.ResetTrigger(trig_on);
            anim.SetTrigger(trig_off);
        }

        public void TurnOn()
        {
            this.gameObject.SetActive(true);
        }

        public void TurnOFF()
        {
            this.gameObject.SetActive(false);
        }

        public bool IsActive()
        {
            return this.gameObject.activeSelf;
        }

        public void TurnOnLight()
        {
            flashlight.gameObject.SetActive(true);
        }

        public void TurnOFFLight()
        {
            flashlight.gameObject.SetActive(false);
        }

        public bool IsActiveLight()
        {
            return flashlight.gameObject.activeSelf;
        }

        public void StatueChase()
        {
            StatueController[] Statues = Object.FindObjectsByType<StatueController>(FindObjectsSortMode.None);

            foreach (StatueController statue in Statues)
            {
                statue.advance_pose();
            }

        }

        public void Start_Essense_Decrease()
        {
            StartCoroutine(EssenseRoutine(Essense, decreaseStep, decreaseTime));
        }

        public void Stop_Essense_Decrease()
        {
            StopCoroutine(EssenseRoutine(Essense, decreaseStep, decreaseTime));
        }

        private IEnumerator EssenseRoutine(int startAmount, int step, float changeTime)
        {

            while (light_active)
            {
                startAmount = Essense;

                float elapsedTime = 0f;
                int endAmount = startAmount + step;

                while (elapsedTime < changeTime)
                {
                    elapsedTime += Time.deltaTime;

                    float progress = elapsedTime / changeTime;

                    Essense = Mathf.RoundToInt(Mathf.Lerp(startAmount, endAmount, progress));

                    yield return null;
                }

                Essense = endAmount;

            }

        }

        private void CheckEssense()
        {
            if(Essense <= 0 && light_active)
            {
                play_disable();
                light_active = false;
                Stop_Essense_Decrease();
                Essense = 0;
            }
        }

    }
}
