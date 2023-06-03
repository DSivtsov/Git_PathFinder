
------------------------------//------------------------------------------
Exist two types of Solutions:
	SolutionForDot : ISolution
	SolutionForEdgeForStartPoint : ISolution

Each Solution have:
	private readonly SectorSolutions _sectorSolutions; (or SectorSolutions[] in case "SolutionForEdge")
	private readonly int _numLastCrossedEdge;
	private readonly int _numRecBaseDot;
	private readonly ConnectionDot _connectionDot; (or ConnectionDot[] in case "SolutionForEdge")

Each SectorSolutions have: (its s sector of possible solutions limited by two line, both of which start from the point baseDotSectorSolutions)
	public readonly Line LineB;
	public readonly Line LineA;
	public readonly Vector2 baseDotSectorSolutions;
	
Each Line have (factorX * X +  factorY * Y = factorB):
	protected readonly float _factorX;
	protected readonly float _factorY;
	protected readonly float _factorB;

but most comonly used the NormolizedLine where FactorY = 1f; (factorX * X +  1 * Y = factorB)

Exist special Type of Line:
	
	LineVertical : Line
        private const float FactorXVerticalLine = 1f;
        private const float FactorYVerticalLine = 0;
	
	LineHorizontal : Line
        private const float FactorXVerticalLine = 0;
        private const float FactorYVerticalLine = 1f;

Each Solution have a ConnectionDot (or ConnectionDot[]) Store the connection between different Solutions (connection between "baseDotSectorSolutions")
	public readonly Vector2 baseDot;
	public readonly IEnumerable<ConnectionDot> prevConnectionDots;	//can connected to more than one of other ConnectionDot
Note.
For simplify the ConnectionDot.baseDot separated from SectorSolutions.baseDotSectorSolutions (but in most cases it is one point, but not in all cases)

ListDotsPath
        private static List<ConnectionDot> _list;			//Contains all dost which was found,
															//It's like Graph (discrete mathematics) which begin from _startPointFindPath and finish in point which have direct conection with _endPointFindPath
        private static int _numDotHaveCrossingwithEndPath;	//number dost which have direct conection with _endPointFindPath
        private static Vector2 _endPointFindPath;
        private static List<Vector2> _path;					//the founded path result of work
		
		static IEnumerable<Vector2> GetPath()				//methods which use _list to create the _path
															//curently found one path with any special requriments
Note!!!
the Graph use minimal possible count the dots (direct lines) to connect _startPointFindPath with _endPointFindPath
Therefore any path created by GetPath() will contain the minimal count of turns on this way (difference can exist only in distance of the Path and size of turns).
Use the _list and any Graph optimization can be found a more optimal way

------------------------------//------------------------------------------

Initilization of static class StoreInfoEdges.InitStoreInfoEdges()
- by information from initial Edges, fill internal storages:
	private static EdgeInfo[] _edgesInfo;
	private static Edge[] _arrEdges;
	private static Rectangle[] _arrRectangle;

Initilization of static class ListDotsPath.InitListDotsPath()

Create SolutionForDot for:
	_startPointFindPath
	_endPointFindPath

------------------------------//------------------------------------------

Begin Cycle:
- Any method trying to find path which can connect current solution for StartPoint with solution for endPoint
- All methods return IEnumerable<Vector2> if they found path (in other case null)

Order of methods is important:

	path = TryLinkCurrentBaseDotSolutionStartWithEndPoint();
	if (path != null) return path;

	path = IsBothSolutionOnOneEdge();
	if (path != null) return path;

	path = TryCrossingCurrentSolutionWithSolutionForEndPoint();
	if (path != null) return path;

	path = TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint();
	if (path != null) return path;
		
- If all methods not found the path (return null) the next iteration will be move the "Solution for StartPoint" to the endPoint more close 
_currentSolutionForStartPoint = SolutionForEdgeForStartPoint.CreateNewSolutionForEdge(_currentSolutionForStartPoint, _arredges.Length - 1);

In case of error in found the Path (in case of correct initial Data it's not possible) in method SolutionForEdgeForStartPoint.CreateNewSolutionForEdge, exist check:
if (numEdgeCurrentSolution == farthestNumEdge)		//We arrived the last edge but didn't find the Solution
	throw new NotSupportedException("Something wrong, because in this case the Finder.IsBothSolutionOnOneEdge() should have been called before");

It means that the intial Data contain the problem which we not detected at checking it