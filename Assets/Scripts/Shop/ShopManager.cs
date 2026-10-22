using RuStore.PayClient;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contentContainer;
    public GameObject packPrefab;
    public Button backButton;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;

    [Header("Сцены")]
    public string mainMenuScene = "MainMenu";

    [Header("Наборы")]
    public List<ShopPackData> packs = new List<ShopPackData>();

    private readonly Dictionary<string, ShopPack> packViews =
        new Dictionary<string, ShopPack>();

    private readonly Dictionary<string, Product> storeProducts =
        new Dictionary<string, Product>();

    private bool paymentsAvailable;
    private bool purchaseInProgress;

    #pragma warning disable 0414
    private bool productsLoaded;
#pragma warning restore 0414

    private bool rewardedCoinsAdInProgress;
    private float nextRewardedCardRefreshTime;

    private const string RewardedCoinsPackId = "reward_ad";
    private const int RewardedCoinsAmount = 500;
    private const int RewardedCoinsMaxUses = 3;
    private const int RewardedCoinsCooldownSeconds = 3 * 60 * 60;

    private const string RewardedCoinsDayKey =
        "SafeZoneRunner_RewardAd_Day";

    private const string RewardedCoinsCountKey =
        "SafeZoneRunner_RewardAd_Count";

    private const string RewardedCoinsNextTimeKey =
        "SafeZoneRunner_RewardAd_NextUtc";
    private const string GrantedPurchaseKey =
        "SafeZoneRunner_RuStore_Granted_";

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(
                () => SceneManager.LoadScene(mainMenuScene)
            );
        }

        if (packs == null || packs.Count == 0)
            packs = GetDefaults();

        BuildPacks();
        UpdateTopCurrencies();

        RefreshRewardedCoinsCard();

#if UNITY_ANDROID && !UNITY_EDITOR
        InitializePayments();
#else
        Debug.Log(
            "[Shop] RuStore Pay проверяется в Android-сборке."
        );
