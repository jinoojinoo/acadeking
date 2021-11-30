using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Purchasing;

public class IAPManager : SingletonMono<IAPManager>, IStoreListener
{
    public const string Product_GOLD1 = "gold1";
    public const string Product_GOLD2 = "gold2";
    public const string Product_GOLD3 = "gold3";
    public const string Product_GOLD4 = "gold4";

    public const string Product_DisableAD = "DisableAD";

    private const string _android_Gold1_ID = "com.pheigame.app.gold1";
    private const string _android_Gold2_ID = "com.pheigame.app.gold2";
    private const string _android_Gold3_ID = "com.pheigame.app.gold3";
    private const string _android_Gold4_ID = "com.pheigame.app.gold4";

    private const string _android_disable_ads_ID = "com.pheigame.app.disable_ads";
    //     private const string _android_Skin = "com.studio.app.skin";
    //     private const string _android_Subscription = "com.studio.app.subscription";


    private IStoreController m_storeContorller = null;
    private IExtensionProvider m_storeextensionProvider = null;

    private void Awake()
    {
        InitUnityIAP();
    }

    public bool IsInit
    {
        get
        {
            return m_storeContorller != null && m_storeextensionProvider != null;
        }
    }

    public void Init()
    {

    }

    private bool m_disableAD = false;
    public bool DisableAD
    {
        get
        {
            return m_disableAD;
        }
        set
        {
            m_disableAD = value;
        }
    }

    private void InitUnityIAP()
    {
        if (IsInit)
            return;

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(Product_GOLD1, ProductType.Consumable, new IDs()
        {
//            { _android_Gold_ID, AppleAppStore.Name },
            { _android_Gold1_ID, GooglePlay.Name },
        });
        builder.AddProduct(Product_GOLD2, ProductType.Consumable, new IDs()
        {
            { _android_Gold2_ID, GooglePlay.Name },
        });
        builder.AddProduct(Product_GOLD3, ProductType.Consumable, new IDs()
        {
            { _android_Gold3_ID, GooglePlay.Name },
        });
        builder.AddProduct(Product_GOLD4, ProductType.Consumable, new IDs()
        {
            { _android_Gold4_ID, GooglePlay.Name },
        });

        builder.AddProduct(Product_DisableAD, ProductType.NonConsumable, new IDs()
        {
            {_android_disable_ads_ID, GooglePlay.Name },
        });


//         builder.AddProduct(Product_Skin, ProductType.Subscription, new IDs()
//         {
//             {_android_Subscription, GooglePlay.Name },
//         });

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_storeContorller = controller;
        m_storeextensionProvider = extensions;
        CheckOneTimeProduct();
    }

    private void CheckOneTimeProduct()
    {
        DisableAD = HadPurchased(Product_DisableAD);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError(error);
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.LogError("OnPurchaseFailed");

        Debug.LogError($"구매실패 - ID : {i.definition.id} Reason : {p}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.LogError("ProcessPurchase");

        Debug.LogError($"구매성공 - ID : {e.purchasedProduct.definition.id}");

        string id = e.purchasedProduct.definition.id;
        Shop_DataProperty property = Shop_Table.Instance.GetShopItemByProductID(id);
        if (property == null)
        {
            return PurchaseProcessingResult.Complete;
        }

        switch(property.Shop_Type)
        {
            case ShopType.GOLD:
                AccountManager.Instance.MyAccountInfo.Gold += property.Gold;
                break;

            case ShopType.DisableAD:
                AccountManager.Instance.MyAccountInfo.Gold += property.Gold;
                DisableAD = true;
                break;

            default:
                Debug.LogError("not found product : " + id);
                return PurchaseProcessingResult.Complete;
        }


        if ((GameUIManager.Instance.GetCurrentUISequence()?.MyGameUIMode ?? GAME_UI_MODE.None) <= GAME_UI_MODE.Start)
            return PurchaseProcessingResult.Complete;

        Debug.LogError("property product : " + property);
        PopupBase popupbase = PopupManager.Instance.ShowPopup(POPUP_TYPE.BuyProduct);
        popupbase.MsgLabel.text = string.Format(popupbase.MsgLabel.text, property.ItemName);
        return PurchaseProcessingResult.Complete;
    }

    private bool IsOneTimeProduct(string id)
    {
        switch(id)
        {
            case Product_DisableAD:
                return true;

            default:
                return false;
        }
    }

    public void Purchase(string productID)
    {
        if (!IsInit)
            return;

        Product product = m_storeContorller.products.WithID(productID);
        if (product == null || product.availableToPurchase == false)
            return;

        if (IsOneTimeProduct(productID))
        {
            if (product.hasReceipt)
            {
                RestorePurchase();
                return;
            }
        }

        m_storeContorller.InitiatePurchase(product);
    }

    public void RestorePurchase()
    {
        if (!IsInit)
            return;

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            IAppleExtensions appextension = m_storeextensionProvider.GetExtension<IAppleExtensions>();
            appextension.RestoreTransactions
                (
                result => Debug.Log($"복구시도 - {result}")
                );
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            IGooglePlayStoreExtensions googleextension = m_storeextensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            googleextension.RestoreTransactions
                (
                result => Debug.Log($"복구시도 - {result}")
                );
        }
    }

    public bool HadPurchased(string productid)
    {
        Debug.LogError($"HadPurchased : {productid}");

        if (!IsInit)
            return false;

        Product product = m_storeContorller.products.WithID(productid);
        if (product == null)
            return false;

        if (IsOneTimeProduct(productid) == false)
            return false;

        return product.hasReceipt;
    }
}
