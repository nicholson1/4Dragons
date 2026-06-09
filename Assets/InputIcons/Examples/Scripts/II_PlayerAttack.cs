using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_PlayerAttack : MonoBehaviour
    {
        private II_Player player => GetComponent<II_Player>();

        public GameObject[] attackPrefabs;
        private int usedWeaponID = 0;

        public Transform gunEnd;
        public float attackCooldown = 1;
        private float currentCooldown = 0;

        // Start is called before the first frame update
        void Start()
        {
            player.AttackEvent += Attack;
            player.WeaponSwitchEvent += SwitchWeapon;
        }

        private void OnDestroy()
        {
            player.AttackEvent -= Attack;
            player.WeaponSwitchEvent -= SwitchWeapon;
        }

        private void Update()
        {
            if (currentCooldown > 0)
                currentCooldown -= Time.deltaTime;
        }

        public void SwitchWeapon()
        {
            if (player.GetScrollInput().y > 0)
            {
                PreviousWeapon();
            }
            else
            {
                NextWeapon();
            }
        }

        public void NextWeapon()
        {
            usedWeaponID++;
            if (usedWeaponID > attackPrefabs.Length - 1)
                usedWeaponID = 0;
        }

        public void PreviousWeapon()
        {
            usedWeaponID--;
            if (usedWeaponID < 0)
                usedWeaponID = attackPrefabs.Length - 1;
        }

        public void Attack()
        {
            if (currentCooldown <= 0)
            {
                currentCooldown = attackCooldown;
                GameObject attackObj = Instantiate(attackPrefabs[usedWeaponID], gunEnd.position, gunEnd.rotation);
                Destroy(attackObj, 2f);
            }
        }
    }
}
