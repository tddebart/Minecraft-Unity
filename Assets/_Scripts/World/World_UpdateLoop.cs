using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class World
{
    public Queue<Block> blockToUpdate = new Queue<Block>();
    
    public IEnumerator UpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f / 4.0f);
            UpdateBlocks();
        }
    }

    public void UpdateBlocks()
    {
        while (blockToUpdate.Count > 0)
        {
            Block block = blockToUpdate.Dequeue();
            block.OnBlockUpdate();
        }
    }
}
