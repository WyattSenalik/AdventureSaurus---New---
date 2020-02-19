using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { None, Wall, Ally, Enemy};

public class Node
{
    public Vector2Int position; // The position of the node in 2D grid space
    public Node whereToGo;  // The node that a unit on this node should move to if they want to get to the desired location
    public CharacterType occupying; // What is currently on this tile

    public Node(Vector2Int _pos_)
    {
        position = _pos_;
        whereToGo = null;
        occupying = CharacterType.None;
    }
}
