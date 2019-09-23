using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace BubblePop
{
    public class PopBubbleGrid : MonoBehaviour
    {

        [SerializeField] 
        private BubbleHandler _bubbleHandler;
        [SerializeField]
        private int _colums = 6;
        [SerializeField]
        private Bubble _gridBubblePrefab;
        [SerializeField]
        private int _lines = 1;
        [SerializeField]
        private int _rows = 8;
        [SerializeField]
        private BoxCollider2D _roofCollider;
        [SerializeField]
        private EdgeCollider2D _lowCollider;

        private List<PoolBubble> _bubblePool;
        private List<List<Bubble>> _bubbleGrid;
        internal float _tileSize = 0.7f;
        internal float bubbleGridOffsetX;
        internal float bubbleGridOffsetY;
        private List<Bubble> _currentNeighborChain;
        private int _rowToMoveGrid;
        private float _speed = 1f;
        private Vector3 _newPositionForGrid;
        private bool _gridShouldMove;

        private void Start()
        {
            _bubblePool = new List<PoolBubble>();

            var i = 0;
            var total = 500;

            while (i < total)
            {
                _bubblePool.Add(_bubbleHandler.GetRandomPoolBubble());
                i++;
            }

            MixBubblePool(_bubblePool);

            DrawBubbleGrid();
        }

        private void DrawBubbleGrid()
        {
            _bubbleGrid = new List<List<Bubble>>();
            var safetyRows = 5;

            bubbleGridOffsetX = _colums * _tileSize * 0.5f;
            bubbleGridOffsetY = _rows * _tileSize * 0.5f;

            bubbleGridOffsetX -= _tileSize * 0.5f;
            bubbleGridOffsetY -= _tileSize * 0.5f;

            for (var row = 0; row < _rows; row++)
            {
                var bubbleRow = new List<Bubble>();

                for (var col = 0; col < _colums; col++)
                {
                    var gridBubble = Instantiate(_gridBubblePrefab, gameObject.transform, true);
                    gridBubble.SetValue(_bubblePool[0].BubbleValue);
                    gridBubble.SetColor(_bubblePool[0].BubbleColor);

                    gridBubble.SetPositionInGrid(this, col, row);
                    _bubblePool.RemoveAt(0);

                    bubbleRow.Add(gridBubble);

                    if (_bubbleGrid.Count < _lines) gridBubble.gameObject.SetActive(false);
                    if (_bubbleGrid.Count > _rows - safetyRows) gridBubble.gameObject.SetActive(false);
                    _rowToMoveGrid = _rows - safetyRows;
                }

                _bubbleGrid.Add(bubbleRow);
            }

            var transformPosition = transform.position;
            transformPosition.y = _lines / (safetyRows/1.9f);
            
            transform.position = transformPosition;
        }

        private void MixBubblePool<T>(List<T> bubblePool)
        {
            var rnd = new Random();

            // using the fisher-yates-shuffle
            var count = bubblePool.Count;
            while (count > 1)
            {
                count--;
                var k = rnd.Next(count + 1);
                var value = bubblePool[k];
                bubblePool[k] = bubblePool[count];
                bubblePool[count] = value;
            }
        }

        public void AddBulletBubbleToGrid(Bubble collisionBubble, BulletBubble bullet)
        {
            var adjacentSpots = GetAdjacentSpots(collisionBubble);
            var emptySpots = adjacentSpots.emptySpots;

            // set bubble to grid
            var minDis = 1000f;
            Bubble b = null;

            foreach (var spot in emptySpots)
            {
                var d = Vector2.Distance(spot.transform.position, bullet.transform.position);

                if (!(d < minDis)) continue;

                minDis = d;
                b = spot;
                b.SetColor(bullet.BubbleColor);
                b.SetValue(bullet.BubbleValue);
            }

            bullet.gameObject.SetActive(false);
            b.gameObject.SetActive(true);

            //get chain of neighbors
            StartNeighborChain(b);
            CheckIfGridNeedsToMove();
        }

        private void StartNeighborChain(Bubble b)
        {
            _currentNeighborChain = new List<Bubble>();
            GetNeighborChain(b, b.BubbleValue);

            if (_currentNeighborChain.Count == 0)
            {
                return;
            }

            var sortedNeighbors = _currentNeighborChain.OrderBy(n => n.transform.position.y).ToList();
            var highestNeighbor = sortedNeighbors.Last();


            MergeBubbles(highestNeighbor, sortedNeighbors);
        }

        private void MergeBubbles(Bubble highestNeighbor, List<Bubble> sortedNeighbors)
        {
            foreach (var neighbor in sortedNeighbors)
            {
                if (neighbor == highestNeighbor)
                {
                    continue;
                }
                    
                neighbor.gameObject.SetActive(false);
                neighbor.connected = false;
            }

            _bubbleHandler.SetNewValueForBubble(highestNeighbor, sortedNeighbors.Count);
            
            StartNeighborChain(highestNeighbor);
            
            RemoveDisconnectedBubbles();
            
            CheckIfGridNeedsToMove();
        }

        private void CheckIfGridNeedsToMove()
        {
            bool needsToMoveDown = true;
            bool needsToMoveUp = false;
            
            foreach (var bubble in _bubbleGrid[_rowToMoveGrid])
            {
                if (bubble.gameObject.activeSelf)
                {
                    needsToMoveDown = false;
                }
            }

            foreach (var bubble in _bubbleGrid[_rowToMoveGrid +2])
            {
                if (bubble.gameObject.activeSelf)
                {
                    needsToMoveUp = true;
                }
            }
            
            if (needsToMoveDown)
            {
                PrepareMoveCompleteGrid(true);
                _rowToMoveGrid--;
                EnableNewRow(_rowToMoveGrid - 4);
            }
            else if (needsToMoveUp)
            {
                PrepareMoveCompleteGrid(false);
                _rowToMoveGrid++;
            }
        }

        private void EnableNewRow(int rowToMoveGrid)
        {
            Debug.LogFormat("Enabling new row {0}...", rowToMoveGrid-1);
            foreach (var bubble in _bubbleGrid[rowToMoveGrid-1])
            {
                bubble.Enable();
            }
        }

        private void PrepareMoveCompleteGrid(bool moveDown)
        {
            _newPositionForGrid = transform.position;
            
            if (moveDown)
            {
                _newPositionForGrid.y -= _tileSize;
                _gridShouldMove = true;
            }
            else
            {
                _newPositionForGrid.y += _tileSize;
                _gridShouldMove = true;
            }
        }

        private void Update()
        {
            if (!_gridShouldMove)
            {
                return;
            }
            
            float step =  _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _newPositionForGrid, step);
                
            if (Vector3.Distance(transform.position, _newPositionForGrid) < 0.001f)
            {
                _gridShouldMove = false;
            }
        }

        private void GetNeighborChain(Bubble bubble, int valueToChain)
        {
            var adjacentBubbles = GetAdjacentSpots(bubble);
            var neighbors = adjacentBubbles.neighbors;

            foreach (var n in neighbors)
            {
                if (n.BubbleValue != bubble.BubbleValue || 
                    _currentNeighborChain.Contains(n))
                {
                    continue;
                }
                
                _currentNeighborChain.Add(n);
                GetNeighborChain(n, valueToChain);
            }
        }

        private void RemoveDisconnectedBubbles()
        {
            foreach (var bubble in _bubbleGrid.SelectMany(row => row))
            {
                bubble.connected = false;
            }

            foreach (var row in _bubbleGrid)
            {
                foreach (var bubble in row)
                {
                    if (!bubble.gameObject.activeSelf)
                    {
                        continue;
                    }
                    
                    if (bubble.gameObject.GetComponent<Collider2D>().IsTouching(_roofCollider))
                    {
                        bubble.connected = true;
                        continue;
                    }
                    
                    var ad = GetAdjacentSpots(bubble);
                    var neighbors = ad.neighbors;
                    
                    if (neighbors.Any(n => n.connected))
                    {
                        bubble.connected = true;
                        continue;
                    }
                    
                    bubble.TriggerDisconnectedEffect();
                }
            }
        }

//        private Bubble GetClosestMatchingNeighbor(Bubble collisionBubble)
//        {
//            var adjacentBubbles = GetAdjacentSpots(collisionBubble);
//            var neighbors = adjacentBubbles.neighbors;
//
//            var neighborDis = 1000f;
//            Bubble closestNeighbor = null;
//
//            if (neighbors.Any(n => n.BubbleValue == collisionBubble.BubbleValue) == false) return null;
//
//            foreach (var neighbor in neighbors.Where(n => n.BubbleValue == collisionBubble.BubbleValue))
//            {
//                var d = Vector2.Distance(neighbor.transform.position, collisionBubble.transform.position);
//
//                if (!(d < neighborDis)) continue;
//                neighborDis = d;
//                closestNeighbor = neighbor;
//            }
//
//            return closestNeighbor;
//        }

        private (List<Bubble> emptySpots, List<Bubble> neighbors) GetAdjacentSpots(Bubble bubble)
        {
            var emptySpots = new List<Bubble>();
            var neighbors = new List<Bubble>();

//            Debug.LogFormat("checking bubble at row: {0} and col {1} : {2}...", bubble.row, bubble.col,
//                bubble.BubbleValue);

            if (bubble.col + 1 < _colums)
            {
                var spotToCheck = _bubbleGrid[bubble.row][bubble.col + 1];
//                Debug.Log("checking right...");

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);

                    neighbors.Add(spotToCheck);
                }
            }

            //left
            if (bubble.col - 1 >= 0)
            {
                var spotToCheck = _bubbleGrid[bubble.row][bubble.col - 1];
//                Debug.Log("checking left...");

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);

                    neighbors.Add(spotToCheck);
                }
            }

            //top
            if (bubble.row - 1 >= 0)
            {
                var spotToCheck = _bubbleGrid[bubble.row - 1][bubble.col];
//                Debug.Log("checking top...");

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
                    neighbors.Add(spotToCheck);
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);
                }
            }

            //bottom
            if (bubble.row + 1 < _bubbleGrid.Count)
            {
                var spotToCheck = _bubbleGrid[bubble.row + 1][bubble.col];
//                Debug.Log("checking bottom...");

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
                    neighbors.Add(spotToCheck);
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);
                }
            }

            //top-left
            if (bubble.row - 1 >= 0 && bubble.col - 1 >= 0 && bubble.col % 2 != 0)
            {
//                Debug.Log("checking top left...");

                var spotToCheck = _bubbleGrid[bubble.row - 1][bubble.col - 1];

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
                    neighbors.Add(spotToCheck);
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);
                }
            }

            //top-right
            if (bubble.row - 1 >= 0 && bubble.col + 1 < _colums && bubble.col % 2 != 0)
            {
                var spotToCheck = _bubbleGrid[bubble.row - 1][bubble.col + 1];
//                Debug.Log("checking top right...");

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
                    neighbors.Add(spotToCheck);
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);
                }
            }
            
            //bottom-left
            if (bubble.row + 1 < _bubbleGrid.Count && bubble.col - 1 >= 0 && bubble.col % 2 == 0)
            {
                var spotToCheck = _bubbleGrid[bubble.row + 1][bubble.col - 1];
//                Debug.Log("checking bottom left...");

                if (!spotToCheck.gameObject.activeSelf)
                {
                    emptySpots.Add(spotToCheck);
//                    Debug.Log("...empty spot.");
                }
                else
                {
                    neighbors.Add(spotToCheck);
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);
                }
            }

            //bottom-right
            if (bubble.row + 1 < _bubbleGrid.Count && bubble.col + 1 < _colums && bubble.col % 2 == 0)
            {
                var spotToCheck = _bubbleGrid[bubble.row + 1][bubble.col + 1];
//                Debug.Log("checking bottom right...");

                if (!spotToCheck.gameObject.activeSelf)
                {
//                    Debug.Log("...empty spot.");
                    emptySpots.Add(spotToCheck);
                }
                else
                {
                    neighbors.Add(spotToCheck);
//                    Debug.Log("...found neighbor: " + spotToCheck.BubbleValue);
                }
            }

            return (emptySpots, neighbors);
        }
    }
}