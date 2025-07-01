using _Script.UI.Menu_Screens.IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.UI.Menu_Screens.Main_Menu
{
    public class ProductWidget : MonoBehaviour
    {
      
        public Image productImage;
        public TextMeshProUGUI productNameText;
        public TextMeshProUGUI productDescriptionText;
        public TextMeshProUGUI productPriceText;
        public bool IsAlreadyPurchased = false;
        public Button purchaseButton;
        private IAP_Product _product;
        
        
        
        
        public IAP_Product Product { 
            set
            {
                _product = value;
                productNameText.text = value.name;
                productPriceText.text = value.Cost.ToString();
                    //? GeneralUtilities.FormatCurrencyUSCulture(value.Cost / 100f)
                    //: GeneralUtilities.FormatNumberCultureSpecific(value.Cost);
                productImage.sprite = value.ProductImage;
            }
    
            get {
                return _product;
            }
        }
        
        public void SetAlreadyPurchased()
        {
            IsAlreadyPurchased = true;
        }
        
        
        
    }
}