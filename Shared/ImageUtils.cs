using UnityEngine;

namespace PeakArchetypes.Shared;

public static class ImageUtils
{
	public static Sprite Base64ToSprite(string base64, float pixelsPerUnit = 100f)
	{
		byte[] imageData = System.Convert.FromBase64String(base64);
		Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
		tex.LoadImage(imageData);
		return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
	}
}
