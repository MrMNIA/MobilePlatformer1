using UnityEngine;
using UnityEngine.UI;

public class CoinShopItem : MonoBehaviour
{
    public GameObject wallet;
    public Text infoTexT;

    private void OnEnable()
    {
        // Kendini ServiceManager'a tanıt
        if (ServiceManager.Instance != null)
            ServiceManager.Instance.currentShopUI = this;

        UpdateUI();
    }

    public void UpdateUI()
    {
        MoneyManager.Instance.UpdateCoins(wallet);
        MoneyManager.Instance.UpdateCoins(MoneyManager.Instance.shopWalletUI);
    }

    public void SatinAl_Button(int miktar)
    {
        ServiceManager.Instance.SatinAl(miktar);
    }

    public void WatchAds_Button()
    {
        ServiceManager.Instance.WatchADSCoin();
    }

    public void ShowMessage(string msg)
    {
        if (infoTexT != null)
        {
            infoTexT.text = msg;
            CancelInvoke(nameof(ClearText));
            Invoke(nameof(ClearText), 2.0f);
        }
    }

    void ClearText()
    {
        if (infoTexT != null) infoTexT.text = "";
    }
}