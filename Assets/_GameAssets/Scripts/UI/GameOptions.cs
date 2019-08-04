using TapticPlugin;
using UnityEngine;
using UnityEngine.UI;



namespace SpeedyBoat
{
    public class GameOptions : MonoBehaviour
    {
        private void Start()
        {
            m_main = GameObject.Find("Main").GetComponent<Main>();
            m_hapticButton = transform.FindIncludingInactive("HapticButton").GetComponent<Button>();
            m_soundButton = transform.FindIncludingInactive("SoundButton").GetComponent<Button>();

            m_hapticButton.gameObject.SetActive(TapticManager.IsSupport());

            SetSound(m_main.PlayerData.SoundOn);
            SetHaptic(m_main.PlayerData.HapticOn);
        }



        private void Update()
        {
            //m_restoreIAPButton.interactable = !m_main.StoreInterface.IsRestoringProducts;
        }



        public void ToggleOptions()
        {
            var animator = GetComponent<Animator>();
            var anim = animator.GetCurrentAnimatorStateInfo(0);

            if (anim.IsName("OptionsClosed"))
            {
                animator.SetTrigger("Open");
            }
            else if (anim.IsName("OptionsOpen"))
            {
                animator.SetTrigger("Close");
            }
        }



        public void ToggleHaptic()
        {
            m_main.PlayerData.HapticOn = !m_main.PlayerData.HapticOn;
            SetHaptic(m_main.PlayerData.HapticOn);
        }



        public void ToggleSound()
        {
            m_main.PlayerData.SoundOn = !m_main.PlayerData.SoundOn;
            SetSound(m_main.PlayerData.SoundOn);

            AudioListener.volume = m_main.PlayerData.SoundOn ? 1 : 0;
        }



        public void RestoreIAPClick()
        {
            /*
            if (!m_main.StoreInterface.IsRestoringProducts)
            {
                m_main.StoreInterface.RestorePurchases();
            }
            */
        }



        private void SetSound(bool on)
        {
            m_soundButton.image.sprite = on ? GameSettings.Instance().SoundOn : GameSettings.Instance().SoundOff;
            m_main.PlayerData.SoundOn = on;
        }



        private void SetHaptic(bool on)
        {
            m_hapticButton.image.sprite = on ? GameSettings.Instance().HapticOn : GameSettings.Instance().HapticOff;
            m_main.PlayerData.HapticOn = on;
        }



        private Main    m_main;
        private Button  m_soundButton, m_hapticButton;
    }
}