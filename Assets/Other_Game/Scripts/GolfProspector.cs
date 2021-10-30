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

public class GolfProspector : Card
{
    [Header("Set Dynamically: Card Prospector")]

    public eCardState state = eCardState.drawpile;
    public List<GolfProspector> hiddenBy = new List<GolfProspector>();
    public int layoutID;
    public SlotDef slotDef;

    override public void OnMouseUpAsButton()
    {
        GolfSolitaire.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }

}
