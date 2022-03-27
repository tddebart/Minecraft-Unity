using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public PlayerInventory playerInventory;
    public int slotIndex;
    public PlayerInventory.Slot mySlot => playerInventory.slots[slotIndex];
    public bool selected;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var highlight = playerInventory.inventoryHighlightRect;
        var pos = transform.position;
        pos.z += 30;
        highlight.transform.position = pos;
        highlight.gameObject.SetActive(true);
        selected = true;

        if (mySlot.type != BlockType.Nothing && Input.GetMouseButton(0))
        {
            playerInventory.PickUpSlot(mySlot);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        playerInventory.inventoryHighlightRect.gameObject.SetActive(false);
        selected = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if (mySlot.type != BlockType.Nothing)
            {
                if (playerInventory.pickedUpSlot == null)
                {
                    playerInventory.PickUpSlot(mySlot);
                }
                else if(playerInventory.pickedUpSlot.type == mySlot.type)
                {
                    playerInventory.CombineSlots(mySlot);
                } else
                {
                    playerInventory.SwapSlots(mySlot);
                }
            }
            else if(selected && playerInventory.pickedUpSlot != null)
            {
                playerInventory.pickedUpSlot.index = slotIndex;
                playerInventory.DropSlot();
            }
        } else if(eventData.button == PointerEventData.InputButton.Right)
        {
            if (playerInventory.pickedUpSlot == null)
            {
                if (mySlot.type != BlockType.Nothing)
                {
                    playerInventory.PickUpHalfSlot(mySlot);
                }
            }
            else
            {
                if (mySlot.type == BlockType.Nothing || mySlot.type == playerInventory.pickedUpSlot.type)
                {
                    playerInventory.pickedUpSlot.index = slotIndex;
                    playerInventory.DropOneItemSlot();
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}