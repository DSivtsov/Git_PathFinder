using UnityEngine;

namespace GameEngine.Environment
{
    /// <summary>
    /// Create Field BackGround
    /// </summary>
    public class FieldBackGround : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.53f, 0.72f, 0.6f);

        private void Awake()
        {
            GenerationSettingSO _fieldSetting = FindObjectOfType<GeneratePathFinderData>().FieldSetting;
            CreateBackgroundField(_fieldSetting.CenterX, _fieldSetting.CenterY, _fieldSetting.WidthField, _fieldSetting.HeightField);
        }

        private void CreateBackgroundField(int posX, int posY, int _widthField, int _heightField)
        {
            SpriteRenderer _spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = CreateSpriteBackgroundField();
            _spriteRenderer.transform.localScale = new Vector3(_widthField, _heightField, 0);
            _spriteRenderer.transform.position = new Vector3(posX, posY, _spriteRenderer.transform.position.z);
        }

        private Sprite CreateSpriteBackgroundField()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, _color);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();
            Rect rect = new Rect(0, 0, 1, 1);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "BackgroundField";
            return sprite;
        }
    }
}
