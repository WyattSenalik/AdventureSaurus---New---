using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { None, Wall, Ally, Enemy, Interactable};

public class Node
{
    // The position of the node in 2D grid space
    private Vector2Int _position;
    public Vector2Int Position
    {
        get { return _position; }
    }
    // The node that a unit on this node should move to if they want to get to the desired location
    private Node _whereToGo;
    public Node WhereToGo
    {
        get { return _whereToGo; }
        set { _whereToGo = value; }
    }
    // The node that a unit on this node came from
    private Node _parent;
    public Node Parent
    {
        get { return _parent; }
        set { _parent = value; }
    }
    // What is currently on this tile
    private CharacterType _occupying;
    public CharacterType Occupying
    {
        get { return _occupying; }
        set { _occupying = value; }
    }

    // A* Stuff
    // G of come from node plus 1. First is 0
    private int _g;
    public int G
    {
        get { return _g; }
        set { _g = value; }
    }
    // Heuristic - estimated distance from the current node to the end node
    private int _h;
    public int H
    {
        get { return _h; }
        set { _h = value; }
    }
    // Total cost of the node (H + G)
    private int _f;
    public int F
    {
        get { return _f; }
        set { _f = value; }
    }

    /// <summary>
    /// Constructor for Node
    /// </summary>
    /// <param name="_pos_">The position of the new node</param>
    public Node(Vector2Int _pos_)
    {
        _position = _pos_;
        _whereToGo = null;
        _parent = null;
        _occupying = CharacterType.None;
        _g = 0;
        _h = 0;
        _f = 0;
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
        // If both nodes are null
        if (n0 == null && n1 == null)
        {
            return 0;
        }
        // If the first node is null
        else if (n0 == null)
        {
            return n1.F.CompareTo(0);
        }
        // If the second node is null
        else if (n1 == null)
        {
            return 0.CompareTo(n0.F);
        }
        // If both nodes are not null, compare their F's normally
        return n1.F.CompareTo(n0.F);
    }
}
