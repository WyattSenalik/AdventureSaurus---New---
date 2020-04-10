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
        //Debug.Log("------------------------------------------");
        //Debug.Log("Start Node " + StandingNode.Position);
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

                // FOR TESTING
                //Vector2Int printPos = new Vector2Int(int.MaxValue, int.MaxValue);
                //if (closestAllyNode != null)
                //    printPos = closestAllyNode.Position;
                //Debug.Log(charNode.Position + " with " + charNode.F + " is being compared with " + printPos + " with " + closestAllyF);

                // If it is closer than the currently closest ally
                if (closestAllyF > charNode.F)
                {
                    int charFVal = charNode.F;
                    //Debug.Log("It was closer in distance");

                    // 2) Get the closest node this enemy could attack that ally from
                    // We know that is enemy is closer in walking distance, but we don't know if they have any openings to attack
                    // So we test to see if we can find any openings to attack
                    // Get the potential nodes to attack from
                    List<Node> potAttackNodes = MAContRef.GetNodesDistFromNode(charNode, MARef.AttackRange);
                    // Figure out which is closest to the standing positions
                    //Debug.Log("The attack nodes are at ");
                    foreach (Node curAdjNode in potAttackNodes)
                    {
                        //Debug.Log(curAdjNode.Position);
                        // If the node is not occupied by someone other than this enemy
                        // since if this enemy is standing next to an ally, obvious that ally is the closest
                        MoveAttack curAdjEnemyMARef = MAContRef.GetCharacterMAByNode(curAdjNode);
                        if (curAdjNode.Occupying == CharacterType.None || curAdjEnemyMARef == MARef)
                        {
                            // Path there, if succesful, then see if it is closer than the current attackfrom node
                            // We say we don't care if we can get there (shouldCare=false) because we have already 
                            // checked if someone was there and if there is someone there, it would be this enemy
                            if (MAContRef.Pathing(StandingNode, curAdjNode, CharacterType.Enemy, false))
                            {
                                // If it is closer than the closest attack from node
                                if (closestAttackF > curAdjNode.F)
                                {
                                    //Debug.Log("It was closer and could be attacked");
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
        // If there is no closest attack from node, return the current Node
        if (closestAttackNode == null)
        {
            //Debug.Log("There was no closest Ally with an opening to attack");
            return StandingNode;
        }
        //Debug.Log("The closest ally is at " + closestAllyNode.Position);


        // This short find a tile to move to replaced the other way, which was faster, but much more complex

        // Short cut. If the closest attack node is one of the moveTiles, just return it
        // Iterate over the possible tiles to move to
        foreach (Node moveTile in MARef.MoveTiles)
        {
            // If its a part of the movetiles
            if (moveTile == closestAttackNode)
            {
                return closestAttackNode;
            }
        }


        // If the shortcut didn't work do it the long way
        Node closestNode = StandingNode;
        int closestF = int.MaxValue;
        // Iterate over the possible tiles to move to
        foreach (Node moveTile in MARef.MoveTiles)
        {
            // Path from that move tile to the closest attack node
            // If successful, check if its closer than the closestNode
            if (MAContRef.Pathing(moveTile, closestAttackNode, CharacterType.Enemy))
            {
                // It its closer, its the new closestNode
                if (closestF > closestAttackNode.F)
                {
                    closestNode = moveTile;
                    closestF = closestAttackNode.F;
                }
            }
        }

        // Its the endtile
        return closestNode;



        /* This way is faster, but less correct, and more complex
       
        //Debug.Log("Closest attack node is at " + closestAttackNode.Position);
        // 3) Create a path to the closest attack node
        // Path to the node
        MAContRef.Pathing(StandingNode, closestAttackNode, CharacterType.Enemy);
        // This list will hold the nodes in our path
        List<Node> currentPath = new List<Node>();
        // Follow the created path to build the list
        Node curPNode = StandingNode;
        while (curPNode.WhereToGo != curPNode)
        {
            currentPath.Add(curPNode);
            curPNode = curPNode.WhereToGo;
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

            // If we can move farther than the path, get the last node in the path
            if (MARef.MoveRange >= currentPath.Count)
                endUpIndex = currentPath.Count - 1;
            // Otherwise get how far we can move in the path
            else
                endUpIndex = MARef.MoveRange;

            // Get that node
            endUpNode = currentPath[endUpIndex];
            // If it is occupied, we have some backtracking to do
            while (endUpNode.Occupying != CharacterType.None)
            {
                // Backtrack 1 tile along the path
                // Make sure we can, if we can't backtrack any more, just return the standing node,
                // since we can't move otherwise
                if (endUpIndex - 1 < 0)
                    return StandingNode;
                // Get the backtrack node by getting the node 1 before the backtrack node
                Node backTrackNode = currentPath[endUpIndex - 1];

                // Probe the 4 adjacent tiles to that node, minus the 2 tiles on the path already
                List<Node> probeNodes = MAContRef.GetNodesDistFromNode(backTrackNode, 1);
                for (int i = 0; i < probeNodes.Count; ++i)
                {
                    if (currentPath.Contains(probeNodes[i]))
                    {
                        probeNodes.RemoveAt(i);
                        --i;
                    }
                }

                // 3 Cases for the remaining tiles: all are occupied; at least 1 is not occupied; multiple are not occupied

                // Case 1: All are occupied
                // Assume they are all occupied
                bool allOcc = true;
                // Try to prove the assumption wrong
                foreach (Node singleProbe in probeNodes)
                {
                    if (singleProbe.Occupying == CharacterType.None)
                    {
                        allOcc = false;
                        break;
                    }
                }
                // If they are all occupied
                if (allOcc)
                {
                    // If the backtrack node is free, that is the node to move to,
                    // so set that as the end node and break
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
                // Create a list of the free nodex
                List<Node> freeNodes = new List<Node>();
                // Add all the free nodes to that list
                foreach (Node singleProbe in probeNodes)
                {
                    if (singleProbe.Occupying == CharacterType.None)
                    {
                        freeNodes.Add(singleProbe);
                    }
                }
                // If at least one is free
                if (freeNodes.Count > 0)
                {
                    Node unoccupiedNode = freeNodes[0];

                    // Case 2a: Multiple are free, we need to chose one
                    if (freeNodes.Count > 1)
                    {
                        // Find the closer node to the attackNode
                        Node closerProbeNode = freeNodes[0];
                        int closerProbeF = int.MaxValue;
                        // Iterate to find the closest
                        foreach (Node singleProbe in probeNodes)
                        {
                            // Path from the probe to the end node to get Fs
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
                        // The node we want to add to the path is the closer probe
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
                    while (curPathNode.WhereToGo != curPathNode)
                    {
                        currentPath.Add(curPathNode);
                        curPathNode = curPathNode.WhereToGo;
                    }
                    // Add the last node
                    currentPath.Add(curPathNode);

                    // Now we need to re-get the tile this ally would stop at along their path, which is redoing 4)
                    shouldRepeatStep4 = true;
                    // Break out of the current loop so we can repeat step 4
                    break;
                }
                // Just a sanity test
                if ((!allOcc && freeNodes.Count == 0 ) || (allOcc && freeNodes.Count != 0))
                {
                    Debug.Log("WARNING - BUG DETECTED - Unexpected If was fallen into");
                }
            }
            // If the tile is not occupied we're done
        } while (shouldRepeatStep4);
        // We should only repeat this in the case that we had to update the path.
        // So if the node we would end up at was occupied and the probed nodes were unoccupied.

        //Debug.Log("End Node is at " + endUpNode.Position);
        return endUpNode;
        */
    }

    /// <summary>
    /// Attempts to do whatever action this enemy does.
    /// Called after the character finishes moving.
    /// Uses the character's skill on an enemy in range.
    /// </summary>
    override protected void AttemptAction()
    {
        // Recalculate the enemy's move and attack tiles.
        // We really only want their attack tiles, but those are based on the move tiles
        // Which are currently out of place since they are what the 
        MARef.CalculateAllTiles();
        Vector2Int nodeToAttack = new Vector2Int(int.MaxValue, int.MaxValue);
        // Try to find an ally in range
        //Debug.Log("These are " + this.name + " at " + this.transform.position + " potential attack tiles: ");
        foreach (Node atkNode in MARef.AttackTiles)
        {
            //Debug.Log(atkNode.Position);
            MoveAttack potAlly = MAContRef.GetCharacterMAByNode(atkNode);
            // If we found an ally at that tile
            if (potAlly != null && potAlly.WhatAmI == CharacterType.Ally)
            {
                //Debug.Log("Attacking ^ That tile");
                nodeToAttack = atkNode.Position;
                break;
            }
        }
        // If there is no node to attack, just end the attack
        if (nodeToAttack.x == int.MaxValue && nodeToAttack.y == int.MaxValue)
            MARef.EndAttack();
        // If there is a node being attacked, start the attack
        else
            MARef.StartAttack(nodeToAttack);
    }
}