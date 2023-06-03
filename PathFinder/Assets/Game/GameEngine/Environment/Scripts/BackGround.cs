using UnityEngine;

namespace GameEngine.Environment
{
    public class BackGround : MonoBehaviour
    {
        [SerializeField] private FieldSettingSO _fieldSetting;

        private void Awake()
        {
            CreateBackground(_fieldSetting.CenterX, _fieldSetting.CenterY, _fieldSetting.WidthField, _fieldSetting.HeightField);
        }

        private void CreateBackground(int posX, int posY, int _widthField, int _heightField)
        {
            //GameObject _gameObjectBackGround = new GameObject("BackGround");
            //_gameObjectBackGround.transform.position = new Vector3(posX, posY, 0);
            //SpriteRenderer _spriteRenderer = _gameObjectBackGround.AddComponent<SpriteRenderer>();
            SpriteRenderer _spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
            Texture2D texture = new Texture2D(1, 1);
            Color color = new Color(0.53f, 0.72f, 0.6f);
            texture.SetPixel(0, 0, color);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();
            Rect rect = new Rect(0, 0, 1, 1);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "MySprite";
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.transform.localScale = new Vector3(_widthField, _heightField, 0);
        }
    }
}
