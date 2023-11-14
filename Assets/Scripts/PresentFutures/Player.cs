using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : Singleton<Player>
{
    public Transform head;
    public Transform handL;
    public Transform handR;

    public Transform headOffset;
    public Transform lHandOffset;
    public Transform rHandOffset;

    public Image hurtScreen;
    public GameObject mildHurtScreen;
    public GameObject respawnScreen;
    public TextMeshProUGUI respawnTimer;
    public Animator damageAnimator;

    //Logs
    public TextMeshProUGUI punchDebugText;
}
