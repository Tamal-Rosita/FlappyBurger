using UnityEngine;

namespace DoomFire
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DoomFireRenderer : MonoBehaviour
    {
        private static readonly int PaletteTexId = Shader.PropertyToID("_PaletteTex");

        [Header("Simulation")]
        [Range(0.01f, 0.5f)]
        [SerializeField] private float updateFrequency = 0.075f;

        [Min(8)]
        [SerializeField] private int gridHeight = 16;

        [Min(8)]
        [SerializeField] private int gridWidth = 192;

        [SerializeField] private bool autoAspectWidth = true;

        [Range(16, 512)]
        [SerializeField] private int maxGridWidth = 256;

        [Range(0.2f, 1.5f)]
        [SerializeField] private float flameHeightRatio = 0.75f;

        [SerializeField] private bool useWind = true;

        [Header("Bounds & Palette")]
        [SerializeField] private bool matchBoxColliderSize = true;
        [SerializeField] private Vector2 fallbackWorldSize = new Vector2(12f, 1f);
        [SerializeField] private Gradient gradient;

        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private Texture2D _fireTexture;
        private Texture2D _paletteTexture;
        private Sprite _fireSprite;
        private byte[] _fireBuffer;
        private Color32[] _rgbaFallbackBuffer;
        private bool _useR8Format;

        private int _simWidth;
        private int _simHeight;
        private float _timer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _propertyBlock = new MaterialPropertyBlock();
            EnsureDefaultGradient();
        }

        private void Start()
        {
            InitializeFire();
        }

        private void Update()
        {
            if (Time.timeScale == 0f || _fireBuffer == null)
                return;

            _timer += Time.deltaTime;
            if (_timer < updateFrequency)
                return;

            _timer %= updateFrequency;
            StepSimulation();
            UploadFireTexture();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _paletteTexture != null)
            {
                BakePaletteTexture();
            }
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void InitializeFire()
        {
            ReleaseResources();

            var targetSize = fallbackWorldSize;
            var pivot = new Vector2(0.5f, 0f);

            if (matchBoxColliderSize && TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                targetSize = new Vector2(
                    Mathf.Max(0.01f, boxCollider.size.x),
                    Mathf.Max(0.01f, boxCollider.size.y)
                );
                var pivotY = 0.5f - (boxCollider.offset.y / targetSize.y);
                var pivotX = 0.5f - (boxCollider.offset.x / targetSize.x);
                pivot = new Vector2(pivotX, pivotY);
            }

            _simHeight = Mathf.Max(8, gridHeight);
            _simWidth = autoAspectWidth
                ? Mathf.Clamp(Mathf.RoundToInt(_simHeight * (targetSize.x / targetSize.y)), 8, maxGridWidth)
                : Mathf.Max(8, gridWidth);

            _fireBuffer = new byte[_simWidth * _simHeight];
            for (var x = 0; x < _simWidth; x++)
            {
                _fireBuffer[x] = 255;
            }

            _useR8Format = SystemInfo.SupportsTextureFormat(TextureFormat.R8);
            var texFormat = _useR8Format ? TextureFormat.R8 : TextureFormat.RGBA32;
            if (!_useR8Format)
            {
                _rgbaFallbackBuffer = new Color32[_simWidth * _simHeight];
            }

            _fireTexture = new Texture2D(_simWidth, _simHeight, texFormat, mipChain: false, linear: true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "DoomFire_Intensity"
            };

            _paletteTexture = new Texture2D(256, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "DoomFire_PaletteLUT"
            };
            BakePaletteTexture();

            var pixelsPerUnit = _simHeight / targetSize.y;
            _fireSprite = Sprite.Create(
                _fireTexture,
                new Rect(0f, 0f, _simWidth, _simHeight),
                pivot,
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );
            _fireSprite.name = "DoomFire_Sprite";

            _spriteRenderer.drawMode = SpriteDrawMode.Simple;
            _spriteRenderer.sprite = _fireSprite;
            _spriteRenderer.enabled = true;

            // Pre-warm the simulation so flames are visible on frame 0.
            for (var i = 0; i < _simHeight; i++)
            {
                StepSimulation();
            }
            UploadFireTexture();
        }

        private void StepSimulation()
        {
            var maxDecay = Mathf.Max(1, Mathf.RoundToInt((255f / (_simHeight * flameHeightRatio)) * 2f));

            for (var y = _simHeight - 1; y >= 1; y--)
            {
                var rowOffset = y * _simWidth;
                var belowRowOffset = (y - 1) * _simWidth;

                for (var x = 0; x < _simWidth; x++)
                {
                    var decay = Random.Range(0, maxDecay + 1);
                    var srcX = x;
                    if (useWind)
                    {
                        srcX += Random.Range(-1, 2);
                        if (srcX < 0) srcX = 0;
                        else if (srcX >= _simWidth) srcX = _simWidth - 1;
                    }

                    var decayed = _fireBuffer[srcX + belowRowOffset] - decay;
                    _fireBuffer[x + rowOffset] = decayed > 0 ? (byte)decayed : (byte)0;
                }
            }
        }

        private void UploadFireTexture()
        {
            if (_useR8Format)
            {
                var raw = _fireTexture.GetRawTextureData<byte>();
                raw.CopyFrom(_fireBuffer);
            }
            else
            {
                for (var i = 0; i < _fireBuffer.Length; i++)
                {
                    var v = _fireBuffer[i];
                    _rgbaFallbackBuffer[i] = new Color32(v, 0, 0, 255);
                }
                _fireTexture.SetPixels32(_rgbaFallbackBuffer);
            }

            _fireTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        }

        private void BakePaletteTexture()
        {
            EnsureDefaultGradient();
            var colors = new Color[256];
            for (var i = 0; i < 256; i++)
            {
                colors[i] = gradient.Evaluate(i / 255f);
            }

            _paletteTexture.SetPixels(colors);
            _paletteTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            _spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(PaletteTexId, _paletteTexture);
            _spriteRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void EnsureDefaultGradient()
        {
            if (gradient != null && gradient.colorKeys.Length > 0)
                return;

            gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.black, 0f),
                    new GradientColorKey(new Color(1f, 0.456f, 0f), 0.703f),
                    new GradientColorKey(new Color(1f, 0.681f, 0f), 0.859f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0.588f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }

        private void ReleaseResources()
        {
            if (_fireSprite != null)
            {
                Destroy(_fireSprite);
                _fireSprite = null;
            }
            if (_fireTexture != null)
            {
                Destroy(_fireTexture);
                _fireTexture = null;
            }

            if (_paletteTexture == null) return;
            Destroy(_paletteTexture);
            _paletteTexture = null;
        }
    }
}
