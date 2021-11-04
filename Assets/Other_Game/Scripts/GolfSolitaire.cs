using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class GolfSolitaire : MonoBehaviour
{

	static public GolfSolitaire S;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
	public TextAsset layoutXML;
	// public float xOffset = 3;
	// public float yOffset = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
	public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);



	[Header("Set Dynamically")]
	public Deck deck;
	public Layout layout;
	public List<GolfProspector> drawPile;
	public Transform layoutAnchor;
	public GolfProspector target;
	public List<GolfProspector> tableau;
	public List<GolfProspector> discardPile;
	public FloatingScore fsRun;

	void Awake()
	{
		S = this;
	}

	void Start()
	{
		ScoreBoard.S.score = ScoreManager.SCORE;

		deck = GetComponent<Deck>();
		deck.InitDeck(deckXML.text);
		Deck.Shuffle(ref deck.cards); // This shuffles the deck by reference

		Card c;
		for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        {
			c = deck.cards[cNum];
			c.transform.localPosition = new Vector3(cNum % 13, cNum / 13 * 4, 0);
        }

		layout = GetComponent<Layout>();
		layout.ReadLayout(layoutXML.text);

		drawPile = ConvertListCardsToListCardProspectors(deck.cards);
		LayoutGame();
	}
	List<GolfProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
	{
		List<GolfProspector> lCP = new List<GolfProspector>();
		GolfProspector tCP;
		foreach (Card tCD in lCD)
		{
			tCP = tCD as GolfProspector;
			lCP.Add(tCP);
		}
		return (lCP);
	}

	GolfProspector Draw()
	{
		GolfProspector cd = drawPile[0];
		drawPile.RemoveAt(0);
		return (cd);
	}

	void LayoutGame()
	{
		if (layoutAnchor == null)
		{
			GameObject tGO = new GameObject("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}

		GolfProspector cp;
		foreach (SlotDef tSD in layout.slotDefs)
		{
			cp = Draw();
			cp.faceUp = tSD.faceUp;
			cp.transform.parent = layoutAnchor;
			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layerID);

			cp.layoutID = tSD.id;
			cp.slotDef = tSD;

			cp.state = eCardState.tableau;

			cp.SetSortingLayerName(tSD.layerName);

			tableau.Add(cp);
		}

		foreach (GolfProspector tCP in tableau)
		{
			foreach (int hid in tCP.slotDef.hiddenBy)
			{
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}
		}

		// Set up the initial target card
		MoveToTarget(Draw());

		// Set up the Draw pile
		UpdateDrawPile();
	}

	GolfProspector FindCardByLayoutID(int layoutID)
	{
		foreach (GolfProspector tCP in tableau)
		{
			if (tCP.layoutID == layoutID)
			{
				return (tCP);
			}
		}

		return (null);
	}

	void SetTableauFaces()
	{
		foreach (GolfProspector cd in tableau)
		{
			bool faceUp = true;
			foreach (GolfProspector cover in cd.hiddenBy)
			{
				if (cover.state == eCardState.tableau)
				{
					faceUp = false;
				}
			}

			cd.faceUp = faceUp;
		}
	}

	void MoveToDiscard(GolfProspector cd)
	{
		cd.state = eCardState.discard;
		discardPile.Add(cd);
		cd.transform.parent = layoutAnchor; // update its transform parent

		// position this card on the discardPile
		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);

		cd.faceUp = true;
		// place it on top of the pile for depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100 + discardPile.Count);
	}

	void MoveToTarget(GolfProspector cd)
	{
		// if there is a currently a target card, move it to discardPile
		if (target != null) MoveToDiscard(target);
		target = cd; // cd is the new target
		cd.state = eCardState.target;
		cd.transform.parent = layoutAnchor;
		// Move to the target position
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
		cd.faceUp = true; // make it face up
						  // set the depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(0);
	}

	void UpdateDrawPile()
	{
		GolfProspector cd;

		for (int i = 0; i < drawPile.Count; i++)
		{
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;

			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
				layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
				-layout.drawPile.layerID + 0.1f * i);

			cd.faceUp = false;
			cd.state = eCardState.drawpile;
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10 * i);
		}
	}

	public void CardClicked(GolfProspector cd)
	{
		switch (cd.state)
		{
			case eCardState.target:
				// Clicking the card does nothing
				break;

			case eCardState.drawpile:
				MoveToDiscard(target); // moves target to discard pile
				MoveToTarget(Draw()); // moves the next drawn card to target
				UpdateDrawPile();
				
				ScoreManager.EVENT(eScoreEvent.draw);
				FloatingScoreHandler(eScoreEvent.draw);
				break;

			case eCardState.tableau:
				bool validMatch = true;
				if (!cd.faceUp)
				{
					// if card is face down, not valid
					validMatch = false;
				}
				if (!AdjacentRank(cd, target))
				{
					validMatch = false;
				}
				if (!validMatch) { return; }

				tableau.Remove(cd);// remove from tableau list
				MoveToTarget(cd); // make it target card
				SetTableauFaces(); // update tableau card face-ups
				
				ScoreManager.EVENT(eScoreEvent.mine);
				FloatingScoreHandler(eScoreEvent.mine);
				break;
		}

		CheckForGameOver();
	}

	void CheckForGameOver()
	{
		if (tableau.Count == 0)
		{
			GameOver(true);
			return;
		}
		if (drawPile.Count > 0)
		{
			return;
		}
		foreach (GolfProspector cd in tableau)
		{
			if (AdjacentRank(cd, target))
			{
				return;
			}
		}
		GameOver(false);
	}

	void GameOver(bool won)
	{
		if (won)
		{
			//print("Game Over. You won! :)");
			ScoreManager.EVENT(eScoreEvent.gameWin);
			FloatingScoreHandler(eScoreEvent.gameWin);
		}
		else
		{
			//print("Game Over. You Lost! :(");
			ScoreManager.EVENT(eScoreEvent.gameLoss);
			FloatingScoreHandler(eScoreEvent.gameLoss);
		}

		SceneManager.LoadScene("_Other_Game_Scene_0");
	}

	public bool AdjacentRank(GolfProspector c0, GolfProspector c1)
	{
		// if either card is face down, it's not adjacent
		if (!c0.faceUp || !c1.faceUp)
		{
			return (false);
		}

		// if they are 1 apart they are adjacent
		if (Mathf.Abs(c0.rank - c1.rank) == 1)
		{
			return (true);
		}

		// if one is Ace and other is King, they are adjacent
		if (c0.rank == 1 && c1.rank == 13)
		{
			return (true);
		}
		if (c0.rank == 13 && c1.rank == 1)
		{
			return (true);
		}

		return (false);
	}

	void FloatingScoreHandler(eScoreEvent evt)
	{
		List<Vector2> fsPts;
		switch (evt)
		{
			case eScoreEvent.draw:
			case eScoreEvent.gameWin:
			case eScoreEvent.gameLoss:
				if (fsRun != null)
				{
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);
					fsRun.reportFinishTo = ScoreBoard.S.gameObject;
					fsRun.Init(fsPts, 0, 1);
					fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
					fsRun = null;
				}
				break;

			case eScoreEvent.mine:
				FloatingScore fs;
				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;
				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosRun);
				fsPts.Add(fsPosMid);
				fs = ScoreBoard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);

				fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
				if (fsRun == null)
				{
					fsRun = fs;
					// fs.reportFinishTo = null;
				}
				else
				{
					fs.reportFinishTo = fsRun.gameObject;
				}
				break;
		}
	}
}
