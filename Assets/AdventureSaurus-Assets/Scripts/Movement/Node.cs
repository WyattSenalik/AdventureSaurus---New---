using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { None, Wall, Ally, Enemy};

public class Node
{
    public Vector2Int position; // The position of the node in 2D grid space
    public Node whereToGo;  // The node that a unit on this node should move to if they want to get to the desired location
    public CharacterType occupying; // What is currently on this tile

    // A* Stuff
    public int G;   // Distance between current node and the start node
    public int H;   // Heuristic - estimated distance from the current node to the end node
    public int F;   // Total cost of the node (F + G)
    public Node parent; // The parent of this node

    /// <summary>
    /// Constructor for Node
    /// </summary>
    /// <param name="_pos_">The position of the new node</param>
    public Node(Vector2Int _pos_)
    {
        position = _pos_;
        whereToGo = null;
        occupying = CharacterType.None;
        G = 0;
        H = 0;
        F = 0;
        parent = null;
    }
}
