using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class World
{
    public Queue<Block> blockToUpdate = new Queue<Block>();
    
    public IEnumerator UpdateLoop()
    {
        // while (true)
        // {
        //     yield return new WaitForSeconds(1.0f / 4.0f);
        //     new Thread(() =>
        //     {
        //         while (blockToUpdate.Count > 0)
        //         {
        //             Block block = blockToUpdate.Dequeue();
        //             block.OnBlockUpdate();
        //         }
        //     }).Start();
        // }
        yield return null;
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
