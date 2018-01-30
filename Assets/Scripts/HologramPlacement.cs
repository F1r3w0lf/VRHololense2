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

    private bool disabledMap = false;

    ///// <summary>
    ///// We use a voice command to enable moving the target.
    ///// </summary>
    //KeywordRecognizer keywordRecognizer;

    private Vector3 startPosition;

    public float cameraDistanceZ = 1f;
    public float cameraDistanceY = -1f;

    void Start()
    {
        // When we first start, we need to disable the model to avoid it obstructing the user picking a hat.
        DisableModel();

        // We care about getting updates for the anchor transform.
        CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.StageTransform] = this.OnStageTransfrom;

        // And when a new user join we will send the anchor transform we have.
        SharingSessionTracker.Instance.SessionJoined += Instance_SessionJoined;

        // And if the users want to reset the stage transform.
        CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.ResetStage] = this.OnResetStage;

        // Setup a keyword recognizer to enable resetting the target location.
        //List<string> keywords = new List<string>();
        //keywords.Add("Move Map");
        //keywordRecognizer = new KeywordRecognizer(keywords.ToArray());
        //keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        //keywordRecognizer.Start();

        startPosition = transform.position;
    }

    ///// <summary>
    ///// When the keyword recognizer hears a command this will be called.  
    ///// In this case we only have one keyword, which will re-enable moving the 
    ///// target.
    ///// </summary>
    ///// <param name="args">information to help route the voice command.</param>
    //private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    //{
    //    Debug.Log(args.text);
    //    ResetStage();
    //}

    /// <summary>
    /// Resets the stage transform, so users can place the target again.
    /// </summary>
    public void ResetStage()
    {
        GotTransform = false;

        // AppStateManager needs to know about this so that
        // the right objects get input routed to them.
        AppStateManager.Instance.ResetStage();

        // Other devices in the experience need to know about this as well.
        CustomMessages.Instance.SendResetStage();
    }

    /// <summary>
    /// When a remote system has a transform for us, we'll get it here.
    /// </summary>
    void OnResetStage(NetworkInMessage msg)
    {
        GotTransform = false;
        AppStateManager.Instance.ResetStage();
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

    /// <summary>
    /// Turns off all renderers for the model.
    /// </summary>
    void DisableModel()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        disabledMap = true;
        //foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        //{
        //    if (renderer.enabled)
        //    {
        //        renderer.enabled = false;
        //        disabledRenderers.Add(renderer);
        //    }
        //}

        //foreach (MeshCollider collider in gameObject.GetComponentsInChildren<MeshCollider>())
        //{
        //    collider.enabled = false;
        //}
    }

    /// <summary>
    /// Turns on all renderers that were disabled.
    /// </summary>
    void EnableModel()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        disabledMap = false;

        //foreach (MeshRenderer renderer in disabledRenderers)
        //{
        //    renderer.enabled = true;
        //}

        //foreach (MeshCollider collider in gameObject.GetComponentsInChildren<MeshCollider>())
        //{
        //    collider.enabled = true;
        //}

        //disabledRenderers.Clear();
    }

    void Update()
    {
        if (disabledMap)
        {
            if (!PlayerAvatarStore.Instance.PickerActive &&
                ImportExportAnchorManager.Instance.AnchorEstablished)
            {
                // After which we want to start rendering.
                EnableModel();

                // And if we've already been sent the relative transform, we will use it.
                if (GotTransform)
                {
                    // Do something when the Anchor is set
                }
            }
        }
        else if (GotTransform == false)
        {
            Vector3 proposed = ProposeTransformPosition();
            //transform.position = Vector3.Lerp(transform.position, new Vector3(proposed.x, startPosition.y + proposed.y, startPosition.z), 0.2f);
            transform.position = Vector3.Lerp(transform.position, proposed, 0.2f);

        }
    }

    Vector3 ProposeTransformPosition()
    {
        Vector3 retval;
        // We need to know how many users are in the experience with good transforms.
        Vector3 cumulatedPosition = Camera.main.transform.position;
        int playerCount = 1;
        foreach (RemotePlayerManager.RemoteHeadInfo remoteHead in RemotePlayerManager.Instance.remoteHeadInfos)
        {
            if (remoteHead.Anchored && remoteHead.Active)
            {
                playerCount++;
                cumulatedPosition += remoteHead.HeadObject.transform.position;
            }
        }

        // If we have more than one player ...
        if (playerCount > 1)
        {
            // Put the transform in between the players.
            retval = cumulatedPosition / playerCount;
            RaycastHit hitInfo;

            // And try to put the transform on a surface below the midpoint of the players.
            if (Physics.Raycast(retval, Vector3.down, out hitInfo, 5, SpatialMappingManager.Instance.LayerMask))
            {
                retval = hitInfo.point;
            }
        }
        // If we are the only player, have the model act as the 'cursor' ...
        else
        {
            // We prefer to put the model on a real world surface.
            RaycastHit hitInfo;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 30, SpatialMappingManager.Instance.LayerMask))
            {
                retval = hitInfo.point;
            }
            else
            {
                // But if we don't have a ray that intersects the real world, just put the model 2m in
                // front of the user.
                retval = Camera.main.transform.position + Camera.main.transform.forward * cameraDistanceZ;
            }
        }
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
}