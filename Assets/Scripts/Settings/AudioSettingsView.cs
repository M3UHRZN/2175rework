using UnityEngine;
using UnityEngine.UI;

namespace Game.Settings
{
    /// <summary>
    /// Connects UI sliders to the <see cref="AudioSettingsManager"/>.
    /// </summary>
    public class AudioSettingsView : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private AudioSettingsManager Manager => AudioSettingsManager.Instance;

        private void OnEnable()
        {
            RefreshSliders();
        }

        public void Initialize(Slider master, Slider music, Slider sfx)
        {
            masterSlider = master;
            musicSlider = music;
            sfxSlider = sfx;
            RefreshSliders();
            NotifySliders();
        }

        public void RefreshSliders()
        {
            if (Manager == null)
            {
                return;
            }

            if (masterSlider != null)
            {
                masterSlider.SetValueWithoutNotify(Manager.MasterVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(Manager.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(Manager.SfxVolume);
            }
        }

        public void OnMasterChanged(float value)
        {
            Manager?.SetMasterVolume(value);
            RefreshDependentSliders();
        }

        public void OnMusicChanged(float value)
        {
            Manager?.SetMusicVolume(value);
        }

        public void OnSfxChanged(float value)
        {
            Manager?.SetSfxVolume(value);
        }

        private void RefreshDependentSliders()
        {
            if (Manager == null)
            {
                return;
            }

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(Manager.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(Manager.SfxVolume);
            }
        }

        private void NotifySliders()
        {
            NotifySlider(masterSlider);
            NotifySlider(musicSlider);
            NotifySlider(sfxSlider);
        }

        private static void NotifySlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.onValueChanged.Invoke(slider.value);
        }
    }
}
