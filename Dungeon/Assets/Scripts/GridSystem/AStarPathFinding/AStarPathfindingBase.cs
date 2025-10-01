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
    
    // 실제 G 비용을 계산할 때 사용할 이동 비용 훅
    // 기본은 격자 간 거리 사용. 하위 클래스에서 셀 타입/지형 가중치를 반영하도록 재정의 가능
    protected virtual float GetMovementCost(TNode from, TNode to)
    {
        return GetDistance(from, to);
    }

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

                float tentativeGCost = currentNode.GCost + GetMovementCost(currentNode, neighbor);

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
