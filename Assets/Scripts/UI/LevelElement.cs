using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelElement : MonoBehaviour
{
	public Image selectedImage;
	public TextMeshProUGUI songName;
	public Button levelSelectButton;
	public Image lockedImage;
	public GameObject ratingPanel;
	public Image[] ratingImages;

	public Sprite filledStar;
}
