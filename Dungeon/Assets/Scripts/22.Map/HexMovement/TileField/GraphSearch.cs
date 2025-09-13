using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphSearch
{
    public static BFSResult BFSGetRange(HexCoordinate startPos, int point)
    {
        Dictionary<HexCoordinate, HexCoordinate?> visitedNodes = new Dictionary<HexCoordinate, HexCoordinate?>();
        Dictionary<HexCoordinate, int> costSoFar = new Dictionary<HexCoordinate, int>();
        Queue<HexCoordinate> nodesToVisitQueue = new Queue<HexCoordinate>();

        nodesToVisitQueue.Enqueue(startPos);
        costSoFar.Add(startPos, 0);
        visitedNodes.Add(startPos, null);

        while (nodesToVisitQueue.Count > 0)
        {
            HexCoordinate currentNode = nodesToVisitQueue.Dequeue();
            foreach (HexCoordinate neighbourPosition in HexGrid.Instance.GetNeighboursFor(currentNode))
            {
                if (HexGrid.Instance.GetTileAt(neighbourPosition).IsObstacle())
                    continue;

                int nodeCost = HexGrid.Instance.GetTileAt(neighbourPosition).GetCost();
                int currentCost = costSoFar[currentNode];
                int newCost = currentCost + nodeCost;

                if (nodeCost > point) continue;

                if (!visitedNodes.ContainsKey(neighbourPosition))
                {
                    visitedNodes[neighbourPosition] = currentNode;
                    costSoFar[neighbourPosition] = newCost;
                    nodesToVisitQueue.Enqueue(neighbourPosition);
                }
                else if (costSoFar[neighbourPosition] > newCost)
                {
                    costSoFar[neighbourPosition] = newCost;
                    visitedNodes[neighbourPosition] = currentNode;
                }
            }
        }
        return new BFSResult(visitedNodes);
    }
    

    public static List<HexCoordinate> GeneratePathBFS(HexCoordinate current, Dictionary<HexCoordinate, HexCoordinate?> visitedNodesDict)
    {
        List<HexCoordinate> path = new List<HexCoordinate>();
        path.Add(current);
        while (visitedNodesDict[current] != null)
        {
            path.Add(visitedNodesDict[current].Value);
            current = visitedNodesDict[current].Value;
        }
        path.Reverse();
        return path.Skip(1).ToList();
    }
}

public struct BFSResult
{
    private Dictionary<HexCoordinate, HexCoordinate?> _visitedNodesDict;

    public BFSResult(Dictionary<HexCoordinate, HexCoordinate?> visitedNodesDict) {
        _visitedNodesDict = visitedNodesDict;
    } 

    public List<HexCoordinate> GetPathTo(HexCoordinate destination)
    {
        if (_visitedNodesDict.ContainsKey(destination) == false)
            return new List<HexCoordinate>();

        List<HexCoordinate> path = new List<HexCoordinate>();
        path.Add(destination);

        while (_visitedNodesDict[destination] != null)
        {
            path.Add(_visitedNodesDict[destination].Value);
            destination = _visitedNodesDict[destination].Value;
        }
        path.Reverse();

        return path.Skip(1).ToList();
    }

    public bool IsHexPositionInRange(HexCoordinate position)
    {
        return _visitedNodesDict.ContainsKey(position);
    }

    public IEnumerable<HexCoordinate> GetRangePositions()
        => _visitedNodesDict?.Keys;
}