using System.Collections.Generic;
using BubblePop;
using UnityEngine;

public class RaycastAiming : MonoBehaviour
{
    [SerializeField] 
    private GameObject _aimingDot;
    [SerializeField] 
    private float _dotsSpace = 0.3f;
    [SerializeField] 
    private Camera _mainCamera;
    [SerializeField] 
    private BulletBubble _bullet;
    [SerializeField] 
    private BulletBubble _nextBullet;
    [SerializeField] 
    private BulletBubble _secondNextBullet;

    private float _bulletBubbleIncrement;
    private float _bulletBubbleProgress;
    private List<Vector2> _dots;
    private List<GameObject> _dotsPool;
    private readonly int _maxDots = 30;
    private bool _mouseDown;

    public BubbleHandler _bubbleHandler;

    private void Start()
    {
        _dots = new List<Vector2>();
        _dotsPool = new List<GameObject>();

        var i = 0;
        var dotAlpha = 1f / _maxDots;
        var dotStartAlpha = 1f;

        // instantiate aiming dots
        while (i < _maxDots)
        {
            var d = Instantiate(_aimingDot);
            var sr = d.GetComponent<SpriteRenderer>();
            var col = sr.color;

            col.a = dotStartAlpha - dotAlpha;
            dotStartAlpha -= dotAlpha;
            sr.color = col;

            d.SetActive(false);
            _dotsPool.Add(d);
            i++;
        }

        _bubbleHandler.SetBulletValueAndColor(_nextBullet);
        _bubbleHandler.SetBulletValueAndColor(_secondNextBullet);
    }

    private void HandleTouchDown(Vector2 touch)
    {
    }

    private void HandleTouchUp(Vector2 touch)
    {
        if (_dots == null || _dots.Count < 2)
            return;

        foreach (var d in _dotsPool)
            d.SetActive(false);

        _bulletBubbleProgress = 0f;

        _bullet.SetValuesFrom(_nextBullet);


        _bullet.gameObject.SetActive(true);
        _bullet.transform.position = transform.position;

        InitalizeBulletPath();
        ReloadBullets();
    }

    private void InitalizeBulletPath()
    {
        var start = _dots[0];
        var end = _dots[1];
        var length = Vector2.Distance(start, end);
        var iterations = length / 0.15f;
        _bulletBubbleProgress = 0f;
        _bulletBubbleIncrement = 1f / iterations;
    }

    private void ReloadBullets()
    {
        _nextBullet.SetValuesFrom(_secondNextBullet);
        _bubbleHandler.SetBulletValueAndColor(_secondNextBullet);
    }

    private void HandleTouchMove(Vector2 touch)
    {
        var pos = transform.position;
        Vector2 point = _mainCamera.ScreenToWorldPoint(touch);
        var direction = new Vector2(point.x - pos.x, point.y - pos.y);
        var hit = Physics2D.Raycast(pos, direction);

        if (_dots == null)
        {
            return;
        }

        _dots.Clear();

        foreach (var d in _dotsPool)
        {
            d.SetActive(false);
        }

        if (hit.collider == null)
        {
            return;
        }

        _dots.Add(transform.position);

        if (hit.collider.tag == "SideWall")
        {
            DoRayCast(hit, direction);
        }
        else
        {
            _dots.Add(hit.point);
            DrawPaths();
        }
    }

    private void DoRayCast(RaycastHit2D previousHit, Vector2 directionIn)
    {
        var normal = Mathf.Atan2(previousHit.normal.y, previousHit.normal.x);
        var newDirection = normal + (normal - Mathf.Atan2(directionIn.y, directionIn.x));
        var reflection = new Vector2(-Mathf.Cos(newDirection), -Mathf.Sin(newDirection));
        var newCastPoint = previousHit.point + 2 * reflection;
        var hit2 = Physics2D.Raycast(newCastPoint, reflection);

        _dots.Add(previousHit.point);

        if (hit2.collider != null)
        {
            if (hit2.collider.tag == "SideWall")
            {
                DoRayCast(hit2, reflection);
            }
            else
            {
                _dots.Add(hit2.point);
                DrawPaths();
            }
        }
        else
        {
            DrawPaths();
        }
    }

    private void DrawPaths()
    {
        if (_dots.Count <= 1)
        {
            return;
        }

        foreach (var d in _dotsPool)
            d.SetActive(false);

        var index = 0;

        for (var i = 1; i < _dots.Count; i++)
        {
            DrawAimPath(i - 1, i, ref index);
        }
    }

    private void DrawAimPath(int start, int end, ref int index)
    {
        var length = Vector2.Distance(_dots[start], _dots[end]);

        var dotAmount = Mathf.RoundToInt(length / _dotsSpace);

        var dotProgress = 1f / dotAmount;

        var p = 0f;

        while (p < 1)
        {
            var px = _dots[start].x + p * (_dots[end].x - _dots[start].x);
            var py = _dots[start].y + p * (_dots[end].y - _dots[start].y);

            if (index < _maxDots)
            {
                var d = _dotsPool[index];
                d.transform.position = new Vector2(px, py);
                d.SetActive(true);
                index++;
            }

            p += dotProgress;
        }
    }

    private void Update()
    {
        if (_bullet.gameObject.activeSelf)
        {
            _bulletBubbleProgress += _bulletBubbleIncrement;

            if (_bulletBubbleProgress > 1)
            {
                _dots.RemoveAt(0);
                if (_dots.Count < 2)
                {
                    _bullet.gameObject.SetActive(false);
                    return;
                }

                InitalizeBulletPath();
            }

            var px = _dots[0].x + _bulletBubbleProgress * (_dots[1].x - _dots[0].x);
            var py = _dots[0].y + _bulletBubbleProgress * (_dots[1].y - _dots[0].y);

            _bullet.transform.position = new Vector2(px, py);

            return;
        }

        if (_dots == null)
            return;

        if (Input.touches.Length > 0)
        {
            var touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchDown(touch.position);
                    break;
                case TouchPhase.Ended:
                    HandleTouchUp(touch.position);
                    break;
                case TouchPhase.Stationary:
                    HandleTouchMove(touch.position);
                    break;
            }

            HandleTouchMove(touch.position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            _mouseDown = true;
            HandleTouchDown(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _mouseDown = false;
            HandleTouchUp(Input.mousePosition);
        }
        else if (_mouseDown)
        {
            HandleTouchMove(Input.mousePosition);
        }
    }
}