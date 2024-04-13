using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IslandItems : Mod
{
	static float ICON_TRANSPARENCY = .75f;

	private static ItemInfo[] itemInfos = new ItemInfo[]{
			new ItemInfo("Plastic", "Plastic", "Plastic"),
			new ItemInfo("Plank", "Plank", "Plank"),		
			new ItemInfo("Iron", "MetalOre", "Iron"),
			new ItemInfo("Copper", "CopperOre", "Copper"),
			new ItemInfo("Scrap", "Scrap", "Scrap"),
			new ItemInfo("Sand", "Sand", "Sand"),
			new ItemInfo("Clay", "Clay", "Clay"),
			new ItemInfo("Rock", "Stone", "Stone"),
			new ItemInfo("Dirt", "Dirt", "Dirt"),
			new ItemInfo("Sea", "SeaVine", "Seaweed"),
			new ItemInfo("GiantClam", "Placeable_GiantClam", "Giant Clam"),
			new ItemInfo("Crate", "Placeable_Storage_Small", "Crate"),
			new ItemInfo("SilverAlgae", "SilverAlgae", "Silver Algae"),
			new ItemInfo("Mushroom", "CaveMushroom", "Mushroom"),
			new ItemInfo("WatermelonLandmark", "Watermelon", "Watermelon"),
			new ItemInfo("PineappleLandmark", "Pineapple", "Pineapple"),
			new ItemInfo("BerryBush", "Berries_Red", "Berry Bush"),
			new ItemInfo("Strawberry", "Strawberry", "Strawberry Bush"),
			new ItemInfo("Beehive", "HoneyComb", "Beehive"),
			new ItemInfo("Banana", "Banana", "Banana Tree"),
			new ItemInfo("Tree_Mango", "Mango", "Mango Tree", "MangoTree"),
			new ItemInfo("Tree_Palm", "Thatch", "Palm tree", "Palmtree"),
			new ItemInfo("Tree_Pine", "Seed_Pine", "Pine Tree"),
			new ItemInfo("Tree_Birch", "Seed_Birch", "Birch Tree"),
			new ItemInfo("Flower_Black", "Flower_Black", "Black Flower"),
			new ItemInfo("Flower_Blue", "Flower_Blue", "Blue Flower"),
			new ItemInfo("Flower_Red", "Flower_Red", "Red Flower"),
			new ItemInfo("Flower_White", "Flower_White", "White Flower"),
			new ItemInfo("Flower_Yellow", "Flower_Yellow", "Yellow Flower"),
			new ItemInfo("Cassette", "Cassette_Elevator", "Cassette"),
			new ItemInfo("Blueprint", "Blueprint_Canteen", "Blueprint"),
			new ItemInfo("QuestItemPickup", "Mystery_Package", "Quest item"),
			new ItemInfo("NoteBookPickup", "Placeable_OpenBook", "Quest Notebook"),
			new ItemInfo("HealingSalve", "HealingSalve", "Healing Salve"),
			new ItemInfo("RandomLoot", "Nail", "Random Loot"),
			/*
			new ItemInfo("randomLoot", "RandomLoot", "randomLoot"),
			new ItemInfo("GeneratorPart", "GeneratorPart", "GeneratorPart"),
			new ItemInfo("Titanium", "TitaniumOre", "Titanium"),
			new ItemInfo("Machete", "Machete", "Machete"),
			new ItemInfo("blueprint", "Blueprint", "blueprint"),
			new ItemInfo("token", "TradeToken", "token"),
			new ItemInfo("Raft", "Flower_Yellow", "Raft"),
			*/
		};
	/*
    enum itemId
    {
        Iron,
        Copper,
        Scrap,
        Sand,
        Clay,
        SeaWeed,
        Stone,
        SilverAlgae,
        Dirt,
        Clam,
        Crate,
        Mushroom
    };
    */
	//Prefabs&Stuff
	AssetBundle assetBundle;
	GameObject noItemsPrefab;

	//UI
	Text[] itemAmountTexts;
	ButtonElement[] buttons;
	RectTransform backGround;
	Text noItemsObject;

	GameObject itemAmountText;
	GameObject iconImageObject;
	GameObject settingsMenu;
	GameObject newCanvas;
	GameObject scrollArea;

	ButtonElement buttonWallhack;
	ButtonElement buttonShowIcons;


	//Vars
	bool inGame = false;
	bool isModLoaded = false;
	string pSettings = "";
	float itemRefreshInterval = 2.0f;
	float iconRefreshInterval = 0.0f;
	float iconRefreshtimer = 0.0f;
	int[] itemAmounts = new int[itemInfos.Length];
	int AmountOfItems = 0;
	static int maxItemsPerKind = 1000;
	bool optionsActive = false;
	bool displayActive = true;
	static float iconScale = 1.0f;

	// Create a new dictionary of strings, with string keys.
	//
	public Dictionary<string, int> itemsDict = new Dictionary<string, int>();
	public Dictionary<string, int> spritesDict = new Dictionary<string, int>();


	public SoundManager soundManager;
	ChatManager chatManager = null;
	ItemIcon[,] AllIcons2d = new ItemIcon[itemInfos.Length, maxItemsPerKind];
	Transform menuTransform;
	Transform itemIconParent;
	Transform itemParent;
	Network_Player player;
	Landmark currentIsland = null;
	Coroutine searchIsland = null;
	Dictionary<PickupItem, ItemIcon> iconDict = new Dictionary<PickupItem, ItemIcon>();

	Transform buttonGrid;

	public static IslandItems instance;


	private IEnumerator Start()
	{
		HNotification notification = ComponentManager<HNotify>.Value.AddNotification(HNotify.NotificationType.spinning, "Loading Island Items...");

		if (instance == null)
			instance = this;
		//[0].GetComponentsInChildren<Collider>()

		isModLoaded = false;
		Raft_Network.OnWorldReceivedLate += OnWorldRecievedLate;



		AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes("islanditems.assets"));
		yield return bundleLoadRequest;
		assetBundle = bundleLoadRequest.assetBundle;

		if (assetBundle == null) { yield break; }

		noItemsPrefab = assetBundle.LoadAsset<GameObject>("NoIslandElement");
		iconImageObject = assetBundle.LoadAsset<GameObject>("itemIcon");
		itemAmountText = assetBundle.LoadAsset<GameObject>("ItemElement");

		newCanvas = Instantiate(assetBundle.LoadAsset<GameObject>("IslandItemsCanvas"), Vector3.zero, Quaternion.identity);
		UnityEngine.Object.DontDestroyOnLoad(newCanvas);
		noItemsObject = Instantiate(noItemsPrefab, itemParent).GetComponentInChildren<Text>();
		noItemsObject.text = "no items";

		itemParent = newCanvas.transform.Find("Elements").transform;
		backGround = itemParent.transform.Find("Background").transform.Find("Image").GetComponent<RectTransform>();

		// The whole window
		settingsMenu = newCanvas.transform.Find("ButtonWindow").gameObject;

		// The scrollable area that should have buttons added to it
        scrollArea = settingsMenu.transform.Find("ScrollArea").gameObject;
        
		// The actual grid that buttons will be added to
        buttonGrid = scrollArea.transform.Find("ButtonGrid").transform;

		menuTransform = settingsMenu.transform;

		//get the item sprites
		foreach (Item_Base item_Base in ItemManager.GetAllItems())
		{
			// ContainsKey can be used to test keys before inserting
			// them.
			if (!spritesDict.ContainsKey(item_Base.UniqueName))
			{
				spritesDict.Add(item_Base.UniqueName, 1);
			}

			for (int i = 0; i < itemInfos.Length; i++)
			{
				if (item_Base.UniqueName == itemInfos[i].uniqueName)
				{
					itemInfos[i].sprite = item_Base.settings_Inventory.Sprite;
				}
			}
		}

		SetUpButtonsAndText();

		//load and set player settings
		pSettings = PlayerPrefs.GetString("TG.ISLANDITEMS.SETTINGS", new string('1', itemInfos.Length));
		if (pSettings.Length < itemInfos.Length)
		{
			pSettings = new string('1', itemInfos.Length);
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			char[] lol = pSettings.ToCharArray();
			if (lol[i] == '0')
			{
				buttons[i].isActive = false;
			}
			else
			{
				buttons[i].isActive = true;
			}

			buttons[i].activeImages.SetActive(buttons[i].isActive);
			buttons[i].inactiveImages.SetActive(!buttons[i].isActive);
		}

		//make sure everything is deactivated
		optionsActive = false;
		settingsMenu.SetActive(false);
		itemParent.gameObject.SetActive(false);
		newCanvas.SetActive(false);

		//start item and island search
		searchIsland = StartCoroutine(TimerIsland());
		StartCoroutine(TimerItems());

		OnWorldRecievedLate();

		notification.Close();

		//print mod loaded;
		Debug.Log("--------------------------------------");
		Debug.Log("IslandItemsMod loaded!");
		Debug.Log("Press J+K to open the menu");
		Debug.Log("Any problems? Open a support");
		Debug.Log("ticket containing the error");
		Debug.Log("message and ping @FranzFischer#6710");
		Debug.Log("on the Raft Modding Discord");
		Debug.Log("--------------------------------------");
		isModLoaded = true;
	}

	public void OnWorldRecievedLate()
	{
		soundManager = GameObject.FindObjectOfType<SoundManager>();
		inGame = !LoadSceneManager.IsGameSceneLoaded;
		player = RAPI.GetLocalPlayer();
		chatManager = GameObject.FindObjectOfType<ChatManager>();

		optionsActive = false;

		displayActive = true;
		settingsMenu.SetActive(false);
		itemParent.gameObject.SetActive(false);
		newCanvas.SetActive(false);
		Destroy(itemIconParent);
	}

	void SetUpButtonsAndText()
	{
		buttons = new ButtonElement[itemInfos.Length];
		itemAmountTexts = new Text[itemInfos.Length];

		menuTransform.Find("ButtonWallhack").GetComponentInChildren<Button>().onClick.AddListener(() => ToggleWallhack());
		buttonWallhack = menuTransform.Find("ButtonWallhack").gameObject.AddComponent<ButtonElement>();
		buttonWallhack.Initialize(
			buttonWallhack.transform.Find("Active").gameObject,
			buttonWallhack.transform.Find("Inactive").gameObject
		);

		menuTransform.Find("ButtonShowIcons").GetComponentInChildren<Button>().onClick.AddListener(() => ToggleShowItems());
		buttonShowIcons = menuTransform.Find("ButtonShowIcons").gameObject.AddComponent<ButtonElement>();
		buttonShowIcons.Initialize(
			buttonShowIcons.transform.Find("Active").gameObject,
			buttonShowIcons.transform.Find("Inactive").gameObject
		);

		menuTransform.Find("CloseButton").GetComponent<Button>().onClick.AddListener(() => CloseWindow());
		menuTransform.Find("CloseButton").gameObject.AddComponent<ButtonElement>();
		menuTransform.Find("ButtonPrintInfo").GetComponentInChildren<Button>().onClick.AddListener(() => PrintItemsToChat());
		menuTransform.Find("ButtonPrintInfo").gameObject.AddComponent<ButtonElement>();
		menuTransform.Find("ButtonPrintInfo").Find("ItemIcon").GetComponent<Image>().raycastTarget = false;

		//Item Buttons and Amount Texts
		for (int i = 0; i < itemInfos.Length; i++)
		{
			// Add new button
			buttons[i] = Instantiate(assetBundle.LoadAsset<GameObject>("ButtonGridElement"), buttonGrid).gameObject.AddComponent<ButtonElement>();

			buttons[i].Initialize(
				buttons[i].transform.Find("Active").gameObject,
				buttons[i].transform.Find("Inactive").gameObject
			);

			buttons[i].activeImages.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemInfos[i].sprite;
			buttons[i].inactiveImages.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemInfos[i].sprite;

			buttons[i].transform.Find("Button").GetComponent<Button>().interactable = true;
			AddListenerToButton(i);

			GameObject itemAmountTextClone = Instantiate(itemAmountText, itemParent, false);
			itemAmountTexts[i] = itemAmountTextClone.transform.Find("AmountText").GetComponent<Text>();
			itemAmountTexts[i].transform.parent.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemInfos[i].sprite;
		}
	}

	void AddListenerToButton(int i)
	{
		buttons[i].transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => ToggleButton(i));
	}

	void PrintItemsToChat()
	{
		soundManager.PlayUI_Click();
		if (Raft_Network.IsHost)
		{
			chatManager.chatFieldController.AddUITextMessage("printed item amounts", player.gameObject.GetComponent<Network_Player>().steamID);
		}

		if (currentIsland != null)
		{
			string msg = "";

			for (int i = 0; i < itemInfos.Length; i++)
			{
				if (itemAmounts[i] > 0)
				{
					msg += itemInfos[i].displayName + ": " + itemAmounts[i].ToString() + "; ";
				}
			}
			chatManager.SendChatMessage(msg, player.steamID);
		}
		else
		{
			chatManager.SendChatMessage("No Island found!", player.steamID);
		}
	}

	void ToggleShowItems()
	{
		soundManager.PlayUI_Click();
		buttonShowIcons.isActive = !buttonShowIcons.isActive;


		buttonShowIcons.activeImages.SetActive(buttonShowIcons.isActive);
		buttonShowIcons.inactiveImages.SetActive(!buttonShowIcons.isActive);

		itemParent.gameObject.SetActive(buttonShowIcons);
	}

	void ToggleButton(int i)
	{
		soundManager.PlayUI_Click();
		buttons[i].isActive = !buttons[i].isActive;

		String newSettings = "";

		for (int o = 0; o < buttons.Length; o++)
		{
			newSettings += buttons[o].isActive ? "1" : "0";
		}

		PlayerPrefs.SetString("TG.ISLANDITEMS.SETTINGS", newSettings);

		SearchForItems();
		DisplayIcons(i);
		RefreshBoxes();
	}



	void ToggleWallhack()
	{
		soundManager.PlayUI_Click();
		buttonWallhack.isActive = !buttonWallhack.isActive;
		buttonWallhack.activeImages.SetActive(buttonWallhack.isActive);
		buttonWallhack.inactiveImages.SetActive(!buttonWallhack.isActive);

		if (buttonWallhack.isActive)
		{
			DisplayIcons(0, true);
		}
		if (itemIconParent == null)
		{
			CreateIcons();
			itemIconParent.gameObject.SetActive(buttonWallhack.isActive);
		}
		else
		{
			itemIconParent.gameObject.SetActive(buttonWallhack.isActive);
		}
	}

	void CloseWindow()
	{
		soundManager.PlayUI_Click();
		RAPI.ToggleCursor(false);
		optionsActive = !optionsActive;
		settingsMenu.SetActive(false);

	}


	void ChangeRefreshInterval(bool item, string[] args)
	{
		if (args.Length != 1)
		{
			Debug.Log("wrong argument");
		}

		float newInterval;
		float.TryParse(args[0], out newInterval);

		if (!item)
		{
			iconRefreshInterval = newInterval;
		}
		else if (item)
		{
			itemRefreshInterval = newInterval;
		}

		if (newInterval < 0.5f)
		{
			Debug.Log("A too small refresh interval could lead to performance issues.Refresh interval set to: " + newInterval + " seconds");
		}
		else
		{
			Debug.Log("Refresh interval set to: " + newInterval + " seconds");
		}
	}


	void DisplayIcons(int i, bool all = false)
	{
		if (all)
		{
			for (int x = 0; x < itemInfos.Length; x++)
			{
				for (int o = 0; o < maxItemsPerKind; o++)
				{
					if (AllIcons2d[x, o] != null)
					{
						//RConsole.Log(o.ToString());
						if (buttons[i].isActive)
						{
							if (AllIcons2d[x, o].objectItem.gameObject.activeSelf)
							{
								AllIcons2d[x, o].image.enabled = true;
								AllIcons2d[x, o].outline.enabled = true;
							}
						}
						else
						{
							AllIcons2d[x, o].image.enabled = false;
							AllIcons2d[x, o].outline.enabled = false;
						}

					}
				}
			}
		}
		else
		{
			for (int o = 0; o < maxItemsPerKind; o++)
			{
				if (AllIcons2d[i, o] != null)
				{
					if (buttons[i].isActive)
					{
						if (AllIcons2d[i, o].objectItem.gameObject.activeSelf)
						{
							AllIcons2d[i, o].image.enabled = true;
							AllIcons2d[i, o].outline.enabled = true;
						}
					}
					else
					{
						AllIcons2d[i, o].image.enabled = false;
						AllIcons2d[i, o].outline.enabled = false;
					}
				}
			}
		}
	}

	bool SearchForIsland()
	{
		if (player == null)
		{
			player = RAPI.GetLocalPlayer();
			return false;
		}

		Landmark[] newLandmarks = GameObject.FindObjectsOfType<Landmark>();
		if (newLandmarks.Length >= 1)
		{

			Landmark nearestLandmark = null;
			float dist = float.MaxValue;
			for (int i = 0; i < newLandmarks.Length; i++)
			{

				if (newLandmarks[i] != null && !newLandmarks[i].name.Contains("Raft"))
				{
					if (Vector3.Distance(newLandmarks[i].transform.position, player.transform.position) < dist)
					{
						dist = Vector3.Distance(newLandmarks[i].transform.position, player.transform.position);
						nearestLandmark = newLandmarks[i];
					}
				}

			}
			if (currentIsland != nearestLandmark)
			{
				currentIsland = nearestLandmark;

				return true;
			}
			else
			{
				return false;
			}

		}
		else
		{
			return true;
		}

	}

	

	public void printItems() {
		foreach (KeyValuePair<string, int> kvp in itemsDict) {
			Debug.Log(kvp.Key);
		}
	}

	public void printSprites() {
		foreach (KeyValuePair<string, int> kvp in spritesDict) {
			Debug.Log(kvp.Key);
		}
	}

	private void SearchForItems(bool printIt = false)
	{

		AmountOfItems = 0;
		

		for (int i = 0; i < itemAmounts.Length; i++)
		{
			itemAmounts[i] = 0;
		}

		if (currentIsland != null)
		{

			for (int h = 0; h < currentIsland.landmarkItems.Length; h++)
			{
				PickupItem pi = currentIsland.landmarkItems[h].connectedBehaviourID.GetComponent<PickupItem>();				

				if (pi != null)
				{

					// ContainsKey can be used to test keys before inserting
					// them.
					if (!itemsDict.ContainsKey(pi.name))
					{
						itemsDict.Add(pi.name, 1);
					}
					
					for (int i = 0; i < itemInfos.Length; i++)
					{
						if (pi.name.Contains(itemInfos[i].unityName) || pi.name.Contains(itemInfos[i].secondUnityName))
						{
							if (buttons[i].isActive)
							{
								if (pi.gameObject.activeSelf)
								{
									itemAmounts[i]++;
								}
								else
								{
									ItemIcon clone = null;
									iconDict.TryGetValue(pi, out clone);
									if (clone != null)
									{
										Destroy(clone);
										iconDict.Remove(pi);
									}
								}
							}
						}
						else if (itemInfos[i].unityName == "Iron")
						{
							if (pi.name.Contains("Metal"))
							{
								if (buttons[i].isActive)
								{
									if (pi.gameObject.activeSelf)
										itemAmounts[i]++;
								}
							}
						}
					}
				}
			}
		}


		for (int i = 0; i < itemAmounts.Length; i++)
		{
			if (printIt)
			{
				Debug.Log("Item Number " + i + ", has " + itemAmounts[i] + " friends");
			}
			if (itemAmounts[i] > 0)
			{
				AmountOfItems++;
				itemAmountTexts[i].transform.parent.gameObject.SetActive(true);
			}
			else
			{
				itemAmountTexts[i].transform.parent.gameObject.SetActive(false);
			}
			if (AmountOfItems > 0)
			{
				itemParent.gameObject.SetActive(buttonShowIcons.isActive);
			}
			else
			{
				itemParent.gameObject.SetActive(false);
			}
		}

		backGround.sizeDelta = new Vector2(155, 20 + (AmountOfItems * 75));

		for (int i = 0; i < itemAmountTexts.Length; i++)
		{
			itemAmountTexts[i].text = itemAmounts[i].ToString();
		}
	}


	public IEnumerator TimerIsland()
	{
		yield return new WaitForSeconds(2.0f);
		StartCoroutine(TimerIsland());
		if (SearchForIsland())
		{
			CreateIcons();
		}
		else
		{
			itemParent.gameObject.SetActive(false);
		}


	}

	public IEnumerator TimerItems()
	{
		yield return new WaitForSeconds(itemRefreshInterval);
		StartCoroutine(TimerItems());
		SearchForItems();

	}


	public void RefreshBoxes()
	{
		for (int i = 0; i < buttons.Length; i++)
		{

			buttons[i].activeImages.SetActive(buttons[i].isActive);
			buttons[i].inactiveImages.SetActive(!buttons[i].isActive);
		}
	}



	public void Update()
	{
		if (!isModLoaded)
			return;
		if (!inGame)
		{
			if (buttonShowIcons.isActive)
			{
				ToggleShowItems();
			}
			if (displayActive)
			{
				displayActive = false;
				if (newCanvas != null)
				{
					newCanvas.SetActive(displayActive);
				}
			}
			if (itemIconParent != null)
			{
				Destroy(itemIconParent.gameObject);
			}
			return;
		}
		if (searchIsland == null)
		{
			try
			{
				searchIsland = StartCoroutine(TimerIsland());
			}
			catch (Exception e)
			{

			}
		}


		if (Input.GetKey(KeyCode.J))
		{
			if (currentIsland == null)
			{
				return;
			}
			else if (Input.GetKeyDown(KeyCode.K))
			{
				optionsActive = !optionsActive;
				if (optionsActive)
					soundManager.PlayUI_OpenMenu();
				settingsMenu.SetActive(optionsActive);
				itemParent.gameObject.SetActive(buttonShowIcons.isActive);
				newCanvas.SetActive(true);
				RAPI.ToggleCursor(optionsActive);
			}
		}

		iconRefreshtimer += Time.deltaTime;
		if (buttonWallhack.isActive && iconRefreshtimer > iconRefreshInterval || iconRefreshInterval < .01f && buttonWallhack.isActive)
		{
			iconRefreshtimer = 0.0f;
			RefreshWallhackPosition();
		}
	}

	void RefreshWallhackPosition()
	{
		try
		{

			if (currentIsland == null)
			{
				//Try clean items before return
				Debug.Log("No island - Clean items");
				if (itemIconParent.gameObject != null)
				{
					Debug.Log("Why the hell were there still items on there");
					Destroy(itemIconParent.gameObject);
				}

				return;
			}
		}
		catch (Exception e) { }

		ItemIcon buffIcon; ;
		bool showIcon = false;

		for (int i = 0; i < itemInfos.Length; i++)
		{
			if (buttons[i].isActive)
			{
				for (int o = 0; o < maxItemsPerKind; o++)
				{
					if (AllIcons2d[i, o] != null)
					{
						buffIcon = AllIcons2d[i, o];
						showIcon = buffIcon.objectItem.gameObject.activeSelf;

						if (showIcon)
						{
							if (Vector3.Dot(Camera.main.transform.forward, (buffIcon.objectItem.transform.position - Camera.main.transform.position).normalized) <= 0)
							{
								showIcon = false;
							}
						}

						buffIcon.image.enabled = showIcon;
						buffIcon.outline.enabled = showIcon;

						buffIcon.transform.position = Camera.main.WorldToScreenPoint(buffIcon.objectItem.transform.position);
					}
				}
			}
		}
	}

	// This variable is automatically changed.
	static bool ExtraSettingsAPI_Loaded = false; // This is set to true while the mod's settings are loaded

	// Occurs when the API loads the mod's settings
	public void ExtraSettingsAPI_Load() 
	{
		iconScale = ExtraSettingsAPI_GetSliderValue("iconScale");
	}

	// Occurs when user closes the settings menu
	public void ExtraSettingsAPI_SettingsClose() 
	{
		iconScale = ExtraSettingsAPI_GetSliderValue("iconScale");
		CreateIcons();
	}

	// Occurs when a slider setting is set to the "custom" type is changed.
	// Returned string is shown on the slider's value display.
	// Method can be changed to return any object and the returned object will be converted to a string via "ToString()"
	// "name" is the setting's name and "value" is the slider's real value
	public float ExtraSettingsAPI_HandleSliderText(string name, float value)
	{
		iconScale = ExtraSettingsAPI_GetSliderValue("iconScale");
		return value;
	}

	// Occurs when a settings button is clicked. "name" is set the the button's name
	public void ExtraSettingsAPI_ButtonPress(string name) 
	{
		if (name == "buttonDisplay")
		{
			iconScale = ExtraSettingsAPI_GetSliderValue("iconScale");
			CreateIcons();
		}
	}

	void CreateIcons()
	{
		if (itemIconParent != null)
		{
			Destroy(itemIconParent.gameObject);
		}
		if (currentIsland == null)
			return;

		itemIconParent = new GameObject().transform;
		itemIconParent.parent = newCanvas.transform;
		itemIconParent.SetAsFirstSibling();
		itemIconParent.gameObject.SetActive(buttonWallhack.isActive);
		iconDict.Clear();

		itemIconParent.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		int[] itemCounter = new int[itemInfos.Length];

		Color iconColor = new Color(1, 1, 1, ICON_TRANSPARENCY);

		ItemIcon buffItemIcon;

		for (int h = 0; h < currentIsland.landmarkItems.Length; h++)
		{
			PickupItem item = currentIsland.landmarkItems[h].connectedBehaviourID.GetComponent<PickupItem>();

			if (item != null)
			{
				for (int i = 0; i < itemInfos.Length; i++)
				{
					if ((item.name.Contains(itemInfos[i].unityName) || item.name.Contains(itemInfos[i].secondUnityName)) && item.gameObject.activeSelf)
					{
						try{
						buffItemIcon = Instantiate(iconImageObject, itemIconParent).AddComponent<ItemIcon>();

						buffItemIcon.objectItem = item;

						buffItemIcon.image = buffItemIcon.GetComponentInChildren<Image>();
						buffItemIcon.image.color = iconColor;
						buffItemIcon.image.sprite = itemInfos[i].sprite;
						buffItemIcon.image.transform.localScale *= iconScale;

						buffItemIcon.outline = Instantiate(buffItemIcon.image, buffItemIcon.image.transform.parent);
						buffItemIcon.outline.transform.SetAsFirstSibling();
						buffItemIcon.outline.color = Color.black;
						buffItemIcon.outline.transform.localScale *= 1.15f;

						itemCounter[i]++;
						AllIcons2d[i, itemCounter[i]] = buffItemIcon;
						iconDict.Add(item, buffItemIcon);
						}
						catch(Exception e){}
					}
				}
			}
		}
	}

	public void OnModUnload()
	{
		assetBundle.Unload(true);
		Destroy(itemIconParent.gameObject);
		Destroy(newCanvas.gameObject);
		Debug.Log("IslandItemsMod has been unloaded!");
	}

	[ConsoleCommand(name: "itemrefresh", docs: "Sets the refresh interval for the display")]
	public static void itemrefreshCommand(string[] args)
	{
		instance.ChangeRefreshInterval(true, args);
	}

	[ConsoleCommand(name: "iconrefresh", docs: "Sets the refresh interval for the hackicons")]
	public static void iconrefreshCommand(string[] args)
	{
		instance.ChangeRefreshInterval(false, args);
	}

	[ConsoleCommand(name: "recreateitemicons", docs: "Clears all the icons on the screen and recreates them to fix items that are stuck on screen")]
	public static void IconRefreshCommand(string[] args)
	{
		if (instance.itemIconParent == null)
		{
			Destroy(instance.itemIconParent.gameObject);
		}
		instance.CreateIcons();
		instance.itemIconParent.gameObject.SetActive(instance.buttonWallhack.isActive);
	}
	
	[ConsoleCommand(name: "resetIconScale", docs: "Resets the Icons scale. Helpful if you accidentally set it too large")]
	public static void resetSettings(string[] args)
	{
		ExtraSettingsAPI_ResetAllSettings();
		iconScale = ExtraSettingsAPI_GetSliderValue("iconScale");
		instance.CreateIcons();
		Debug.Log("Icon scale reset to default value");
	}

	[ConsoleCommand(name: "printItemNames", docs: "print pickup items to the console log")]
	public static void printItemNames(string[] args)
	{
		instance.printItems();
	}

	[ConsoleCommand(name: "printItemSprites", docs: "print pickup items to the console log")]
	public static void printItemSprites(string[] args)
	{
		instance.printSprites();
	}



	// Use to reset all settings to their default values
	public static void ExtraSettingsAPI_ResetAllSettings() { }

	// Use to get the current value from a Slider type setting
	// This method returns the value of the slider rounded according to the mod's setting configuration
	public static float ExtraSettingsAPI_GetSliderValue(string SettingName) => 0;

}

public class ItemInfo
{
	public readonly string unityName;
	public readonly string uniqueName;
	public readonly string displayName;
	public readonly string secondUnityName;
	public Sprite sprite;
	public ItemInfo(string unityName, string uniqueName, string displayName)
	{
		this.unityName = unityName;
		this.uniqueName = uniqueName;
		this.displayName = displayName;
		this.secondUnityName = unityName;
	}
	public ItemInfo(string unityName, string uniqueName, string displayName, string secondUnityName)
	{
		this.unityName = unityName;
		this.uniqueName = uniqueName;
		this.displayName = displayName;
		this.secondUnityName = secondUnityName;
	}
}

class ItemIcon : MonoBehaviour
{
	public Image image;
	public Image outline;

	public PickupItem objectItem;
}

public class ButtonElement : MonoBehaviour, IPointerEnterHandler
{
	public bool isActive;

	public GameObject activeImages;
	public GameObject inactiveImages;

	public void Initialize(GameObject activeImages, GameObject inactiveImages)
	{
		this.isActive = false;
		this.activeImages = activeImages;
		this.inactiveImages = inactiveImages;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		IslandItems.instance.soundManager.PlayUI_Highlight();
	}

}