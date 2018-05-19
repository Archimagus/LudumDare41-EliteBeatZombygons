using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
	public Texture2D reticle;
	public CursorMode cursorMode = CursorMode.ForceSoftware;
	public Vector2 hotSpot = Vector2.zero;

	void Start()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			Cursor.SetCursor(reticle, hotSpot, CursorMode.ForceSoftware);
		else
			Cursor.SetCursor(reticle, hotSpot, CursorMode.Auto);
	}
}