#endif
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRewardedCardRefreshTime)
            return;

        nextRewardedCardRefreshTime = Time.unscaledTime + 1f;

        RefreshRewardedCoinsCard();
    }

    private List<ShopPackData> GetDefaults()
    {
        return new List<ShopPackData>
        {
            new ShopPackData
            {
                id = "pack_survivor",
                displayName = "НАБОР ВЫЖИВШЕГО",
                priceRub = 199,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 2000,
                grantDiamonds = 200,
                contents = "• 2000 монет\n• 200 кристаллов"
            },
            new ShopPackData
            {
                id = "pack_weapons",
                displayName = "НАБОР СНАБЖЕНИЯ",
                priceRub = 399,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 5000,
                grantDiamonds = 500,
                contents = "• 5000 монет\n• 500 кристаллов"
            },
            new ShopPackData
            {
                id = "pack_coins",
                displayName = "МЕШОК МОНЕТ",
                priceRub = 99,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 2000,
                grantDiamonds = 0,
                contents = "• 2000 монет"
            },
            new ShopPackData
            {
                id = "pack_crystals",
                displayName = "СУНДУК КРИСТАЛЛОВ",
                priceRub = 149,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 0,
                grantDiamonds = 300,
                contents = "• 300 кристаллов"
            },
            new ShopPackData
            {
                id = "big_pack_crystals",
                displayName = "БОЛЬШОЙ СУНДУК КРИСТАЛЛОВ",
                priceRub = 299,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 0,
                grantDiamonds = 700,
                contents = "• 700 кристаллов"
            },
            new ShopPackData
            {
                id = "big_pack_coin",
                displayName = "БОЛЬШОЙ МЕШОК МОНЕТ",
                priceRub = 199,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 5000,
                grantDiamonds = 0,
                contents = "• 5000 монет"
            },
            new ShopPackData
            {
                id = "reward_ad",
                displayName = "МОНЕТЫ ЗА ВИДЕО",
                priceRub = 0,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 500,
                grantDiamonds = 0,
                contents = "• 500 монет"
            }
        };
    }

    private void BuildPacks()
    {
        if (contentContainer == null || packPrefab == null)
        {
            Debug.LogError(
                "[Shop] Не назначены Content Container или Pack Prefab."
            );
            return;
        }

        packViews.Clear();

        for (int i = contentContainer.childCount - 1; i >= 0; i--)
            Destroy(contentContainer.GetChild(i).gameObject);

        foreach (ShopPackData pack in packs)
        {
            if (pack == null || string.IsNullOrWhiteSpace(pack.id))
                continue;

            GameObject go = Instantiate(packPrefab, contentContainer);
            ShopPack view = go.GetComponent<ShopPack>();

            if (view == null)
            {
                Debug.LogError(
                    "[Shop] На карточке отсутствует компонент ShopPack."
                );
                Destroy(go);
                continue;
            }

            view.Setup(pack, () => OnBuy(pack));

            if (view.buyButton != null)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                view.buyButton.interactable = false;
#else
                // В редакторе кнопки позволяют проверить интерфейс,
                // но покупка и выдача игровой валюты не симулируются.
                view.buyButton.interactable = true;
#endif
            }

            packViews[pack.id] = view;
        }
    }

    private void InitializePayments()
    {
        Debug.Log("[Shop] Проверяем доступность RuStore Pay.");

        RuStorePayClient.Instance.GetPurchaseAvailability(
            onFailure: error =>
            {
                paymentsAvailable = false;
                SetBuyButtonsInteractable(false);

                Debug.LogError(
                    "[Shop] Проверка доступности платежей: " + error
                );

                ShowMessage(
                    "Не удалось проверить доступность оплаты."
                );
            },
            onSuccess: result =>
            {
                paymentsAvailable = result.isAvailable;

                if (!paymentsAvailable)
                {
                    SetBuyButtonsInteractable(false);

                    Debug.LogWarning(
                        "[Shop] RuStore Pay недоступен: " + result.cause
                    );

                    ShowMessage(
                        "Оплата временно недоступна."
                    );
                    return;
                }

                Debug.Log("[Shop] RuStore Pay доступен.");

                RestorePendingPurchases();
                LoadProducts();
            }
        );
    }

    private void LoadProducts()
    {
        List<ProductId> requestedProducts = new List<ProductId>();

        foreach (ShopPackData pack in packs)
        {
            if (pack == null ||
                string.IsNullOrWhiteSpace(pack.id) ||
                pack.id == RewardedCoinsPackId)
            {
                continue;
            }

            requestedProducts.Add(new ProductId(pack.id));
        }

        productsLoaded = false;
        storeProducts.Clear();
        SetBuyButtonsInteractable(false);

        if (requestedProducts.Count == 0)
        {
            productsLoaded = true;
            RefreshBuyButtons();
            return;
        }

        // Передаём массив первым позиционным аргументом:
        // это не зависит от имени параметра в установленной DLL SDK.
        RuStorePayClient.Instance.GetProducts(
            requestedProducts.ToArray(),

            error =>
            {
                productsLoaded = false;
                storeProducts.Clear();

                SetBuyButtonsInteractable(false);
                RefreshRewardedCoinsCard();

                Debug.LogError(
                    "[Shop] Не удалось загрузить товары: " + error
                );

                ShowMessage(
                    "Не удалось загрузить каталог покупок."
                );
            },

            result =>
            {
                storeProducts.Clear();

                if (result == null)
                {
                    productsLoaded = false;
                    SetBuyButtonsInteractable(false);
                    RefreshRewardedCoinsCard();

                    Debug.LogError(
                        "[Shop] SDK вернул пустой каталог товаров."
                    );

                    ShowMessage(
                        "Не удалось загрузить каталог покупок."
                    );

                    return;
                }

                foreach (Product product in result)
                {
                    if (product == null ||
                        product.productId == null)
                    {
                        continue;
                    }

                    string id = product.productId.value;

                    if (!string.IsNullOrWhiteSpace(id))
                        storeProducts[id] = product;
                }

                productsLoaded = true;

                int missingCount = 0;

                foreach (ShopPackData pack in packs)
                {
                    if (pack == null ||
                        pack.id == RewardedCoinsPackId)
                    {
                        continue;
                    }

                    bool found = storeProducts.ContainsKey(pack.id);

                    ShopPack view;

                    if (packViews.TryGetValue(pack.id, out view))
                    {
                        if (view.buyButton != null)
                        {
                            view.buyButton.interactable =
                                paymentsAvailable &&
                                productsLoaded &&
                                !purchaseInProgress &&
                                found;
                        }

                        Product product;

                        if (found &&
                            storeProducts.TryGetValue(
                                pack.id, out product) &&
                            view.buyButtonText != null &&
                            product.amountLabel != null &&
                            !string.IsNullOrWhiteSpace(
                                product.amountLabel.value))
                        {
                            view.buyButtonText.text =
                                product.amountLabel.value;
                        }
                    }

                    if (!found)
                    {
                        missingCount++;

                        Debug.LogWarning(
                            "[Shop] Товар отсутствует в каталоге SDK: " +
                            pack.id
                        );
                    }
                }

                RefreshRewardedCoinsCard();

                Debug.Log(
                    "[Shop] Каталог загружен. Получено платных товаров: " +
                    storeProducts.Count
                );

                if (missingCount > 0)
                {
                    ShowMessage(
                        "Некоторые товары пока недоступны."
                    );
                }
            }
        );
    }

    private void OnBuy(ShopPackData pack)
    {
        if (pack == null)
            return;

        if (pack.id == RewardedCoinsPackId)
        {
            TryClaimRewardedCoins();
            return;
        }

#if !UNITY_ANDROID || UNITY_EDITOR
        ShowMessage(
            "Покупки проверяются только в Android-сборке."
        );
        return;
#else
        if (!paymentsAvailable)
        {
            ShowMessage("Оплата временно недоступна.");
            return;
        }

        if (!productsLoaded ||
            !storeProducts.ContainsKey(pack.id))
        {
            ShowMessage(
                "Этот товар пока недоступен для покупки."
            );
            return;
        }

        if (purchaseInProgress)
        {
            ShowMessage("Предыдущая покупка ещё обрабатывается.");
            return;
        }

        purchaseInProgress = true;
        SetBuyButtonsInteractable(false);

        RefreshRewardedCoinsCard();

        AppMetricaBootstrap.TrackEvent(
            "inapp_purchase_started",
            "{\"product_id\":\"" + pack.id + "\"}"
        );

        Debug.Log("[Shop] Запускаем оплату: " + pack.id);

        try
        {
            ProductPurchaseParams parameters =
                new ProductPurchaseParams(
                    productId: new ProductId(pack.id),
                    appUserEmail: null,
                    appUserId: null,
                    developerPayload: null,
                    orderId: null,
                    quantity: new Quantity(1)
                );

            RuStorePayClient.Instance.Purchase(
                parameters: parameters,
                preferredPurchaseType:
                    PreferredPurchaseType.ONE_STEP,
                sdkTheme: SdkTheme.DARK,
                onFailure: error =>
                {
                    purchaseInProgress = false;
                    RefreshBuyButtons();

                    if (error is
                        RuStorePaymentException.ProductPurchaseCancelled)
                    {
                        Debug.Log(
                            "[Shop] Пользователь отменил оплату."
                        );

                        AppMetricaBootstrap.TrackEvent(
                            "inapp_purchase_cancelled",
                            "{\"product_id\":\"" + pack.id + "\"}"
                        );

                        ShowMessage("Покупка отменена.");
                    }
                    else
                    {
                        Debug.LogError(
                            "[Shop] Ошибка оплаты: " + error
                        );

                        AppMetricaBootstrap.TrackEvent(
                            "inapp_purchase_failed",
                            "{\"product_id\":\"" + pack.id + "\"}"
                        );

                        ShowMessage(
                            "Не удалось выполнить покупку."
                        );
                    }
                },
                onSuccess: result =>
                {
                    purchaseInProgress = false;
                    RefreshBuyButtons();

                    if (result == null)
                    {
                        Debug.LogError(
                            "[Shop] SDK вернул пустой результат покупки."
                        );

                        ShowMessage(
                            "Не удалось подтвердить результат оплаты."
                        );
                        return;
                    }

                    string purchasedProductId =
                        result.productId.value;

                    if (purchasedProductId != pack.id)
                    {
                        Debug.LogError(
                            "[Shop] ID товара в результате оплаты не совпадает."
                        );

                        ShowMessage(
                            "Не удалось сопоставить покупку с товаром."
                        );
                        return;
                    }

                    bool granted = GrantPurchaseOnce(
                        purchasedProductId,
                        result.purchaseId,
                        result.sandbox,
                        result.quantity.value
                    );

                    if (granted)
                    {
                        UpdateAcknowledgement(
                            result.purchaseId,
                            purchasedProductId
                        );
                    }
                },
                purchaseEventListener: null
            );
        }
        catch (Exception exception)
        {
            purchaseInProgress = false;
            RefreshBuyButtons();

            Debug.LogError(
                "[Shop] Не удалось запустить покупку: " + exception
            );

            ShowMessage(
                "Не удалось открыть окно оплаты."
            );
        }
#endif
    }

    private void RestorePendingPurchases()
    {
        // Проверяем завершённые покупки, для которых выдача
        // товара ещё не отмечена как обработанная.
        RuStorePayClient.Instance.GetPurchases(
            onFailure: error =>
            {
                Debug.LogWarning(
                    "[Shop] Не удалось получить историю покупок: " +
                    error
                );
            },
            onSuccess: purchases =>
            {
                foreach (var item in purchases)
                {
                    if (!(item is ProductPurchase purchase))
                        continue;

                    if (purchase.status !=
                        ProductPurchaseStatus.CONFIRMED)
                        continue;

                    if (purchase.acknowledgementState !=
                        AcknowledgementState.PENDING)
                        continue;

                    string productId =
                        purchase.productId.value;

                    if (FindPack(productId) == null)
                        continue;

                    bool granted = GrantPurchaseOnce(
                        productId,
                        purchase.purchaseId,
                        purchase.sandbox,
                        purchase.quantity.value
                    );

                    if (granted)
                    {
                        UpdateAcknowledgement(
                            purchase.purchaseId,
                            productId
                        );
                    }
                }
            }
        );
    }

    private bool GrantPurchaseOnce(
        string productId,
        PurchaseId purchaseId,
        bool sandbox,
        int quantity)
    {
        ShopPackData pack = FindPack(productId);

        if (pack == null ||
            purchaseId == null ||
            string.IsNullOrWhiteSpace(purchaseId.value))
        {
            Debug.LogError(
                "[Shop] Не удалось сопоставить покупку с набором."
            );
            return false;
        }

        string key = GrantedPurchaseKey + purchaseId.value;

        // Защита от повторной выдачи одной и той же покупки.
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            Debug.Log(
                "[Shop] Покупка уже выдана: " + purchaseId.value
            );
            return true;
        }

        quantity = Mathf.Max(1, quantity);

        int newCoins =
            PlayerPrefs.GetInt("TotalCoins", 0) +
            pack.grantCoins * quantity;

        int newDiamonds =
            PlayerPrefs.GetInt("TotalDiamonds", 0) +
            pack.grantDiamonds * quantity;

        PlayerPrefs.SetInt("TotalCoins", newCoins);
        PlayerPrefs.SetInt("TotalDiamonds", newDiamonds);

        // Баланс и отметка покупки сохраняются вместе.
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        UpdateTopCurrencies();

        AppMetricaBootstrap.TrackEvent(
            "inapp_purchase_granted",
            "{\"product_id\":\"" + productId +
            "\",\"sandbox\":" +
            (sandbox ? "true" : "false") +
            ",\"quantity\":" + quantity + "}"
        );

        Debug.Log(
            "[Shop] Выдан товар " + productId +
            ", покупка " + purchaseId.value +
            ", тестовая покупка: " + sandbox
        );

        ShowMessage("Покупка успешна: " + pack.displayName);

        return true;
    }

    private void UpdateAcknowledgement(
        PurchaseId purchaseId,
        string productId)
    {
        // Выдача уже сохранена. Отдельно сообщаем RuStore,
        // что товар обработан, чтобы не терять историю выдачи.
        RuStorePayClient.Instance.UpdateAcknowledgementState(
            purchaseId: purchaseId,
            acknowledgementState:
                AcknowledgementState.ACKNOWLEDGED,
            developerPayload: null,
            onFailure: error =>
            {
                Debug.LogWarning(
                    "[Shop] Не удалось отметить выдачу товара " +
                    productId + ": " + error
                );
            },
            onSuccess: result =>
            {
                Debug.Log(
                    "[Shop] Выдача подтверждена для " + productId
                );
            }
        );
    }

    private ShopPackData FindPack(string productId)
    {
        return packs.Find(
            item => item != null && item.id == productId
        );
    }

    private void RefreshBuyButtons()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

    bool enabled =
        paymentsAvailable &&
        productsLoaded &&
        !purchaseInProgress;

    foreach (ShopPackData pack in packs)
    {
        if (pack == null ||
            pack.id == RewardedCoinsPackId)
        {
            continue;
        }

        ShopPack view;

        bool available =
            enabled &&
            storeProducts.ContainsKey(pack.id);

        if (packViews.TryGetValue(pack.id, out view) &&
            view != null &&
            view.buyButton != null)
        {
            view.buyButton.interactable = available;
        }
    }

