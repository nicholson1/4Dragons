using System;

namespace _Script.UI.Menu_Screens.IAP
{
    public interface IPurchaseHandler
    {
        public void Success(IAP_Product product);

        public void Failure(IAP_Product product, String feedback);

        public void PurchasesRestored(bool success);
    }
}