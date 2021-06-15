using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SaveSystem : ScriptableObject
{
	[SerializeField] private LoadEventChannelSO _loadLocation = default;
	[SerializeField] private Inventory _playerInventory;

	public string saveFilename = "save.chop";
	public string backupSaveFilename = "save.chop.bak";
	public Save saveData = new Save();

	void OnEnable()
	{
		Debug.Log("On enable");
		_loadLocation.OnLoadingRequested += CacheLoadLocations;
	}

	void OnDisable()
	{
		_loadLocation.OnLoadingRequested -= CacheLoadLocations;
	}

	public void CacheLoadLocations(GameSceneSO locationsToLoad, bool showLoadingScreen)
	{
		LocationSO locationSO = locationsToLoad as LocationSO;
		//if (locationSO)
		//{
			saveData._locationId = StaticClass.UserID;
		Debug.Log($"saveData._locationId :{saveData._locationId}StaticClass.UserID :{StaticClass.UserID}");
		//}

		SaveDataToDisk();
	}

	public bool LoadSaveDataFromDisk()
	{
		if (FileManager.LoadFromFile(saveFilename, out var json))
		{
			Debug.Log($"Load System {json}");
			saveData.LoadFromJson(json);
			return true;
		}

		return false;
	}

	public IEnumerator LoadSavedInventory()
	{
		_playerInventory.Items.Clear();
		foreach (var serializedItemStack in saveData._itemStacks)
		{
			var loadItemOperationHandle = Addressables.LoadAssetAsync<Item>(serializedItemStack.itemGuid);
			yield return loadItemOperationHandle;
			if (loadItemOperationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var itemSO = loadItemOperationHandle.Result;
				_playerInventory.Add(itemSO, serializedItemStack.amount);
			}
		}
	}

	public void SaveDataToDisk()
	{
		saveData._itemStacks.Clear();
		foreach (var itemStack in _playerInventory.Items)
		{
			//	saveData._itemStacks.Add(new SerializedItemStack(itemStack.Item.Guid, itemStack.Amount));
		}

		if (FileManager.MoveFile(saveFilename, backupSaveFilename))
		{
			if (FileManager.WriteToFile(saveFilename, saveData.ToJson()))
			{
				Debug.Log("Save successful");
			}
		}
	}

	public void WriteEmptySaveFile()
	{
		FileManager.WriteToFile(saveFilename, "");
	}
}
