using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState
{
    drawpile,
    target,
    tableau,
    discard
}

public class CardProspector : Card
{
    [Header("Set Dynamically: Card Prospector")]

    public eCardState state = eCardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    public int layoutID;
    public SlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }

}
