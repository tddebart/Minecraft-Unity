using System;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GameObject UIBlockPrefab;
    
    public Slot[] slots = new Slot[slotAmount];
    
    private Player player;
    private RectTransform hotbarRect;
    private RectTransform selectedSlotRect;
    private const int slotAmount = 9;
    private const int maxStackSize = 64;
    
    [HideInInspector]
    public int selectedSlot;
    
    private void Start()
    {
        player = GetComponent<Player>();
        hotbarRect = GameObject.Find("PlayerCanvas/Hotbar").GetComponent<RectTransform>();
        selectedSlotRect = hotbarRect.GetChild(0).GetComponent<RectTransform>();
        for (int i = 0; i < slotAmount; i++)
        {
            slots[i] = new Slot();
        }

        AddItem(BlockType.GlowStone, 64);
    }

    private void Update()
    {
        UpdateSelectedSlotUI();

        for (int i = 0; i < slotAmount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlot = i;
            }
        }
    }
    
    public void AddItem(BlockType blockType, int count = 1)
    {
        for (int i = 0; i < slotAmount; i++)
        {
            if (slots[i].type == blockType)
            {
                if(slots[i].count + count <= maxStackSize)
                {
                    slots[i].SetCount(slots[i].count + count);
                    return;
                }
            }
        }
        
        for (int i = 0; i < slotAmount; i++)
        {
            if (slots[i].type == BlockType.Nothing)
            {
                SetSlotBlockType(i,blockType);
                slots[i].SetCount(count);
                return;
            }
        }
    }
    
    public void RemoveHeldItem(int count = 1)
    {
        slots[selectedSlot].SetCount(slots[selectedSlot].count - count);
        if (slots[selectedSlot].count <= 0)
        {
            SetSlotBlockType(selectedSlot,BlockType.Nothing);
        }
    }

    private void UpdateSelectedSlotUI()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            selectedSlot = (selectedSlot + 1) % slotAmount;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            selectedSlot = (selectedSlot + slotAmount-1) % slotAmount;
        }

        selectedSlotRect.localPosition = new Vector3(selectedSlot * 20 - 80, -1, 0);
    }

    public void SetSlotBlockType(int slot, BlockType type)
    {
        if (hotbarRect.GetChild(slot + 1).childCount > 0)
        {
            var existingItem = hotbarRect.GetChild(slot + 1).GetChild(0);
            if (existingItem != null)
            {
                Destroy(existingItem.gameObject);
            }
        }
        
        slots[slot].type = type;

        if (type != BlockType.Nothing)
        {
            GameObject blockItem = Instantiate(UIBlockPrefab, hotbarRect.GetChild(slot+1));
            var meshData = new MeshData(true);
            foreach (var direction in BlockHelper.directions)
            {
                BlockHelper.GetFaceDataIn(direction, Vector3Int.zero, meshData, type,
                    BlockDataManager.textureDataDictionary[(int)type], null);
            }
            
            var mesh = new Mesh();
            mesh.SetVertices(meshData.vertices);
            mesh.SetTriangles(meshData.triangles, 0);
            mesh.SetUVs(0, meshData.uv);
            mesh.RecalculateNormals();
            blockItem.GetComponent<MeshFilter>().mesh = mesh;
            slots[slot].countText = blockItem.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public class Slot
    {
        public BlockType type;
        public int count { get; private set; }
        public TextMeshProUGUI countText;

        public Slot()
        {
            type = BlockType.Nothing;
            count = 0;
        }
        
        public void SetCount(int count)
        {
            this.count = count;
            if(count > 1)
                countText.text = count.ToString();
            else
                countText.text = "";
        }
    }
}