namespace Photon.Voice.Unity.Demos
{
    using UnityEngine;
    using UnityEngine.UI;

    public class BackgroundMusicController : MonoBehaviour
    {
        #pragma warning disable 649
        [SerializeField]
        private readonly Text volumeText;
        [SerializeField]
        private readonly Slider volumeSlider;
        [SerializeField]
        private readonly AudioSource audioSource;
        [SerializeField]
        private readonly float initialVolume = 0.125f;
        #pragma warning restore 649

        private void Awake()
        {
            this.volumeSlider.minValue = 0f;
            this.volumeSlider.maxValue = 1f;
            this.volumeSlider.SetSingleOnValueChangedCallback(this.OnVolumeChanged);
            this.volumeSlider.value = this.initialVolume;
            this.OnVolumeChanged(this.initialVolume);
        }

        private void OnVolumeChanged(float newValue)
        {
            this.volumeText.text = string.Format("BG Volume: {0:0.###}", newValue);
            this.audioSource.volume = newValue;
        }
    }
}