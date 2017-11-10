using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using HoloToolkitExtensions.Messaging;
using UnityEngine;

public class ButtonClicker : MonoBehaviour, IInputClickHandler {

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Messenger.Instance.Broadcast(new ButtonClickMessage {ClickedObject = gameObject, EventData = eventData});
    }
}
