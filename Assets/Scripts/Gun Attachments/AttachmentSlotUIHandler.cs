using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttachmentSlotUIHandler : MonoBehaviour
{
    public AttachmentType attachmentType;
    public Button LeftArrowButton, RightArrowButton;
    public TextMeshProUGUI attachmentText;

    private void Start()
    {
        LeftArrowButton?.onClick.AddListener(() => Cycle(-1));
        RightArrowButton?.onClick.AddListener(() => Cycle(1));
    }

    void Cycle(int direction)
    {
        var data = GunAttachmentSpawnManager.Instance.CycleAttachment(attachmentType, direction);

        if (data != null)
        {
            attachmentText.text = data.attachmentName;
        }
    }
}
