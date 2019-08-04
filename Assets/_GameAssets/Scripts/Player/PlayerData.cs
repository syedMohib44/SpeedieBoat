using UnityEngine;



// Holds playerdata such as currency and level progress, writes values to PlayerPrefs when they change
// Must be finally written when app is paused or quit

namespace SpeedyBoat
{
    public class PlayerData
    {
        public bool ShownHoldFlip
        {
            get { return m_shownHoldFlip; }
            set
            {
                if (m_shownHoldFlip != value)
                {
                    m_shownHoldFlip = value;
                    m_dirty = true;
                }
            }
        }



        public bool SoundOn
        {
            get { return m_soundOn; }
            set
            {
                if (m_soundOn != value)
                {
                    m_soundOn = value;
                    m_dirty = true;
                }
            }
        }



        public bool HapticOn
        {
            get { return m_hapticOn; }
            set
            {
                if (m_hapticOn != value)
                {
                    m_hapticOn = value;
                    m_dirty = true;
                }
            }
        }



        public int Coins
        {
            get { return m_coins; }
            set
            {
                if (m_coins != value)
                {
                    m_coins = value;
                    m_dirty = true;
                }
            }
        }



        public int BestScore
        {
            get { return m_bestScore; }
            set
            {
                if (m_bestScore != value)
                {
                    m_bestScore = value;
                    m_dirty = true;
                }
            }
        }



        public int LevelProgress
        {
            get { return m_levelProgress; }
            set
            {
                if (m_levelProgress != value)
                {
                    m_levelProgress = value;
                    m_dirty = true;
                }
            }
        }



        public PlayerData(Main main)
        {
            Deserialize();
        }



        public void Update()
        {
            if (m_dirty)
            {
                m_dirty = false;
                Serialize();
            }
        }



        public void Serialize()
        {
            var text =  m_coins + Delimeter +
                        m_bestScore + Delimeter +
                        m_levelProgress + Delimeter +
                        m_soundOn + Delimeter +
                        m_hapticOn + Delimeter +
                        m_shownHoldFlip + Delimeter;

            PlayerPrefs.SetString(PlayerPrefsKey, text);
            PlayerPrefs.Save();
        }



        private void Deserialize()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                var index = 0;
                var tokens = PlayerPrefs.GetString(PlayerPrefsKey).Split(Delimeter.ToCharArray());

                m_coins = int.Parse(tokens[index++]);
                m_bestScore = int.Parse(tokens[index++]);
                m_levelProgress = int.Parse(tokens[index++]);

                m_soundOn = bool.Parse(tokens[index++]);
                m_hapticOn = bool.Parse(tokens[index++]);

                m_shownHoldFlip = bool.Parse(tokens[index++]);
            }
            else
            {
                m_soundOn = m_hapticOn = true;
            }
        }



        private const string        Delimeter = ",";
        private const string        PlayerPrefsKey = "SpeedyBoat::PlayerPrefs";

        private bool                m_dirty;

        private int                 m_coins;
        private int                 m_bestScore;
        private int                 m_levelProgress;
        private bool                m_soundOn, m_hapticOn, m_shownHoldFlip;
    }
}