#endif

        RefreshRewardedCoinsCard();
    }

    private void SetBuyButtonsInteractable(bool interactable)
    {
        foreach (KeyValuePair<string, ShopPack> item in packViews)
        {
            // Рекламная карточка не относится к RuStore Pay.
            if (item.Key == RewardedCoinsPackId)
                continue;

            ShopPack view = item.Value;

            if (view != null && view.buyButton != null)
                view.buyButton.interactable = interactable;
        }
    }

    private void AddCoins(int amount)
    {
        int total = PlayerPrefs.GetInt("TotalCoins", 0) + amount;
        PlayerPrefs.SetInt("TotalCoins", total);
        PlayerPrefs.Save();
    }

    private void AddDiamonds(int amount)
    {
        int total =
            PlayerPrefs.GetInt("TotalDiamonds", 0) + amount;

        PlayerPrefs.SetInt("TotalDiamonds", total);
        PlayerPrefs.Save();
    }

    private void RefreshRewardedCoinsDay()
    {
        // День определяется по местному времени телефона.
        string today = DateTime.Now.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture
        );

        string savedDay = PlayerPrefs.GetString(
            RewardedCoinsDayKey,
            string.Empty
        );

        if (savedDay != today)
        {
            PlayerPrefs.SetString(RewardedCoinsDayKey, today);
            PlayerPrefs.SetInt(RewardedCoinsCountKey, 0);

            // Не сбрасываем незавершённый трёхчасовой интервал.
            PlayerPrefs.Save();
        }
    }

    private long GetRewardedCoinsCooldownRemaining()
    {
        string savedValue = PlayerPrefs.GetString(
            RewardedCoinsNextTimeKey,
            "0"
        );

        long nextAllowedTime;

        if (!long.TryParse(
            savedValue,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out nextAllowedTime))
        {
            return 0;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return Math.Max(0L, nextAllowedTime - now);
    }

    private static string FormatRewardedCoinsTime(long seconds)
    {
        TimeSpan remaining = TimeSpan.FromSeconds(
            Math.Max(0L, seconds)
        );

        int totalHours = (int)Math.Floor(remaining.TotalHours);

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:00}:{1:00}:{2:00}",
            totalHours,
            remaining.Minutes,
            remaining.Seconds
        );
    }

    private void RefreshRewardedCoinsCard()
    {
        ShopPack view;

        if (!packViews.TryGetValue(
            RewardedCoinsPackId,
            out view) ||
            view == null)
        {
            return;
        }

        RefreshRewardedCoinsDay();

        int used = Mathf.Clamp(
            PlayerPrefs.GetInt(RewardedCoinsCountKey, 0),
            0,
            RewardedCoinsMaxUses
        );

        if (rewardedCoinsAdInProgress)
        {
            view.SetRewardedState(
                "Просмотр рекламы выполняется.\n" +
                "Дождитесь подтверждения награды.",
                "ПРОСМОТР ИДЁТ",
                "ИДЁТ ВИДЕО",
                false
            );

            return;
        }

        if (purchaseInProgress)
        {
            view.SetRewardedState(
                "Дождитесь завершения покупки.\n" +
                "Сегодня использовано: " + used + "/3",
                "ОЖИДАНИЕ",
                "ПОДОЖДИТЕ",
                false
            );

            return;
        }

        if (used >= RewardedCoinsMaxUses)
        {
            TimeSpan untilMidnight =
                DateTime.Today.AddDays(1) - DateTime.Now;

            long secondsUntilMidnight = Math.Max(
                0L,
                (long)Math.Ceiling(untilMidnight.TotalSeconds)
            );

            view.SetRewardedState(
                "Лимит на сегодня: 3/3\n" +
                "Следующий набор доступен после полуночи.",
                FormatRewardedCoinsTime(secondsUntilMidnight),
                "ЗАВТРА",
                false
            );

            return;
        }

        long cooldown = GetRewardedCoinsCooldownRemaining();

        if (cooldown > 0)
        {
            view.SetRewardedState(
                "Сегодня использовано: " + used + "/3\n" +
                "Следующий просмотр через:",
                FormatRewardedCoinsTime(cooldown),
                "ОЖИДАНИЕ",
                false
            );

            return;
        }

        view.SetRewardedState(
            "+500 монет за просмотр\n" +
            "Сегодня использовано: " + used + "/3",
            "ИНТЕРВАЛ 03:00:00",
            "СМОТРЕТЬ",
            true
        );
    }

    private void TryClaimRewardedCoins()
    {
#if !UNITY_ANDROID || UNITY_EDITOR

        ShowMessage(
            "Награда за рекламу доступна в Android-сборке."
        );

        return;

#else

    if (rewardedCoinsAdInProgress)
        return;

    if (purchaseInProgress)
    {
        ShowMessage(
            "Дождитесь завершения текущей покупки."
        );

        return;
    }

    RefreshRewardedCoinsDay();

    int used = Mathf.Clamp(
        PlayerPrefs.GetInt(RewardedCoinsCountKey, 0),
        0,
        RewardedCoinsMaxUses
    );

    if (used >= RewardedCoinsMaxUses)
    {
        ShowMessage(
            "На сегодня все три просмотра использованы."
        );

        RefreshRewardedCoinsCard();
        return;
    }

    long cooldown = GetRewardedCoinsCooldownRemaining();

    if (cooldown > 0)
    {
        ShowMessage(
            "Следующий просмотр через " +
            FormatRewardedCoinsTime(cooldown)
        );

        RefreshRewardedCoinsCard();
        return;
    }

    if (RewardedAdsManager3D.Instance == null)
    {
        ShowMessage(
            "Реклама временно недоступна. Попробуйте позже."
        );

        Debug.LogWarning(
            "[Shop] RewardedAdsManager3D не найден."
        );

        return;
    }

    rewardedCoinsAdInProgress = true;
    RefreshRewardedCoinsCard();

    RewardedAdsManager3D.Instance.ShowRewardedAd(
        onRewarded: GrantRewardedCoinsAfterAd,

        onFailed: error =>
        {
            rewardedCoinsAdInProgress = false;

            Debug.LogWarning(
                "[Shop] Просмотр рекламы не завершён: " + error
            );

            RefreshRewardedCoinsCard();

            ShowMessage(
                "Не удалось получить награду. " +
                "Попробуйте посмотреть рекламу позже."
            );
        },

        placement: "shop_coins"
    );

#endif
    }

    private void GrantRewardedCoinsAfterAd()
    {
        // Не допускаем повторного начисления одной рекламной попытки.
        if (!rewardedCoinsAdInProgress)
            return;

        rewardedCoinsAdInProgress = false;

        RefreshRewardedCoinsDay();

        int used = Mathf.Clamp(
            PlayerPrefs.GetInt(RewardedCoinsCountKey, 0),
            0,
            RewardedCoinsMaxUses - 1
        );

        used++;

        int totalCoins =
            PlayerPrefs.GetInt("TotalCoins", 0) +
            RewardedCoinsAmount;

        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt(RewardedCoinsCountKey, used);

        if (used < RewardedCoinsMaxUses)
        {
            long nextAllowedTime =
                DateTimeOffset.UtcNow
                    .AddSeconds(RewardedCoinsCooldownSeconds)
                    .ToUnixTimeSeconds();

            PlayerPrefs.SetString(
                RewardedCoinsNextTimeKey,
                nextAllowedTime.ToString(
                    CultureInfo.InvariantCulture
                )
            );
        }
        else
        {
            // После третьего просмотра ограничение до полуночи
            // обеспечивается дневным счётчиком.
            PlayerPrefs.DeleteKey(RewardedCoinsNextTimeKey);
        }

        PlayerPrefs.Save();

        UpdateTopCurrencies();
        RefreshRewardedCoinsCard();

        AppMetricaBootstrap.TrackEvent(
            "rewarded_coins_granted",
            "{\"placement\":\"shop_coins\",\"amount\":500}"
        );

        ShowMessage("Получено 500 монет!");
    }

    private void UpdateTopCurrencies()
    {
        if (coinsText != null)
        {
            coinsText.text =
                PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        }

        if (diamondsText != null)
        {
            diamondsText.text =
                PlayerPrefs.GetInt("TotalDiamonds", 0).ToString();
        }
    }

    private void ShowMessage(string message)
    {
        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show(message);
        else
            Debug.Log("[Shop] " + message);
    }
}
