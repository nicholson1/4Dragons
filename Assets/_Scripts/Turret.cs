using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Unit
{
    private void Start()
    {
        InitializeTurret();
    }

    private void InitializeTurret()
    {
        this.health = 100;
        this.damagePower = 50;
    }

    public override void attack()
    {
        Debug.Log("Do Turret Attack");
    }

    public override void defend()
    {
        Debug.Log("Do Turret defend");
    }

    public override void repair()
    {
        Debug.Log("Do Turret repair");
    }

    public override void die()
    {
        Debug.Log("Do Turret die");
    }
}

public class Tank : Unit
{
    private float MoveSpeed;

    private void Start()
    {
        InitializeTank();
    }

    private void InitializeTank()
    {
        this.health = 500;
        this.damagePower = 20;
        MoveSpeed = 5;
    }

    public override void attack()
    {
        Debug.Log("Do Tank Attack");
    }

    public override void defend()
    {
        Debug.Log("Do Tank  defend");
    }

    public override void repair()
    {
        Debug.Log("Do Tank  repair");
    }

    public override void die()
    {
        Debug.Log("Do Tank  die");
    }

    public void Move()
    {
        Debug.Log("Do Tank  Move");
    }
}
