using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingEnemy : SingleEnemy
{
    /// <summary>
    /// Finds and returns the tile this enemy should move to.
    /// Find the closest tile they can attack an enemy from.
    /// </summary>
    /// <returns>Node that this enemy should move to</returns>
    override protected Node FindTileToMoveTo()
    {
        Debug.Log("------------------------------------------");
        Debug.Log("Start Node " + StandingNode.Position);
        // 1) Get closest ally node with ana attackable node
        Node closestAllyNode = null;
        int closestAllyF = int.MaxValue;
        Node closestAttackNode = null;
        int closestAttackF = int.MaxValue;
        // Iterate over each character to find the closest
        foreach (Transform charTrans in CharacterParent)
        {
            // Get the node that character is on
            Node charNode = MAContRef.GetNodeByWorldPosition(charTrans.position);
            // If that character is an ally
            if (charNode.Occupying == CharacterType.Ally)
            {
                // Path to that ally
                MAContRef.Pathing(StandingNode, charNode, CharacterType.Enemy, false);
                Vector2Int printPos = new Vector2Int(int.MaxValue, int.MaxValue);
                if (closestAllyNode != null)
                    printPos = closestAllyNode.Position;
                Debug.Log(charNode.Position + " with " + charNode.F + " is being compared with " + printPos + " with " + closestAllyF);
                // If it is closer than the currently closest ally
                if (closestAllyF > charNode.F)
                {
                    int charFVal = charNode.F;
                    Debug.Log("It was closer in distance");

                    // 2) Get the closest node this enemy could attack that ally from
                    // Get the potential nodes to attack from
                    List<Node> potAttackNodes = MAContRef.GetNodesDistFromNode(charNode, MARef.AttackRange);
                    // Figure out which is closest to the standing positions
                    Debug.Log("The attack nodes are at ");
                    foreach (Node curAdjNode in potAttackNodes)
                    {
                        Debug.Log(curAdjNode.Position);
                        // If the node is not occupied by someone other than this enemy
                        MoveAttack curAdjEnemyMARef = MAContRef.GetCharacterMAByNode(curAdjNode);
                        if (curAdjNode.Occupying == CharacterType.None || curAdjEnemyMARef == MARef)
                        {
                            // Path there, if succesful keep testing
                            // We say we don't care because we have already checked if someone was there
                            // And if there is someone there, it would be this enemy
                            if (MAContRef.Pathing(StandingNode, curAdjNode, CharacterType.Enemy, false))
                            {
                                // If it is closer than the last one
                                if (closestAttackF > curAdjNode.F)
                                {
                                    Debug.Log("It was closer and could be attacked");
                                    // It is the new closest enemy node
                                    closestAllyNode = charNode;
                                    closestAllyF = charFVal;

                                    // It is the new closest attack node
                                    closestAttackNode = curAdjNode;
                                    closestAttackF = curAdjNode.F;
                                }
                            }
                        }
                    }
                }
            }
        }
        // If there is no closestAlly, return the current Node
        if (closestAllyNode == null)
        {
            //Debug.Log("There was no closest Ally");
            return StandingNode;
        }
        //Debug.Log("The closest ally is at " + closestAllyNode.Position);

        

        //Debug.Log("Closest attack node is at " + closestAttackNode.Position);
        // 3) Create a path to the closest attack node
        // Path to that node
        MAContRef.Pathing(StandingNode, closestAttackNode, CharacterType.Enemy);
        List<Node> currentPath = new List<Node>();
        // Follow the path to build it
        Node curPNode = StandingNode;
        int infiniteProtect = 1000;
        int curInfIncr = 0;
        while (curPNode.WhereToGo != curPNode)
        {
            currentPath.Add(curPNode);
            curPNode = curPNode.WhereToGo;
            if (++curInfIncr > infiniteProtect)
            {
                Debug.Log("Had to break from an infinite");
                break;
            }
        }
        // Add the last node
        currentPath.Add(curPNode);
        //string printPath = "Path = ";
        //foreach (Node curNode in currentPath)
        //    printPath += curNode.Position + " -> ";
        //Debug.Log(printPath);

        // 4) Get the node this ally would stop at along the created path
        Node endUpNode = null;
        int endUpIndex = 0;
        bool shouldRepeatStep4 = false;
        // We will only repeat step 4 in the case that one of the probed tiles is not occupied
        do
        {
            // Assume we should not repeat step 4
            shouldRepeatStep4 = false;

            // If we can move farther than the path
            if (MARef.MoveRange >= currentPath.Count)
                endUpIndex = currentPath.Count - 1;
            else
                endUpIndex = MARef.MoveRange;

            endUpNode = currentPath[endUpIndex];
            // If it is occupied, we have some backtracking to do
            while (endUpNode.Occupying != CharacterType.None)
            {
                // Backtrack 1 tile along the path
                // Make sure we can, if we can't backtrack any more, just return the standing node
                if (endUpIndex - 1 <= 0)
                    return StandingNode;
                Node backTrackNode = currentPath[endUpIndex - 1];

                // Probe the 4 adjacent tiles, minus the 2 tiles on the path already
                List<Node> probeNodes = MAContRef.GetNodesDistFromNode(backTrackNode, 1);
                for (int i = 0; i < probeNodes.Count; ++i)
                {
                    if (currentPath.Contains(probeNodes[i]))
                    {
                        probeNodes.RemoveAt(i);
                        --i;
                    }
                }

                // 3 Cases for the remaining 2 tiles: Both are occupied. 1 is not occupied. Neither are occupied
                // Make sure we only have 2
                if (probeNodes.Count != 2)
                {
                    Debug.Log("WARNING - BUG DETECTED - Probe Nodes does not contain 2 nodes");
                }
                // Case 1: Both are occupied
                else if (probeNodes[0].Occupying != CharacterType.None && probeNodes[1].Occupying != CharacterType.None)
                {
                    // If the backtrack node is free, that is the node to move to
                    if (backTrackNode.Occupying == CharacterType.None)
                    {
                        endUpNode = backTrackNode;
                        break;
                    }
                    // If it's occupied, we need to backtrack again, so just decrement the endUpIndex
                    // and of course, don't break
                    else
                    {
                        --endUpIndex;
                    }
                }
                // Case 2: at least 1 is unoccupied
                else if (probeNodes[0].Occupying == CharacterType.None || probeNodes[1].Occupying == CharacterType.None)
                {
                    // Assumed the first probe is unoccupied and then try to prove it wrong
                    Node unoccupiedNode = probeNodes[0];
                    if (probeNodes[0].Occupying != CharacterType.None)
                        unoccupiedNode = probeNodes[1];

                    // Case 2a: Both are unoccupied
                    if (probeNodes[0].Occupying == CharacterType.None && probeNodes[1].Occupying == CharacterType.None)
                    {
                        // Find the closer node to the attackNode
                        Node closerProbeNode = probeNodes[0];
                        int closerProbeF = int.MaxValue;
                        // Iterate to find the closest
                        foreach (Node singleProbe in probeNodes)
                        {
                            // Path to get Fs
                            if (MAContRef.Pathing(singleProbe, closestAttackNode, CharacterType.Enemy))
                            {
                                // If the current probe is closer
                                if (closerProbeF > closestAttackNode.F)
                                {
                                    closerProbeNode = singleProbe;
                                    closerProbeF = closestAttackNode.F;
                                }
                            }
                        }
                        unoccupiedNode = closerProbeNode;
                    }

                    // Get the index of the backtrack node
                    int backTrackIndex = currentPath.IndexOf(backTrackNode);
                    // Remove all nodes after this index from the path
                    while (backTrackIndex + 1 != currentPath.Count)
                    {
                        currentPath.RemoveAt(currentPath.Count - 1);
                    }
                    // Path from the probed node to the attack ndoe
                    MAContRef.Pathing(unoccupiedNode, closestAttackNode, CharacterType.Enemy);
                    // Add the nodes from the pathing to the path current path list
                    Node curPathNode = unoccupiedNode;
                    int infiniteLoopProtection = 1999;
                    int curInfInc = 0;
                    while (curPathNode.WhereToGo != curPathNode)
                    {
                        currentPath.Add(curPathNode);
                        curPathNode = curPathNode.WhereToGo;

                        if (++curInfInc > infiniteLoopProtection)
                        {
                            Debug.Log("Had to break from an infinite");
                            break;
                        }
                    }
                    // Add the last node
                    currentPath.Add(curPathNode);
                    // Now we need to re-get the tile this ally would stop at along their path, which is redoing 4)
                    shouldRepeatStep4 = true;
                    // Break out of the current loop so we can repeat step 4
                    break;
                }
                else
                {
                    Debug.Log("WARNING - BUG DETECTED - Unexpected Else was fallen into");
                }
            }
            // If the tile is not occupied we're done
            // We should only repeat this in the case that we had to update the path
        } while (shouldRepeatStep4);

        //Debug.Log("End Node is at " + endUpNode.Position);
        return endUpNode;
    }

    /// <summary>
    /// Attempts to do whatever action this enemy does.
    /// Called after the character finishes moving.
    /// Uses the character's skill on an enemy in range.
    /// </summary>
    override protected void AttemptAction()
    {
        MARef.EndAttack();
    }
}