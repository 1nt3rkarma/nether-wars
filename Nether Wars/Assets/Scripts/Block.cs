using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Structure
{
    public BlockOptions option = BlockOptions.cap;

    private void Start()
    {
        defaultColor = renderer.material.color;
    }
}
