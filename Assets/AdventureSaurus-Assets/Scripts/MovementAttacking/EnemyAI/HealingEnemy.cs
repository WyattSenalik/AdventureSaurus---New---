using System.Collections.Generic;
using UnityEngine;

public class HealingEnemy : SingleEnemy
{
    /// <summary>
    /// Finds and returns the tile this enemy should move to.
    /// Find the closest tile they can heal an enemy from
    /// </summary>
    /// <returns>Node that this enemy should move to</returns>
    override protected Node FindTileToMoveTo()
    {
        //Debug.Log("------------------------------------------");
        //Debug.Log("Start Node " + StandingNode.Position);
        // 1) Get closest enemy node that can be healed
        Node closestEnemyNode = null;
        int closestEnemyF = int.MaxValue;
        Node closestHealNode = null;
        int closestHealF = int.MaxValue;
        // Iterate over each enemy to find the closest
        foreach (Transform charTrans in GetEnemyParent())
        {
            // Only if the enemy is active and not this character
            if (charTrans.gameObject.activeInHierarchy && charTrans != this.transform)
            {
                // We only want to try moving there is the enemy is damaged
                Health charHealth = charTrans.GetComponent<Health>();
                if (charHealth.CurHP < charHealth.MaxHP) {
                    Node charNode = MAContRef.GetNodeByWorldPosition(charTrans.position);
                    // Path to that enemy
                    MAContRef.Pathing(StandingNode, charNode, CharacterType.Enemy, false);

                    // FOR TESTING
                    //Vector2Int printPos = new Vector2Int(int.MaxValue, int.MaxValue);
                    //if (closestAllyNode != null)
                    //    printPos = closestAllyNode.Position;
                    //Debug.Log(charNode.Position + " with " + charNode.F + " is being compared with " + printPos + " with " + closestAllyF);

                    // If it is closer than the currently closest enemy
                    if (closestEnemyF > charNode.F)
                    {
                        int charFVal = charNode.F;
                        //Debug.Log("It was closer in distance");

                        // 2) Get the closest node this enemy could heal that enemy from
                        // We know that is enemy is closer in walking distance, but we don't know if they have any openings to heal
                        // So we test to see if we can find any openings to heal
                        // Get the potential nodes to heal from
                        List<Node> potHealNodes = MAContRef.GetNodesDistFromNode(charNode, MARef.AttackRange);
                        // Figure out which is closest to the standing positions
                        //Debug.Log("The attack nodes are at ");
                        foreach (Node curAdjNode in potHealNodes)
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
                                    if (closestHealF > curAdjNode.F)
                                    {
                                        //Debug.Log("It was closer and could be attacked");
                                        // It is the new closest enemy node
                                        closestEnemyNode = charNode;
                                        closestEnemyF = charFVal;

                                        // It is the new closest heal node
                                        closestHealNode = curAdjNode;
                                        closestHealF = curAdjNode.F;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // If there is no closest heal from node, make the enemy run away
        if (closestHealNode == null)
        {
            // Get the allies nodes before (we only want the allies who are close enough
            // that they could attack us on their next turn)
            List<Node> allyNodes = new List<Node>();
            foreach (Transform allyTrans in GetAllyParent())
            {
                Node curAllyNode = MAContRef.GetNodeByWorldPosition(allyTrans.position);
                // Calculate the distance between the ally's node and this enemy's node
                Vector2Int distVect = curAllyNode.Position - StandingNode.Position;
                int dist = Mathf.Abs(distVect.x + distVect.y);
                // Only add the ally if the enemy is within their attack range
                MoveAttack curAllyMA = MAContRef.GetCharacterMAByNode(curAllyNode);
                curAllyMA.ResetMyTurn();
                //Debug.Log("Dist from " + curAllyMA.name + ": " + dist + ". " + curAllyNode.Position + "->" + StandingNode.Position);
                if (dist <= curAllyMA.AttackRange + curAllyMA.MoveRange)
                {
                    //Debug.Log(curAllyMA.name + " is close enough to " + this.name);
                    allyNodes.Add(curAllyNode);
                }
            }
            // Set a reference to the node that is the farthest from all allies
            Node furthestNode = StandingNode;
            int furthestF = 0;
            // Get the move tiles of this enemy plus the node they are standing on
            List<Node> testNodes = new List<Node>(MARef.MoveTiles);
            testNodes.Add(StandingNode);
            // Iterate over this enemy's move tiles
            foreach (Node moveTile in testNodes)
            {
                int curAddedF = 0;
                // Path from each ally to the current tile
                foreach (Node curAllyNode in allyNodes)
                {
                    MAContRef.Pathing(curAllyNode, moveTile, CharacterType.Enemy, false);
                    curAddedF += moveTile.F;
                }
                // If the distance from each ally is greater than the last distance, it is the new furthest
                if (curAddedF > furthestF)
                {
                    furthestNode = moveTile;
                    furthestF = curAddedF;
                }
            }

            // Return the furthest node
            return furthestNode;
        }
        //Debug.Log("The closest ally is at " + closestAllyNode.Position);


        // Short cut. If the closest attack node is one of the moveTiles, just return it
        // Iterate over the possible tiles to move to
        if (MARef.MoveTiles != null)
        {
            foreach (Node moveTile in MARef.MoveTiles)
            {
                // If its a part of the movetiles
                if (moveTile == closestHealNode)
                {
                    return closestHealNode;
                }
            }
        }
        else
        {
            Debug.LogError("MoveTiles is null");
        }


        // If the shortcut didn't work do it the long way
        Node closestNode = StandingNode;
        int closestF = int.MaxValue;
        if (MARef.MoveTiles != null)
        {
            // Iterate over the possible tiles to move to
            foreach (Node moveTile in MARef.MoveTiles)
            {
                // Path from that move tile to the closest attack node
                // If successful, check if its closer than the closestNode
                if (MAContRef.Pathing(moveTile, closestHealNode, CharacterType.Enemy))
                {
                    // It its closer, its the new closestNode
                    if (closestF > closestHealNode.F)
                    {
                        closestNode = moveTile;
                        closestF = closestHealNode.F;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("MoveTiles is null");
        }
        // Its the endtile
        return closestNode;
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
        if (MARef.AttackTiles != null)
        {
            foreach (Node atkNode in MARef.AttackTiles)
            {
                //Debug.Log(atkNode.Position);
                MoveAttack potAlly = MAContRef.GetCharacterMAByNode(atkNode);
                // If we found a damaged enemy at that tile
                if (potAlly != null && potAlly.WhatAmI == CharacterType.Enemy)
                {
                    Health enHealth = potAlly.GetComponent<Health>();
                    if (enHealth.CurHP < enHealth.MaxHP)
                    {
                        //Debug.Log("Attacking ^ That tile");
                        nodeToAttack = atkNode.Position;
                        break;
                    }
                }
            }
        }
        else
            Debug.LogError("AttackTiles are null");
        // If there is no node to attack, just end the attack
        if (nodeToAttack.x == int.MaxValue && nodeToAttack.y == int.MaxValue)
        {
            MARef.EndAttack();
        }
        // If there is a node being attacked, start the attack
        else
            MARef.StartAttack(nodeToAttack);
    }
}
