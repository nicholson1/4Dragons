using UnityEngine;
using UnityEngine.Serialization;

namespace _Script.UI.Menu_Screens.IAP
{
    [CreateAssetMenu(menuName = "In App Purchase Product")]
    public class IAP_Product : ScriptableObject
    {
        public string ProductName;
        public string Description;
        public string Product_ID;
        public bool CostsRealMoney;
        public int Cost;
        public Sprite ProductImage;
        public bool Consumable;
        public TBAD_ProductType TypeOfProduct;
        public int CurrencyToAdd;
    }

    public enum TBAD_ProductType
    {
        Currency,
        Dragon,
        AdsRemoved,
        Skin,
        Effect
    }
}