using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public WeaponType weaponType;
    public bool isTwoHanded;
    public float baseDamage;
    public float speed;

    int currentCombo = 0;
    public int combo1Length;
    public int combo2Length;


    public Animator anim;

    bool isAttacking;

    public void Attack1()
    {

        if (isAttacking)
        {
            ++currentCombo;

        }
        else
        {
            currentCombo = 0;
        }

        isAttacking = true;
    }
}
