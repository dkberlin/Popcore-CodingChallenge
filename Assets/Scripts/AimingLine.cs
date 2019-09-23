using System.Collections;
using System.Collections.Generic;
using BubblePop;
using UnityEngine;

public class AimingLine : MonoBehaviour
{
    
    [SerializeField]
    private Transform _shootingPosition;
    [SerializeField]
    private GameObject _shootingLinePrefab;
    [SerializeField]
    private Transform _linePivot;
    
    private List<GameObject> _lineDots = new List<GameObject>(); 
    
    void Update()
    {
        ActivateAimingLine();
    }

    private void ActivateAimingLine()
    {
        // list of colliders
        Ray2D ray2D = new Ray2D(_shootingPosition.position, _shootingPosition.up);
        List<Vector2> wallsHit = new List<Vector2>();
        List<Vector3> localWallsHit = new List<Vector3>();
        
        wallsHit.Add(ray2D.origin);
        wallsHit.AddRange(GetWallsHit(ray2D));
        
        wallsHit.ForEach(delegate(Vector2 vector2)
        {
            localWallsHit.Add(_linePivot.InverseTransformPoint(vector2));
        });
        
        SetupAimingLine(localWallsHit.Count);
        
        for (int i = 0; i < localWallsHit.Count -1; i++)
        {
            DrawAimingLine(_lineDots[i], localWallsHit[i], localWallsHit[i+1]);
        }
    }

    private void DrawAimingLine(GameObject lineDot, Vector3 start, Vector3 end)
    {
        lineDot.transform.localPosition = (start + end) / 2;
        lineDot.transform.up = start - end;
        lineDot.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(33, (end - start).magnitude);
        lineDot.SetActive(true);
    }

    private void SetupAimingLine(int count)
    {
        int toAdd = count - _lineDots.Count;
        
        for (int i = 0; i < toAdd; i++)
        {
            GameObject go = GameObject.Instantiate(_shootingLinePrefab);
            go.transform.parent = _linePivot;
            go.transform.localScale = Vector3.one;
            _lineDots.Add(go);
        }

        foreach (var dot in _lineDots)
        {
            dot.SetActive(false);
        }
    }

    private IEnumerable<Vector2> GetWallsHit(Ray2D ray2D)
    {
        List<Vector2> list = new List<Vector2>();
        RaycastHit2D wallHit = Physics2D.Raycast(ray2D.origin, ray2D.direction, 1080, 1 << LayerMask.NameToLayer(GameConsts.CollisionWallLayer));
        RaycastHit2D bubbleHit = Physics2D.Raycast(ray2D.origin, ray2D.direction, 1080, 1 << LayerMask.NameToLayer(GameConsts.BubbleLayer));
        
        if (bubbleHit.collider != null)
        {
            list.Add(bubbleHit.point);
            return list;
        }
        
        if (wallHit.collider != null)
        {
            Vector2 mirrorPoint = FindMirrorPoint(ray2D.origin, wallHit.point);
            Vector2 direction = mirrorPoint - wallHit.point;
            
            list.Add(wallHit.point);
            list.AddRange(GetWallsHit(new Ray2D(wallHit.point + direction.normalized, direction)));
        }
        
        return list;
    }

    private Vector2 FindMirrorPoint(Vector2 ray2DOrigin, Vector2 wallHitPoint)
    {
        Vector2 point = new Vector2(ray2DOrigin.x, wallHitPoint.y);
        return point * 2 - ray2DOrigin;
    }
}
