using Academy.HoloToolkit.Sharing;
using Academy.HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class HologramPlacement : Singleton<HologramPlacement>
{
    /// <summary>
    /// Tracks if we have been sent a transform for the model.
    /// The model is rendered relative to the actual anchor.
    /// </summary>
    public bool GotTransform { get; private set; }

    private Vector3 startPosition;

    public float cameraDistanceZ = 5.5f;
    public float cameraDistanceY = -1f;

    void Start()
    {

        // We care about getting updates for the anchor transform.
        CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.StageTransform] = this.OnStageTransfrom;

        // And when a new user join we will send the anchor transform we have.
        SharingSessionTracker.Instance.SessionJoined += Instance_SessionJoined;

        //// Start by making the model as the cursor.
        //// So the user can put the hologram where they want.
        //GestureManager.Instance.OverrideFocusedObject = this.gameObject;
        startPosition = transform.position;
    }

    /// <summary>
    /// When a new user joins we want to send them the relative transform for the anchor if we have it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Instance_SessionJoined(object sender, SharingSessionTracker.SessionJoinedEventArgs e)
    {
        if (GotTransform)
        {
            CustomMessages.Instance.SendStageTransform(transform.localPosition, transform.localRotation);
        }
    }

    void Update()
    {
        if (GotTransform)
        {
            if (ImportExportAnchorManager.Instance.AnchorEstablished)
            {
                // Do something when an anchor is established to show it to the user
            }
        }
        else
        {
            Vector3 proposed = ProposeTransformPosition();
            //transform.position = Vector3.Lerp(transform.position, new Vector3(proposed.x, startPosition.y + proposed.y, startPosition.z), 0.2f);
            transform.position = Vector3.Lerp(transform.position, proposed, 0.2f);

        }
    }

    Vector3 ProposeTransformPosition()
    {
        // Put the model cameraDistanceZ meter in front of the user.
        Vector3 retval = Camera.main.transform.position + Camera.main.transform.forward * cameraDistanceZ;

        return retval;
    }

    public void OnSelect()
    {
        // Note that we have a transform.
        GotTransform = true;

        // And send it to our friends.
        CustomMessages.Instance.SendStageTransform(transform.localPosition, transform.localRotation);
    }

    /// <summary>
    /// When a remote system has a transform for us, we'll get it here.
    /// </summary>
    /// <param name="msg"></param>
    void OnStageTransfrom(NetworkInMessage msg)
    {
        // We read the user ID but we don't use it here.
        msg.ReadInt64();

        transform.localPosition = CustomMessages.Instance.ReadVector3(msg);
        transform.localRotation = CustomMessages.Instance.ReadQuaternion(msg);

        //// The first time, we'll want to send the message to the anchor to do its animation and
        //// swap its materials.
        //if (GotTransform == false)
        //{
        //    GetComponent<EnergyHubBase>().SendMessage("OnSelect");
        //}

        GotTransform = true;
    }

    public void ResetStage()
    {
        // We'll use this later.
    }
}