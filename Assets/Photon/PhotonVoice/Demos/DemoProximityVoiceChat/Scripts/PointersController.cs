using Photon.Voice.PUN;
using UnityEngine;

[RequireComponent(typeof(PhotonVoiceView))]
public class PointersController : MonoBehaviour
{
    #pragma warning disable 649
    [SerializeField]
    private readonly GameObject pointerDown;
    [SerializeField]
    private readonly GameObject pointerUp;
    #pragma warning restore 649

    private PhotonVoiceView photonVoiceView;

    private void Awake()
    {
        this.photonVoiceView = this.GetComponent<PhotonVoiceView>();
        this.SetActiveSafe(this.pointerUp, false);
        this.SetActiveSafe(this.pointerDown, false);
    }

    private void Update()
    {
        this.SetActiveSafe(this.pointerDown, this.photonVoiceView.IsSpeaking);
        this.SetActiveSafe(this.pointerUp, this.photonVoiceView.IsRecording);
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
        {
            go.SetActive(active);
        }
    }
}
