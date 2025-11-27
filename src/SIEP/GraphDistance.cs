
internal static class GraphDistance
{
    public static double Calculate(Graph graphA, Graph graphB)
    {
        return WlKernelPseudometric(graphA, graphB);    
    }

    private static double WlKernelPseudometric(Graph graphA, Graph graphB)
    {
        var (featureA, featureB) = MakeWlFeatureMaps(graphA, graphB);
        
        var kernelAtoA = featureA.CalculateKernel();
        var kernelBtoB = featureB.CalculateKernel();
        var kernelAtoB = featureA.CalculateKernel(featureB);
        
        // kernel to pseudometric formula
        return Math.Sqrt(kernelAtoA + kernelBtoB - 2 * kernelAtoB);
    }

    private static (WlFeatureMap, WlFeatureMap) MakeWlFeatureMaps(Graph graphA, Graph graphB, int iterations = 4)
    {
        var labelCap = GetLabelsCountCap(graphA, graphB, iterations);
        var hasher = new LabelHasher();
        var featureA = new WlFeatureMap(graphA, labelCap, iterations + 1, hasher);
        var featureB = new WlFeatureMap(graphB, labelCap, iterations + 1, hasher);

        for (var i = 0; i < iterations; i++)
        {
            WlSubtreeIteration(featureA, featureB, i);
        }
        
        return (featureA, featureB);
    }

    private static int GetLabelsCountCap(Graph graphA, Graph graphB, int iterations)
    {
        var max = Math.Max(graphA.VertexCount, graphB.VertexCount);
        const int graphCount = 2;
        
        // Bound from paper
        return max * graphCount * (iterations + 1);
    }

    private static void WlSubtreeIteration(WlFeatureMap featureA, WlFeatureMap featureB, int iter)
    {
        featureA.OneIter(iter);
        featureB.OneIter(iter);
    }
}

internal class WlFeatureMap
{
    private readonly int[,] _featureMap;
    
    private readonly LabelHasher _hasher;
    private readonly Graph _graph;
    private int[] _labelMap;
    private int[] _labelMapNext;
    
    public WlFeatureMap(Graph graph, int labelCap, int depth, LabelHasher hasher)
    {
        _featureMap = new int[depth, labelCap];
        _hasher = hasher;
        _graph = graph;
        
        _labelMap = new int[graph.VertexCount];
        _labelMapNext = new int[graph.VertexCount];
        
        InitLabelMap();
    }

    private void InitLabelMap()
    {
        for (var i = 0; i < _labelMap.Length; i++)
        {
            _labelMap[i] = _hasher.GetLabel(_graph.LoopCount(i));
            _featureMap[0, _labelMap[i]]++;
        }
    }

    public void OneIter(int iter)
    {
        var lvl = iter + 1;
        _labelMapNext = new int[_graph.VertexCount];

        for (var vert = 0; vert < _graph.VertexCount; vert++)
        {
            var head = _labelMap[vert];
            
            var outNeighbours = _graph
                .YieldOutNeighboursWithCount(vert)
                .Select(p => (_labelMap[p.Item1], p.Item2))
                .OrderBy(p => p.Item1)
                .ThenBy(p => p.Item2)
                .ToArray();
            
            var inNeighbours = _graph
                .YieldOutNeighboursWithCount(vert)
                .Select(p => (_labelMap[p.Item1], p.Item2))
                .OrderBy(p => p.Item1)
                .ThenBy(p => p.Item2)
                .ToArray();
            
            var label = _hasher.GetLabel(head, outNeighbours, inNeighbours);
            _featureMap[lvl, label]++;
            _labelMapNext[vert] = label;
        }
        
        _labelMap = _labelMapNext;
    }

    public int CalculateKernel()
    {
        return CalculateKernel(this);
    }

    public int CalculateKernel(WlFeatureMap other)
    {
        var kernel = 0;

        for (var row = 0; row < _featureMap.GetLength(0); row++)
        {
            // dot product in row
            for (var i = 0; i < _featureMap.GetLength(1); i++)
            {
                kernel += _featureMap[row, i] * other._featureMap[row, i];
            }
        }
        
        return kernel;
    }
}

internal class LabelHasher
{
    private readonly Dictionary<Signature, int> _labels = new();
    private int _currValue = -1;

    public int GetLabel(int vert, IReadOnlyList<(int, int)> outNeighbours, IReadOnlyList<(int, int)> inNeighbours)
    {
        var signature = new Signature(vert, outNeighbours, inNeighbours);
        return FindOrMakeLabel(signature);
    }

    public int GetLabel(int loops)
    {
        var signature = new Signature(loops);
        return FindOrMakeLabel(signature);
    }

    private int FindOrMakeLabel(Signature signature)
    {
        if (_labels.TryGetValue(signature, out var value))
        {
            return value;
        }
        
        value = ++_currValue;
        _labels.Add(signature, value);
        
        return value;
    }
}

internal readonly struct Signature: IEquatable<Signature>
{
    private readonly int _head;
    private readonly (int, int)[] _outNeighbors;
    private readonly (int, int)[] _inNeighbors;

    public Signature(int head, IReadOnlyList<(int, int)> outNeigh, IReadOnlyList<(int, int)> inNeigh)
    {
        _head = head;
        _outNeighbors = outNeigh.ToArray();
        _inNeighbors = inNeigh.ToArray();
    }

    public Signature(int head)
    {
        _head = head;
        _outNeighbors = [];
        _inNeighbors = [];
    }

    public bool Equals(Signature other)
    {
        if (_head != other._head)
            return false; 
        
        if (_outNeighbors.Length != other._outNeighbors.Length)
            return false;
        
        if (!_outNeighbors.SequenceEqual(other._outNeighbors)) 
            return false;
        
        if (_inNeighbors.Length != other._inNeighbors.Length)
            return false;
        
        if (!_inNeighbors.SequenceEqual(other._inNeighbors))
            return false;

        return true;
    }

    public override bool Equals(object? obj) => obj is Signature other && Equals(other);

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(_head);
        
        foreach (var (neigh, count) in _outNeighbors)
        {
            hash = HashCode.Combine(hash, neigh, count);
        }
        
        foreach (var (neigh, count) in _inNeighbors)
        {
            hash = HashCode.Combine(hash, neigh, count);
        }
        
        return hash;
    }
}
