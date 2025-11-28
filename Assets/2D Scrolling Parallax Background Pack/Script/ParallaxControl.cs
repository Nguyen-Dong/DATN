using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BGStudio.ParallaxControl 
{
	public class ParallaxControl : MonoBehaviour
	{
		public SpriteRenderer[] parallaxSprites;
		public float[] parallaxSpeed;
		// Start is called before the first frame update
		void Start()
		{
			
		}

		// Update is called once per frame
		void Update()
		{
			for (int i = 0; i < parallaxSprites.Length; i++)
			{
				Material parallaxSpritesMat= parallaxSprites[i].material;
				Vector2 newPostion = new Vector2(parallaxSpritesMat.mainTextureOffset.x, parallaxSpritesMat.mainTextureOffset.y);
				newPostion.x += parallaxSpeed[i]* Time.deltaTime/200f;
				parallaxSpritesMat.mainTextureOffset = newPostion;
			}
		}
	}
}