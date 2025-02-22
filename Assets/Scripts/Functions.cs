using System;
using BasicTypes;
using UnityEngine;
using UnityEngine.UI;

public static class Functions
{
	private static int hueAdjust = 180;

	private static int satAndLigAdjust = 100;

	public static void RoundToPixelPerfect(ref Vector3 wantedPosition)
	{
	}

	public static void MyErrorDebug(string value)
	{
		Debug.LogError(value);
	}

	public static void MyDebug(string value)
	{
		Debug.Log(value);
	}

	public static void MyDebug(int value)
	{
		Debug.Log(value);
	}

	public static void MyDebug(float value)
	{
		Debug.Log(value);
	}

	public static void DebugTransformCount(Transform parentTransform)
	{
		if (parentTransform == null)
		{
			Debug.Log("Scene Transform count: " + UnityEngine.Object.FindObjectsOfType<Transform>().Length);
		}
		else
		{
			Debug.Log(parentTransform.name + " Transform and childs count: " + CountChilds(parentTransform));
		}
	}

	public static int CountChilds(Transform parentTransform)
	{
		int num = 1;
		if (parentTransform.childCount > 0)
		{
			for (int i = 0; i < parentTransform.childCount; i++)
			{
				num += CountChilds(parentTransform.GetChild(i));
			}
		}
		return num;
	}

	
	public static Color HSLToColor(int hue, int saturation, int lightness)
	{
		return new Color((float)(hue + 180) / 360f, (float)(saturation + 100) / 200f, (float)(lightness + 100) / 200f, 1f);
	}

	

	public static float GetFloatingItemScaleMultiplier(Sprite sprite)
	{
		if (sprite != null)
		{
			float num = 0.06f;
			float num2 = 0.32f;
			num2 = ((!(sprite.bounds.size.x > sprite.bounds.size.y)) ? sprite.bounds.size.y : sprite.bounds.size.x);
			if (num2 < num)
			{
				num2 = num;
			}
			float t = (num2 - num) / (0.32f - num);
			return Mathf.Lerp(1f, 0.5f, t);
		}
		return 1f;
	}

	public static float GetDisplayBlockScaleMultiplier(Sprite sprite)
	{
		if (sprite != null)
		{
			float num = 0.06f;
			float num2 = 0.32f;
			num2 = ((!(sprite.bounds.size.x > sprite.bounds.size.y)) ? sprite.bounds.size.y : sprite.bounds.size.x);
			if (num2 < num)
			{
				num2 = num;
			}
			float t = (num2 - num) / (0.32f - num);
			return Mathf.Lerp(1f, 0.75f, t);
		}
		return 1f;
	}

	public static float GetGiftBoxPrizeScaleMultiplier(Sprite sprite)
	{
		if (sprite != null)
		{
			float num = 1f;
			float num2 = 0.375f;
			float num3 = 0.32f;
			float num4 = 1f;
			num3 = ((!(sprite.bounds.size.x > sprite.bounds.size.y)) ? sprite.bounds.size.y : sprite.bounds.size.x);
			if (num3 < 0.06f)
			{
				num3 = Mathf.Max(0.01f, num3);
				num4 = Mathf.Lerp(0f, 0.6f, num3 / 0.05f);
			}
			num = 0.32f / num3;
			return num * num2 * num4;
		}
		return 1f;
	}

	public static Vector3 CountSpriteUIOffset(Sprite sprite)
	{
		Vector3 zero = Vector3.zero;
		if (sprite != null)
		{
			int num = (int)((sprite.bounds.size.x + 0.005f) * 100f);
			int num2 = (int)((sprite.bounds.size.y + 0.005f) * 100f);
			num += num % 2;
			num2 -= num2 % 2;
			num /= 2;
			num2 /= 2;
			zero.x = (float)num / 100f;
			zero.y = (float)num2 / 100f;
			zero.x *= -1f;
			zero.y *= -1f;
		}
		return zero;
	}

	public static void ChangeLayersRecursively(GameObject gameObject, string name)
	{
		ChangeLayersRecursively(gameObject, LayerMask.NameToLayer(name));
	}

	public static void ChangeLayersRecursively(GameObject gameObject, int layer)
	{
		gameObject.layer = layer;
		foreach (Transform item in gameObject.transform)
		{
			ChangeLayersRecursively(item.gameObject, layer);
		}
	}

	
	
	

	public static uint HSLToUint(int hue, int saturation, int lightness)
	{
		return (uint)((hue + hueAdjust << 20) | (saturation + satAndLigAdjust << 10) | (lightness + satAndLigAdjust));
	}

	public static int[] UintToHSLArray(uint packetUint)
	{
		return new int[3]
		{
			(int)(packetUint >> 20) - hueAdjust,
			((int)(packetUint & 0xFFFFF) >> 10) - satAndLigAdjust,
			(int)(packetUint & 0x3FF) - satAndLigAdjust
		};
	}

	public static bool KeyCodeFromInt(ref int keyCodeInt)
	{
		if (Enum.IsDefined(typeof(KeyCode), keyCodeInt))
		{
			return true;
		}
		return false;
	}

	

	public static void LoadKeyCodeFromSave(ref KeyCode saveToKeyCode, KeyCode defaultKeyCode, string keyCodeSaveKey)
	{
		int keyCodeInt = PlayerPrefs.GetInt(keyCodeSaveKey, (int)defaultKeyCode);
		if (KeyCodeFromInt(ref keyCodeInt))
		{
			saveToKeyCode = (KeyCode)keyCodeInt;
		}
		else
		{
			saveToKeyCode = defaultKeyCode;
		}
	}

	public static void SaveKeyCodeToSave(KeyCode keyCode, string keyCodeSaveKey)
	{
		PlayerPrefs.SetInt(keyCodeSaveKey, (int)keyCode);
		PlayerPrefs.Save();
	}

	public static bool GetKeyDown(ref KeyCode keyCode)
	{
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(value))
			{
				keyCode = value;
				return true;
			}
		}
		return false;
	}
}
