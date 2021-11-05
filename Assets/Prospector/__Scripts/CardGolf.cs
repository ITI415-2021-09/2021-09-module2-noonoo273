﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardGolf : MonoBehaviour
{

	public string suit;
	public int rank;
	public Color color = Color.black;
	public string colS = "Black";  // or "Red"

	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();

	public GameObject back;  // back of card;
	public CardDefinition def;  // from DeckXML.xml		

	public SpriteRenderer[] spriteRenderers;
	void Start()
	{

		SetSortOrder(0);
	}
	public bool faceUp
	{
		get
		{
			return (!back.activeSelf);
		}

		set
		{
			back.SetActive(!value);
		}
	}

	public void PopulateSpriteRenderers()
	{
		if (spriteRenderers == null || spriteRenderers.Length == 0)
		{
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}

	public void SetSortingLayerName(string tSLN)
	{
		PopulateSpriteRenderers();

		foreach (SpriteRenderer tSR in spriteRenderers)
		{
			tSR.sortingLayerName = tSLN;
		}
	}

	public void SetSortOrder(int sOrd)
	{
		PopulateSpriteRenderers();

		foreach (SpriteRenderer tSR in spriteRenderers)
		{
			if (tSR.gameObject == this.gameObject)
			{
				tSR.sortingOrder = sOrd;
				continue;
			}

			switch (tSR.gameObject.name)
			{
				case "back":
					tSR.sortingOrder = sOrd + 2;
					break;


				case "face":
				default:
					tSR.sortingOrder = sOrd + 1;
					break;
			}
		}
	}

	virtual public void OnMouseUpAsButton()
	{
		print(name);
	}

}// class Card