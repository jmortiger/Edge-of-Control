using UnityEngine;

namespace Assets.Scripts
{
	public class DamageIndicator : MonoBehaviour
	{
		//private float SpriteWidth { get => SpriteRenderer.sprite.bounds.size.x; }
		//private float SpriteHeight { get => SpriteRenderer.sprite.bounds.size.y; }
		private float spriteWidth;
		private float spriteHeight;
		//private float worldScreenWidth;
		//private float worldScreenHeight;
		private float WorldScreenWidth	{ get => WorldScreenHeight * camera.aspect; }
		private float WorldScreenHeight	{ get => camera.orthographicSize * 2.0f; }
		private new Camera camera;
		private SpriteRenderer spriteRenderer;
		private SpriteRenderer SpriteRenderer
		{
			get
			{
				if (spriteRenderer == null)
					spriteRenderer = GetComponent<SpriteRenderer>();
				return spriteRenderer;
			}
		}
		void Reset()
		{
			camera = GetComponentInParent<Camera>();
			spriteWidth = SpriteRenderer.sprite.bounds.size.x;
			spriteHeight = SpriteRenderer.sprite.bounds.size.y;
			//worldScreenHeight = camera.orthographicSize * 2.0f;
			//worldScreenWidth = worldScreenHeight * camera.aspect;
		}
		void Awake()
		{
			camera = camera != null ? camera : GetComponentInParent<Camera>();
			spriteWidth = SpriteRenderer.sprite.bounds.size.x;
			spriteHeight = SpriteRenderer.sprite.bounds.size.y;
			//worldScreenHeight = camera.orthographicSize * 2.0f;
			//worldScreenWidth = worldScreenHeight * camera.aspect;
			//gameObject.SetActive(false);
			//SpriteRenderer.enabled = false;
			var c = SpriteRenderer.color;
			c.a = 0;
			SpriteRenderer.color = c;
			enabled = false;
		}
		private float timer = 0;
		private float timerLength = 1f;
		void Update()
		{
			if (timer > 0)
			{
				ScaleGraphic();
				var c = SpriteRenderer.color;
				c.a = Mathf.SmoothStep(0, 1, (timer * .995f) / timerLength);
				SpriteRenderer.color = c;
				timer -= Time.deltaTime;
				if (timer <= 0)
				{
					//gameObject.SetActive(false);
					//SpriteRenderer.enabled = false;
					c = SpriteRenderer.color;
					c.a = 0;
					SpriteRenderer.color = c;
					timer = 0;
					enabled = false;
				}
			}
		}
		//void OnDrawGizmosSelected()
		//{
		//	ScaleGraphic();
		//	//SpriteRenderer.enabled = true;
		//	var c = SpriteRenderer.color;
		//	c.a = 1;
		//	SpriteRenderer.color = c;
		//	enabled = true;
		//}
		//void OnDrawGizmos()
		//{
		//	//gameObject.SetActive(false);
		//	//SpriteRenderer.enabled = false;
		//	var c = SpriteRenderer.color;
		//	c.a = 0;
		//	SpriteRenderer.color = c;
		//	enabled = false;
		//}
		public void Show(float timerLength)
		{
			this.timerLength = timerLength;
			timer = timerLength;
			ScaleGraphic();
			//gameObject.SetActive(true);
			//SpriteRenderer.enabled = true;
			var c = SpriteRenderer.color;
			c.a = 1;
			SpriteRenderer.color = c;
			enabled = true;
		}
		public void ScaleGraphic()
		{
			//var worldScreenHeight = camera.orthographicSize * 2.0f;
			//var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
			transform.localScale = new(WorldScreenWidth / spriteWidth, WorldScreenHeight / spriteHeight, 1);
		}
	}
}