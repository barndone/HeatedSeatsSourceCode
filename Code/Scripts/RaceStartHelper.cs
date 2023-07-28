using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStartHelper : MonoBehaviour
{
    public List<Transform> startPositions = new List<Transform>();

    private void OnEnable()
    {
        AudioManager.instance.musicSource.Stop();
        GameManager.instance.InstantiateJoinedPlayers(startPositions);
    }
}
