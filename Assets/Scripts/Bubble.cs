using TMPro;
using UnityEngine;

namespace BubblePop
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text _valueLabel;
        [SerializeField] 
        private ParticleSystem _mergeParticleSystem;
        [SerializeField] 
        private GameObject _maxValueParticleSystemPrefab;
        [SerializeField] 
        private GameObject _disconnectedParticleSystemPrefab;
        private PopBubbleGrid _grid;
        [SerializeField]
        internal int col;
        [SerializeField]
        internal int row;
        private Vector3 _bubblePos;
        public bool connected;

        public Color BubbleColor { get; private set; }

        public int BubbleValue { get; private set; }

        public void SetValue(int value)
        {
            BubbleValue = value;
            _valueLabel.text = value.ToString();
        }

        public void SetColor(Color bubbleColor)
        {
            BubbleColor = bubbleColor;
            var renderer = transform.GetComponent<SpriteRenderer>();

            renderer.color = bubbleColor;
        }
        
        public void SetValuesFrom(Bubble secondNextBullet)
        {
            SetValue(secondNextBullet.BubbleValue);
            SetColor(secondNextBullet.BubbleColor);
        }

        public void SetPositionInGrid(PopBubbleGrid grid, int col, int row)
        {
            _grid = grid;
            this.col = col;
            this.row = row;
            
            _bubblePos = new Vector3((col * _grid._tileSize) - _grid.bubbleGridOffsetX, _grid.bubbleGridOffsetY + (-row * _grid._tileSize), 0);
            
            if (this.col % 2 == 0)
            {
                _bubblePos.y -= _grid._tileSize * 0.5f;
            }
            
            transform.localPosition = _bubblePos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag != "Bullet" || !other.gameObject.activeSelf)
            {
                return;
            }
            
            var bubbleGo = other.gameObject.GetComponent<BulletBubble>();
            _grid.AddBulletBubbleToGrid(this, bubbleGo);
        }
        
        public void TriggerParticleEffect(Color col)
        {
            var main = _mergeParticleSystem.main;
            main.startColor = col;
            
            _mergeParticleSystem.Play();
        }

        public void TriggerMaxValueEffect()
        {
            Instantiate(_maxValueParticleSystemPrefab, transform.position, transform.rotation);
            transform.gameObject.SetActive(false);
        }
        
        public void TriggerDisconnectedEffect()
        {
            var ps = Instantiate(_disconnectedParticleSystemPrefab, transform.position, transform.rotation);
            var mainModule = ps.GetComponent<ParticleSystem>().main;
            mainModule.startColor = BubbleColor;
            
            transform.gameObject.SetActive(false);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            connected = true;
        }
    }
}