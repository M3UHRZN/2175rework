using UnityEngine;
using UnityEngine.Events;
using TMPro;
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
        [SerializeField] private TMP_Text masterValueLabel;
        [SerializeField] private TMP_Text musicValueLabel;
        [SerializeField] private TMP_Text sfxValueLabel;

        private AudioSettingsManager Manager => AudioSettingsManager.Instance;

        private void OnEnable()
        {
            EnsureBindings();
            RefreshSliders();
        }

        public void Initialize(Slider master, Slider music, Slider sfx)
        {
            masterSlider = master;
            musicSlider = music;
            sfxSlider = sfx;
            EnsureBindings();
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
                UpdateValueLabel(masterValueLabel, Manager.MasterVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(Manager.MusicVolume);
                UpdateValueLabel(musicValueLabel, Manager.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(Manager.SfxVolume);
                UpdateValueLabel(sfxValueLabel, Manager.SfxVolume);
            }
        }

        public void OnMasterChanged(float value)
        {
            Manager?.SetMasterVolume(value);
            RefreshDependentSliders();
            UpdateValueLabel(masterValueLabel, value);
        }

        public void OnMusicChanged(float value)
        {
            Manager?.SetMusicVolume(value);
            UpdateValueLabel(musicValueLabel, value);
        }

        public void OnSfxChanged(float value)
        {
            Manager?.SetSfxVolume(value);
            UpdateValueLabel(sfxValueLabel, value);
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
                UpdateValueLabel(musicValueLabel, Manager.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(Manager.SfxVolume);
                UpdateValueLabel(sfxValueLabel, Manager.SfxVolume);
            }
        }

        private void NotifySliders()
        {
            NotifySlider(masterSlider);
            NotifySlider(musicSlider);
            NotifySlider(sfxSlider);
        }

        private void EnsureBindings()
        {
            CacheValueLabels();
            BindSlider(masterSlider, OnMasterChanged);
            BindSlider(musicSlider, OnMusicChanged);
            BindSlider(sfxSlider, OnSfxChanged);
        }

        private void CacheValueLabels()
        {
            masterValueLabel ??= FindValueLabel(masterSlider);
            musicValueLabel ??= FindValueLabel(musicSlider);
            sfxValueLabel ??= FindValueLabel(sfxSlider);
        }

        private static void BindSlider(Slider slider, UnityAction<float> callback)
        {
            if (slider == null)
            {
                return;
            }

            slider.onValueChanged.RemoveListener(callback);
            slider.onValueChanged.AddListener(callback);
        }

        private static void NotifySlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.onValueChanged.Invoke(slider.value);
        }

        private static void UpdateValueLabel(TMP_Text label, float value)
        {
            if (label == null)
            {
                return;
            }

            label.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        private static TMP_Text FindValueLabel(Slider slider)
        {
            if (slider == null)
            {
                return null;
            }

            var parent = slider.transform.parent;
            if (parent == null)
            {
                return null;
            }

            var expectedName = slider.gameObject.name.Replace(" Slider", " Value");
            var candidate = parent.Find(expectedName);
            return candidate != null ? candidate.GetComponent<TMP_Text>() : null;
        }
    }
}
