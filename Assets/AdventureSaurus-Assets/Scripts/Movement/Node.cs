using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { None, Wall, Ally, Enemy, Interactable};

public class Node
{
    public Vector2Int position; // The position of the node in 2D grid space
    public Node whereToGo;  // The node that a unit on this node should move to if they want to get to the desired location
    public CharacterType occupying; // What is currently on this tile

    // A* Stuff
    public int G;   // Distance between current node and the start node
    public int H;   // Heuristic - estimated distance from the current node to the end node
    public int F;   // Total cost of the node (H + G)
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

/// <summary>
/// For sorting a list of Nodes by their F
/// </summary>
public class NodeComp : IComparer<Node>
{
    // Compares two nodes based on their F
    public int Compare(Node n0, Node n1)
    {
        if (n0 == null && n1 == null)
        {
            return 0;
        }
        else if (n0 == null)
        {
            return 0.CompareTo(n1.F);
        }
        else if (n1 == null)
        {
            return n0.F.CompareTo(0);
        }
        return n0.F.CompareTo(n1.F);
    }
}
