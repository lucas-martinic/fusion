using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : Singleton<Player>
{
    public Transform head;
    public Transform handL;
    public Transform handR;

    public Transform headOffset;
    public Transform lHandOffset;
    public Transform rHandOffset;

    public GameObject hurtScreen;
    public GameObject mildHurtScreen;
    public GameObject respawnScreen;
    public TextMeshProUGUI respawnTimer;
    public Animator damageAnimator;
}
