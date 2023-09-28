using Fusion;
using System;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI timeText;
    [SerializeField] TMPro.TextMeshProUGUI score1Text;
    [SerializeField] TMPro.TextMeshProUGUI score2Text;

    [SerializeField] private float totalTime = 180;
    private float time = 0;
    int playersOnline = 0;

    [Networked(OnChanged = nameof(NetworkTimeChanged))]
    float networkedTime { get; set; }


    private void Start()
    {
        time = totalTime;
    }

    public void PlayerJoined()
    {
        playersOnline++;
    }

    public void PlayerLeft()
    {
        playersOnline--;
    }

    private void Update()
    {
        if (HasStateAuthority)
        {
            //Match started
            if (playersOnline == 2)
            {
                networkedTime = time -= Time.deltaTime;
            }
        }
        timeText.text = TimeSpan.FromSeconds(time).ToString(@"m\:ss");
    }

    private static void NetworkTimeChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.time = changed.Behaviour.networkedTime;
    }
}
