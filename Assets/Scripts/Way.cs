public class Way
{
    private Node _nodeDestiny;
    private float _cost;
    private float _aCUMulatedCost;
    public Node WayNodeDestiny { get => _nodeDestiny; set { _nodeDestiny = value;} }
    public float WayCost { get => _cost; set { _cost = value; } }
    public float TotalACUMulatedWayCost { get => _aCUMulatedCost; set { _aCUMulatedCost = value; } }
    public Way(Node node, float cost)
    {
        _nodeDestiny = node;
        _cost = cost;
        _aCUMulatedCost = cost == 0 ? 0 : float.MaxValue;
    }
}
