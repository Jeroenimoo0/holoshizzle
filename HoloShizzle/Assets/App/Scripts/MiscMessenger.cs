using System.Collections;
using System.Collections.Generic;
using HoloToolkitExtensions.Messaging;
using UnityEngine;


public class MiscMessenger : MonoBehaviour
{
    public void SendReset()
    {
        Messenger.Instance.Broadcast(new ResetMessage());
        Messenger.Instance.Broadcast(new ConfirmSoundMessage());
    }
}
