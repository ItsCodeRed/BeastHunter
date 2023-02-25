using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanserBush : Herb
{
    public Animator anim;

    public override void CollectItem()
    {
        hintText.SetActive(false);
        anim.Play("Pick");
        GameManager.instance.ChangeAntidote(2);
        enabled = false;
    }
}
