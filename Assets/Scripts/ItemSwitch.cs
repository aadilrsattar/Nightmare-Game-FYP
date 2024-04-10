using UnityEngine;
using Mirror;
using System.Collections;

public class ItemSwitch : NetworkBehaviour
{
    public GameObject[] itemPrefabs;
    private GameObject[] items;
    private bool itemsInitialized = false;
    private bool pendingActivation = false;

    [SyncVar(hook = nameof(OnActiveItemIndexChanged))]
    private int activeItemIndex = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(InitializeItemsAfterDelay());
    }

    private IEnumerator InitializeItemsAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Wait for a brief moment

        InitializeItems();

        // Immediately activate the initial item if it's not the first item
        if (activeItemIndex != 0)
        {
            ActivateItem(activeItemIndex);
        }
        else if (items.Length > 0)
        {
            // For the first item, force activation to ensure visibility
            pendingActivation = true; // Ensure the flag is set to trigger activation in InitializeItems
            ActivateItem(0); // Directly activate the first item
        }
    }

    private void InitializeItems()
    {
        items = new GameObject[itemPrefabs.Length];
        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            GameObject item = Instantiate(itemPrefabs[i], transform.position + transform.forward * (i + 1), transform.rotation);
            item.transform.SetParent(transform);
            Vector3 offset = new Vector3(0.25f, 0.95f, 0.5f);
            item.transform.localPosition = offset;
            if (i != 4 && i != 5)
            {
                item.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                // For 4th and 5th elements, keep the rotation as Quaternion.identity (no rotation)
                item.transform.localRotation = Quaternion.identity;
            }
            item.SetActive(false); // Initially disable the item
            items[i] = item;
        }

        itemsInitialized = true;

        // If activation was pending, activate the appropriate item now
        if (pendingActivation)
        {
            ActivateItem(activeItemIndex);
            pendingActivation = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { CmdSwitchItem(0); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { CmdSwitchItem(2); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { CmdSwitchItem(4); }
        else if (Input.GetKeyDown(KeyCode.F)) { CmdSwitchItem(5); }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) { CmdSwitchItem(-1); }
    }

    [Command]
    public void CmdSwitchItem(int index)
    {
        activeItemIndex = index; // This will automatically update and trigger the hook for all clients
    }

    void OnActiveItemIndexChanged(int oldIndex, int newIndex)
    {
        if (itemsInitialized)
        {
            ActivateItem(newIndex);
        }
        else
        {
            // Mark that an activation is pending until items are initialized
            pendingActivation = true;
        }
    }

    private void ActivateItem(int index)
    {
        // Ensure all items are deactivated before activating the new one
        foreach (GameObject item in items)
        {
            if (item != null) item.SetActive(false);
        }

        if (items == null || index < -1 || index >= items.Length)
        {
            Debug.LogError("ActivateItem: Attempt to access uninitialized array or out of range index.");
            return;
        }

        if (index == -1)
        {
            return;
        }
        else { items[index].SetActive(true); }

    }

    public void SwitchToAlternateItem()
    {
        if (activeItemIndex == 0) // Check if the player is holding item 1 (index 0)
        {
            int alternateIndex = (activeItemIndex + 1) % itemPrefabs.Length; // Example logic to switch to the next item
            CmdSwitchItem(alternateIndex); // Command to switch item
        }
    }
    public int GetActiveItemIndex()
    {
        return activeItemIndex;
    }

}