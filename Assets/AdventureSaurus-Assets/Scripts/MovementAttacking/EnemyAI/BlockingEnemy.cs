using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingEnemy : SingleEnemy
{
    /// <summary>
    /// Finds and returns the tile this enemy should move to.
    /// Specified in the overrides.
    /// </summary>
    /// <returns>Node that this enemy should move to</returns>
    protected override Node FindTileToMoveTo()
    {
        // 1) Find the closest ally
        Node closestAllyNode = null;
        int closestAllyF = int.MaxValue;
        // Iterate over each ally to find the closest
        foreach (Transform charTrans in GetAllyParent())
        {
            // Get the node that character is on
            Node charNode = MAContRef.GetNodeByWorldPosition(charTrans.position);
            // Path to that ally
            MAContRef.Pathing(StandingNode, charNode, CharacterType.Enemy, false);

            // If it is closer than the currently closest ally
            if (closestAllyF > charNode.F)
            {
                closestAllyNode = charNode;
                closestAllyF = charNode.F;
            }
        }
        // If there is no closest ally node, we return the node the enemy is currently on
        if (closestAllyNode == null)
        {
            //Debug.Log("There was no closest Ally");
            return StandingNode;
        }

        // 2) Find the path from that ally to the exit
        // Find the exit
        GameObject stairsObj = GameObject.Find(ProceduralGenerationController.STAIRS_NAME);
        Node stairsNode = MAContRef.GetNodeByWorldPosition(stairsObj.transform.position);
        // Initialize the list of nodes in the ally's path
        // This path is the one the ally would take if they did not care about enemies in their path
        List<Node> allyStraightPathToExit = new List<Node>();
        // Path from that ally to the exit
        // We say we don't care in the case that there is an enemy covering the exit
        if (MAContRef.Pathing(closestAllyNode, stairsNode, CharacterType.Enemy, false))
        {
            // Store this path
            Node curNode = closestAllyNode;
            while (curNode.WhereToGo != curNode)
            {
                curNode = curNode.WhereToGo;
                allyStraightPathToExit.Add(curNode);
            }
        }
        // Iterate over this straight path to get the actual path the ally would have to take
        List<Node> allyActualPathToExit = new List<Node>();
        for (int i = 0; i < allyStraightPathToExit.Count; ++i)
        {
            Node curNode = allyStraightPathToExit[i];
            // If the ally could actually go here, add the node to the allyActualPath
            if (curNode.Occupying == CharacterType.None ||
               curNode.Occupying == CharacterType.Ally)
                allyActualPathToExit.Add(curNode);
            // If the ally could not actually go here, find the next unoccupied node and
            // try to path from the previous node to the next unoccupied ndoe
            else
            {
                // Get the previous node
                Node prevNode = null;
                if (allyActualPathToExit.Count > 0)
                    prevNode = allyActualPathToExit[allyActualPathToExit.Count - 1];
                else
                    prevNode = closestAllyNode;
                // Find next unoccupied node
                // If there are no more unoccupied nodes, then we just have the unoccupied node be the last node
                Node unoccNode = allyStraightPathToExit[allyStraightPathToExit.Count - 1];
                for (int k = i + 1; k < allyStraightPathToExit.Count; ++k)
                {
                    Node curTestNode = allyStraightPathToExit[k];
                    // If we found the unoccupied node
                    if (curTestNode.Occupying == CharacterType.None ||
                        curTestNode.Occupying == CharacterType.Ally)
                    {
                        unoccNode = curTestNode;
                        break;
                    }
                }

                // Path from the previous node to the unoccupied node (we don't care if we can
                // actually make it there in case there was no unoccupied node)
                if (MAContRef.Pathing(prevNode, unoccNode, CharacterType.Ally, false))
                {
                    // Add this path to the actual path (we don't add the first or last node)
                    Node subPathNode = prevNode.WhereToGo;
                    while (subPathNode.WhereToGo != subPathNode)
                    {
                        allyActualPathToExit.Add(subPathNode);
                        subPathNode = subPathNode.WhereToGo;
                    }
                }
                // If the pathing fails, we have made the full path the enemy can take right now
                else
                {
                    break;
                }
            }
        }

        // 3) Find the closest node in this path to the enemy
        // We start by pathing to the first one and iterating. If the distance to a node
        // ever increases, that node is farther away, and the rest will be too, so we stop there
        Node targetNode = null;
        // The settle node is the node we will get close to if we can't reach any of the nodes in the path
        Node settleNode = StandingNode;
        int settleF = int.MaxValue;
        Debug.Log("Actual Ally Path: ");
        foreach (Node curNode in allyActualPathToExit)
        {
            Debug.Log(curNode.Position);
            // If we are already there or could actually move to there right now, we found our node
            if (curNode == StandingNode || MARef.MoveTiles.Contains(curNode))
            {
                targetNode = curNode;
                break;
            }
            // If we cannot instantly reach this tile, see how far it is away from this enemy
            // If we succesfully pathed there, compare the distance
            if (MAContRef.Pathing(StandingNode, curNode, CharacterType.Enemy))
            {
                // If the current node's F is smaller than the settleF, this node is closer
                if (settleF > curNode.F)
                {
                    settleNode = curNode;
                    settleF = curNode.F;
                }
                // If it is farther away, all other ones will be farther away too, so dont keep testing
                else
                    break;
            }
        }

        // If we found a target node we can move to immediately, return it
        if (targetNode != null)
            return targetNode;
        // If we did not find a target node, find node closest to the settle node that we can reach
        else
        {
            Node closestNode = StandingNode;
            int closestF = int.MaxValue;
            // Iterate over the possible tiles to move to
            foreach (Node moveTile in MARef.MoveTiles)
            {
                // Path from that move tile to the settle node
                // If successful, check if its closer than the current closestNode
                if (MAContRef.Pathing(moveTile, settleNode, CharacterType.Enemy))
                {
                    // It its closer, its the new closestNode
                    if (closestF > settleNode.F)
                    {
                        closestNode = moveTile;
                        closestF = settleNode.F;
                    }
                }
            }

            return closestNode;
        }
    }

    /// <summary>
    /// Attempts to do whatever action this enemy does.
    /// Called after the character finishes moving.
    /// If the enemy is within range of an ally, it will push them.
    /// </summary>
    protected override void AttemptAction()
    {
        Node currentNode = MAContRef.GetNodeByWorldPosition(this.transform.position);
        // Get the nodes it can push
        List<Node> adjNodes = MAContRef.GetNodesDistFromNode(currentNode, 1);
        // Try to find an ally to push at any of these nodes
        Node nodeToAttack = null;
        foreach (Node curNode in adjNodes)
        {
            // If we found a node containing an ally
            if (curNode.Occupying == CharacterType.Ally)
            {
                nodeToAttack = curNode;
                break;
            }
        }

        // If we found an ally to push, push it
        if (nodeToAttack != null)
        {
            MARef.StartAttack(nodeToAttack.Position);
        }
        // Otherwise, just end the attack
        else
        {
            MARef.EndAttack();
        }
    }
}
