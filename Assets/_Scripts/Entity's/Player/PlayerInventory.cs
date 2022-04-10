using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public GameObject UIBlockPrefab;
    public GameObject SlotTextPrefab;
    
    public Slot[] slots = new Slot[slotAmount];
    
    private Player player;
    private RectTransform hotbarRect;
    private RectTransform selectedSlotRect;
    private const int slotAmount = 9*4;
    private const int maxStackSize = 64;
    
    public bool isOpen;
    private RectTransform inventoryRect;
    private RectTransform inventorySlotsRect;
    private RectTransform canvasRect;
    [HideInInspector]
    public RectTransform inventoryHighlightRect;
    private RectTransform pickedUpItemRect;
    
    [CanBeNull] public Slot pickedUpSlot;
    
    [HideInInspector]
    public int selectedSlotIndex;
    
    private void Start()
    {
        player = GetComponent<Player>();
        hotbarRect = GameObject.Find("PlayerCanvas/GameUI/Hotbar").GetComponent<RectTransform>();
        selectedSlotRect = hotbarRect.GetChild(0).GetComponent<RectTransform>();
        
        canvasRect = GameObject.Find("PlayerCanvas").GetComponent<RectTransform>();
        inventoryRect = GameObject.Find("PlayerCanvas/GameUI/Inventory").GetComponent<RectTransform>();
        inventorySlotsRect = inventoryRect.GetChild(3).GetComponent<RectTransform>();
        inventoryHighlightRect = inventoryRect.GetChild(2).GetComponent<RectTransform>();
        inventoryHighlightRect.gameObject.SetActive(false);
        pickedUpItemRect = inventoryRect.GetChild(4).GetComponent<RectTransform>();

        var emptyObj = new GameObject();
        
        // Create inventory slots
        for (var j = 0; j < 4; j++)
        {
            for (var i = 0; i < 9; i++)
            {
                var slotObj = Instantiate(emptyObj, inventorySlotsRect);
                slotObj.AddComponent<RectTransform>();
                slotObj.AddComponent<Image>().color = Color.clear;
                var text = Instantiate(SlotTextPrefab, slotObj.transform);
                var slotRect = slotObj.GetComponent<RectTransform>();
                slotRect.localPosition = new Vector3(i * 18-72f, (j==0 ? -19-16*3 : (-j+1) * 18-9), -35);
                slotRect.sizeDelta = new Vector2(18, 18);
                slotRect.name = "Slot" + j + i;
                var invSlot = slotObj.AddComponent<InventorySlot>();
                invSlot.playerInventory = this;
                invSlot.slotIndex = i + j * 9;
                
                slots[i + j*9] = new Slot(i+j*9);
                slots[i + j*9].countText = text.GetComponent<TextMeshProUGUI>();
                if (j == 0)
                {
                    slots[i + j*9].hotbarCountText = hotbarRect.GetChild((i + j*9 )+ 1).GetChild(0).GetComponent<TextMeshProUGUI>();
                }
            }
        }
        Destroy(emptyObj);
        
        
        inventoryRect.gameObject.SetActive(false);

        AddItem(BlockType.GlowStone, 64);
    }

    private void Update()
    {
        UpdateSelectedSlotUI();

        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlotIndex = i;
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            AddItem(BlockType.WoodenSlab, 64);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        if (pickedUpSlot != null)
        {
            pickedUpItemRect.gameObject.SetActive(true);
            var position = Input.mousePosition;
            position.x /= Screen.width;
            position.y /= Screen.height;
            position.x *= canvasRect.sizeDelta.x;
            position.y *= canvasRect.sizeDelta.y;
            position.z = -50;
            pickedUpItemRect.anchoredPosition = position;
        }
        else
        {
            pickedUpItemRect.gameObject.SetActive(false);
        }
    }
    
    public void OpenInventory()
    {
        isOpen = true;
        inventoryRect.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void CloseInventory()
    {
        isOpen = false;
        inventoryRect.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    #region Inventory management
    
    public void PickUpSlot(Slot slot)
    {
        Debug.Log("picked up slot");
        if (pickedUpSlot != null)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            // If in hotbar
            if (slot.index < 9)
            {
                if (AddItem(slot.type, slot.count, true))
                {
                    RemoveItem(slot.index);
                }
            }
            else
            {
                if (AddItem(slot.type, slot.count, false, true))
                {
                    RemoveItem(slot.index);
                }
            }
        }
        else
        {
            pickedUpSlot = new Slot(slot);
            SetSlotBlockType(slot.index, BlockType.Nothing);
            slot.SetCount(0);
            UpdatePickedUpSlotVisual();
        }

    }
    
    public void CombineSlots(Slot slot)
    {
        Debug.Log("combined slots");   
        if (pickedUpSlot == null)
        {
            return;
        }

        if (pickedUpSlot.type == slot.type)
        {
            if (pickedUpSlot.count + slot.count <= maxStackSize)
            {
                slot.SetCount(slot.count + pickedUpSlot.count);
                pickedUpSlot = null;
            }
            else
            {
                slot.SetCount(maxStackSize);
                pickedUpSlot.SetCount(pickedUpSlot.count - (maxStackSize - slot.count));
            }
        }
    }
    
    public void SwapSlots(Slot slot)
    {
        Debug.Log("swapped slots");
        if (pickedUpSlot == null)
        {
            return;
        }

        var tempSlot = new Slot(pickedUpSlot);
        pickedUpSlot = new Slot(slot);
        UpdatePickedUpSlotVisual();
        
        
        SetSlotBlockType(slot.index, tempSlot.type);
        slots[slot.index].SetCount(tempSlot.count);
    }
    
    public void PickUpHalfSlot(Slot slot)
    {
        Debug.Log("picked up half slot");
        if (pickedUpSlot != null)
        {
            return;
        }

        pickedUpSlot = new Slot(slot);
        pickedUpSlot.SetCount(Mathf.CeilToInt(pickedUpSlot.count / 2f));
        slots[slot.index].SetCount(Mathf.FloorToInt(slots[slot.index].count / 2f));
        
        UpdatePickedUpSlotVisual();
    }

    public void DropSlot()
    {
        Debug.Log("dropped slot");
        if (pickedUpSlot == null)
        {
            return;
        }

        var slot = pickedUpSlot;
        pickedUpSlot = null;
        SetSlotBlockType(slot.index, slot.type);
        slots[slot.index].SetCount(slot.count);
    }

    public void DropOneItemSlot()
    {
        Debug.Log("dropped one slot");
        if (pickedUpSlot == null)
        {
            return;
        }

        pickedUpSlot.SetCount(pickedUpSlot.count - 1);
        if (pickedUpSlot.count <= 0)
        {
            pickedUpSlot = null;
        }

        if (slots[pickedUpSlot!.index].type == BlockType.Nothing)
        {
            SetSlotBlockType(pickedUpSlot.index, pickedUpSlot.type);
        }
        slots[pickedUpSlot!.index].SetCount(slots[pickedUpSlot.index].count + 1);
    }

    public void UpdatePickedUpSlotVisual()
    {
        if (pickedUpSlot == null) return;
        
        // Destroy any existing block mesh
        if(pickedUpItemRect.childCount > 1)
        {
            Destroy(pickedUpItemRect.GetChild(1).gameObject);
        }

        CreateBlockMesh(pickedUpSlot.type, pickedUpItemRect);
        pickedUpSlot.countText = pickedUpItemRect.GetChild(0).GetComponent<TextMeshProUGUI>();
        pickedUpSlot!.SetCount(pickedUpSlot.count);
    }
    
    #endregion
    
    public bool AddItem(BlockType blockType, int count = 1, bool onlyInventory = false, bool onlyHotbar = false)
    {
        for (int i = onlyInventory ? 9 : 0; i < (onlyHotbar ? 9 : slotAmount); i++)
        {
            if (slots[i].type == blockType)
            {
                if(slots[i].count + count <= maxStackSize)
                {
                    slots[i].SetCount(slots[i].count + count);
                    return true;
                }
            }
        }
        
        for (int i = onlyInventory ? 9 : 0; i < (onlyHotbar ? 9 : slotAmount); i++)
        {
            if (slots[i].type == BlockType.Nothing)
            {
                SetSlotBlockType(i,blockType);
                slots[i].SetCount(count);
                return true;
            }
        }

        return false;
    }
    
    public void RemoveHeldItem(int count = 1)
    {
        slots[selectedSlotIndex].SetCount(slots[selectedSlotIndex].count - count);
        if (slots[selectedSlotIndex].count <= 0)
        {
            SetSlotBlockType(selectedSlotIndex,BlockType.Nothing);
        }
    }

    public void RemoveItem(int index)
    {
        slots[index].SetCount(0);
        SetSlotBlockType(index, BlockType.Nothing);
    }

    private void UpdateSelectedSlotUI()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            selectedSlotIndex = (selectedSlotIndex + 1) % 9;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            selectedSlotIndex = (selectedSlotIndex + 9-1) % 9;
        }

        selectedSlotRect.localPosition = new Vector3(selectedSlotIndex * 20 - 80, -1, 0);
    }

    public void SetSlotBlockType(int slot, BlockType type)
    {
        slots[slot].type = type;
        // If slot is in hotbar
        if (slot < 9)
        {
            if (hotbarRect.GetChild(slot + 1).childCount > 1)
            {
                var existingItem = hotbarRect.GetChild(slot + 1).GetChild(1);
                if (existingItem != null)
                {
                    Destroy(existingItem.gameObject);
                }
            }
            CreateBlockMesh(type,hotbarRect.GetChild(slot + 1));
        }
        
        // Create inventory slot mesh
        var parent = inventorySlotsRect.GetChild(slot);
        if(parent.childCount > 1)
        {
            var existingItem = parent.GetChild(1);
            if (existingItem != null)
            {
                Destroy(existingItem.gameObject);
            }
        }
        
        CreateBlockMesh(type,parent);
    }

    private void CreateBlockMesh(BlockType type, Transform parent)
    {
        if (type == BlockType.Nothing) return;
        
        GameObject blockItem = Instantiate(UIBlockPrefab, parent);
        blockItem.transform.localScale = Vector3.one*10;
        blockItem.GetComponent<RectTransform>().anchoredPosition = new Vector3(-7, -5, 0);
        var meshData = new MeshData(true);
        var block = new Block(type, Vector3Int.zero, null);
        block = BlockMapping.MapTypeToBlock(type, block);
        foreach (var direction in BlockHelper.directions)
        {
            BlockHelper.GetFaceDataIn(direction, Vector3Int.zero, meshData, block, BlockDataManager.blockTypeDataDictionary[(int)type], null);
            switch (direction)
            {
                case Direction.backwards:
                    meshData.skyLight.Add(13);
                    meshData.skyLight.Add(13);
                    meshData.skyLight.Add(13);
                    meshData.skyLight.Add(13);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    break;
                case Direction.right:
                    meshData.skyLight.Add(10);
                    meshData.skyLight.Add(10);
                    meshData.skyLight.Add(10);
                    meshData.skyLight.Add(10);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    break;
                default:
                    meshData.skyLight.Add(14);
                    meshData.skyLight.Add(14);
                    meshData.skyLight.Add(14);
                    meshData.skyLight.Add(14);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    meshData.sides.Add(Vector3.zero);
                    break;
            }
        }
        
        var mesh = new Mesh();
        mesh.SetVertices(meshData.vertices);
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.SetUVs(0, meshData.uv);
        mesh.RecalculateNormals();
        var lighArray = new Vector2[meshData.skyLight.Count];
        for (int i = 0; i < meshData.skyLight.Count; i++)
        {
            lighArray[i] = new Vector2(meshData.skyLight[i], 0);
        }

        mesh.SetUVs(1, lighArray);
        mesh.SetUVs(2, meshData.sides);
        blockItem.GetComponent<MeshFilter>().mesh = mesh;
    }
    
    public class Slot
    {
        public BlockType type;
        public int count { get; private set; }
        public int index;
        public TextMeshProUGUI countText;
        // do a separate text for the hotbar
        [CanBeNull] public TextMeshProUGUI hotbarCountText;

        public Slot(int index)
        {
            type = BlockType.Nothing;
            count = 0;
            this.index = index;
        }

        public Slot(Slot slot)
        {
            type = slot.type;
            count = slot.count;
            index = slot.index;
        }
        
        public void SetCount(int count)
        {
            this.count = count;
            if (count > 1)
            {
                if(countText != null)
                {
                    countText.text = count.ToString();
                }
                if(hotbarCountText != null)
                {
                    hotbarCountText.text = count.ToString();
                }
            }
            else
            {
                if (countText != null)
                {
                    countText.text = "";
                }
                if(hotbarCountText != null)
                {
                    hotbarCountText.text = "";
                }
            }
        }
    }
}