using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit: MonoBehaviour
{

    protected int damagePower;

    protected int health;

    public int getDamagePower()
    {

        return damagePower;

    }

    public int getHealth()
    {

        return health;

    }

    public abstract void attack();

    public abstract void defend();

    public abstract void repair();

    public abstract void die();

}