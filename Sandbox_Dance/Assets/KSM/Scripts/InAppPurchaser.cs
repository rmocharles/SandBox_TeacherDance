using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BackEnd;
using UnityEngine.Purchasing;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    public static InAppPurchaser instance;

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    public const string PACKAGE = "com.touchtouch.sandbox_dance.package";
    public const string COIN_2000 = "com.touchtouch.sandbox_dance.coin2000";
    public const string COIN_5000 = "com.touchtouch.sandbox_dance.coin5000";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializePurchasing();
    }

    bool IsInitialized()
    {
        return (storeController != null && extensionProvider != null);
    }
    
    void InitializePurchasing()
    {
        if (IsInitialized()) return;


        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        builder.AddProduct(PACKAGE, ProductType.Consumable);
        builder.AddProduct(COIN_2000, ProductType.Consumable);
        builder.AddProduct(COIN_5000, ProductType.Consumable);


        UnityPurchasing.Initialize(this, builder);
        Debug.Log("##### InitializePurchasing : Initialize");
    }

    void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = storeController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                storeController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                // ... report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void PurchaseRestore()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }
        // If we are running on an Apple device ...
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");
            // Fetch the Apple store-specific subsystem.
            var apple = extensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) =>
            {

                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                if (result)
                {
                    // TODO : TEST 기능 (이미 구매한 상품이면 광고 지우기)
                    //AdsManager.instance.SetRemoveAds();
                    //BackEndServerManager.instance.SetRemoveAds();
                }
                else
                {
                }
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController _sc, IExtensionProvider _ep)
    {
        storeController = _sc;
        extensionProvider = _ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {
    }

    // ====================================================================================================
    #region 영수증 검증
    /* 
     *
	 */
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = string.Empty;
        string token = string.Empty;
        Param param = new Param();

        // 뒤끝 영수증 검증 처리    
        BackendReturnObject validation = null;
#if UNITY_ANDROID || UNITY_EDITOR
        validation = Backend.Receipt.IsValidateGooglePurchase(args.purchasedProduct.receipt, "receiptDescriptionGoogle");

        BackEnd.Game.Payment.GoogleReceiptData.FromJson(args.purchasedProduct.receipt, out id, out token);
        param.Add("productID", id);
        param.Add("token", token);

        param.Add("platform", "google");
#elif UNITY_IOS
        validation = Backend.Receipt.IsValidateApplePurchase(args.purchasedProduct.receipt, "receiptDescriptionApple");
#endif
        string msg = "";

        // 뒤끝 펑션 호출
        Backend.BFunc.InvokeFunction("receiptVaildate", param, callback => {
            if (callback.IsSuccess() == false)
            {
                return;
            }
            var result = callback.GetReturnValuetoJSON()["result"].ToString();
        });

        // 영수증 검증에 성공한 경우
        if (validation.IsSuccess())
        {
            // 구매 성공한 제품에 대한 id 체크하여 그에맞는 보상 
            // A consumable product has been purchased by this user.
            if (String.Equals(args.purchasedProduct.definition.id, COIN_2000, StringComparison.Ordinal))
            {
                LobbyUI.GetInstance().errorObject.SetActive(true);
                BackendServerManager.GetInstance().SaveGold(LobbyUI.GetInstance().gold + 2000);
            }
            else if (String.Equals(args.purchasedProduct.definition.id, COIN_5000, StringComparison.Ordinal))
            {
                LobbyUI.GetInstance().errorObject.SetActive(true);
                BackendServerManager.GetInstance().SaveGold(LobbyUI.GetInstance().gold + 5000);
            }
            else if (String.Equals(args.purchasedProduct.definition.id, PACKAGE, StringComparison.Ordinal))
            {
                BackendServerManager.GetInstance().SaveItem(5, 5, 5);
                LobbyUI.GetInstance().errorObject.SetActive(true);
                PlayerPrefs.SetInt("PACKAGE", 1);
            }
            LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[20].kor : BackendServerManager.GetInstance().langaugeSheet[20].eng;
        }
        // 영수증 검증에 실패한 경우 
        else
        {
            //LobbyUI.GetInstance().errorObject.SetActive(true);
            //LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = "결제 실패";

            //BackEndServerManager.instance.BuyRemoveAdsFailed();
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed.
        Backend.TBC.ChargeTBC(args.purchasedProduct.receipt, "파격 할인중!");
        return PurchaseProcessingResult.Complete;
    }
    #endregion

    // ====================================================================================================	

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
    }

    // ==================================================
    public void BuyItem(int num)
    {
        switch (num)
        {
            case 0:
                BuyProductID(PACKAGE);
                break;
            case 1:
                BuyProductID(COIN_2000);
                break;
            case 2:
                BuyProductID(COIN_5000);
                break;
        }
    }
}
