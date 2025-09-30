using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomData roomData;

    private void Awake()
    {
        roomData.id = (uint) transform.GetSiblingIndex() + 1;
        roomData.position = transform.position;
    }
}
