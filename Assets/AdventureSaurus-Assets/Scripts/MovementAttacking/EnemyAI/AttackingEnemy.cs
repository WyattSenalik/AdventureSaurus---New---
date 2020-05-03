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
        // 1) Get closest ally node with an attackable node
        Node closestAllyNode = null;
        int closestAllyF = int.MaxValue;
        Node closestAttackNode = null;
        int closestAttackF = int.MaxValue;
        // Iterate over each ally to find the closest
        foreach (Transform charTrans in GetAllyParent())
        {
            // Get the node that character is on
            Node charNode = MAContRef.GetNodeByWorldPosition(charTrans.position);
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
        {
            MARef.EndAttack();
        }
        // If there is a node being attacked, start the attack
        else
            MARef.StartAttack(nodeToAttack);
    }
}