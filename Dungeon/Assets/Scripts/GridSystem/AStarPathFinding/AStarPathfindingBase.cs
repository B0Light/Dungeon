using System.Collections.Generic;
using System.Linq;

public interface IPathNode
{
    float GCost { get; set; }
    float HCost { get; set; }
    float FCost { get; }
    IPathNode Parent { get; set; }
}

public abstract class AStarPathfindingBase<TNode>
    where TNode : IPathNode
{
    protected abstract IEnumerable<TNode> GetNeighbors(TNode node);
    protected abstract float GetDistance(TNode a, TNode b);

    protected List<TNode> FindPath(TNode startNode, TNode goalNode)
    {
        List<TNode> openList = new List<TNode>();
        HashSet<TNode> closedList = new HashSet<TNode>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            TNode currentNode = openList.OrderBy(node => node.FCost).First();

            if (currentNode.Equals(goalNode))
            {
                return RetracePath(startNode, goalNode);
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (TNode neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                float tentativeGCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetDistance(neighbor, goalNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null; // 경로를 찾지 못한 경우
    }

    protected List<TNode> RetracePath(TNode startNode, TNode goalNode)
    {
        List<TNode> path = new List<TNode>();
        TNode currentNode = goalNode;

        while (!currentNode.Equals(startNode))
        {
            path.Add(currentNode);
            currentNode = (TNode)currentNode.Parent;
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }
}
