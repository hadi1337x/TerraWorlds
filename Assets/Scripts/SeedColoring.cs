using UnityEngine;

public class SeedColoring : MonoBehaviour
{
	[Range(-180f, 180f)]
	public int hue;

	[Range(-100f, 100f)]
	public int saturation;

	[Range(-100f, 100f)]
	public int lightness;

	public SpriteRenderer[] sprite;

	private bool isInited;

	public void Update()
	{
		isInited = true;
		ChangeHSL();
	}

	public void SetColors(int hue, int saturation, int lightness)
	{
		this.hue = hue;
		this.saturation = saturation;
		this.lightness = lightness;
	}

	public void ChangeHSL()
	{
		if (isInited)
		{
			foreach (SpriteRenderer rend in sprite)
			{
                rend.color = Functions.HSLToColor(hue, saturation, lightness);
            }
		}
	}
}
