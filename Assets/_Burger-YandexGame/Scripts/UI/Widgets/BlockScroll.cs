using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockScroll : ScrollRect
{
    public override void OnInitializePotentialDrag(PointerEventData e)
    {
        if(e.button == PointerEventData.InputButton.Left) return;
        base.OnInitializePotentialDrag(e);
    }

    public override void OnBeginDrag(PointerEventData e)
    {
        if(e.button == PointerEventData.InputButton.Left) return;
        base.OnBeginDrag(e);
    }

    public override void OnDrag(PointerEventData e)
    {
        if(e.button == PointerEventData.InputButton.Left) return;
        base.OnDrag(e);
    }

    public override void OnEndDrag(PointerEventData e)
    {
        if(e.button == PointerEventData.InputButton.Left) return;
        base.OnEndDrag(e);
    }

#pragma warning disable CS0114 
    private void OnRectTransformDimensionsChange()
#pragma warning restore CS0114 
    {
        if(gameObject.activeInHierarchy)
        {
            Vector2 savedPosition = normalizedPosition;
            StartCoroutine(RestoreScroll(savedPosition));
        }
    }

    private IEnumerator RestoreScroll(Vector2 savedPos)
    {
        yield return new WaitForEndOfFrame();
        normalizedPosition = savedPos;
    }

    public override void OnScroll(PointerEventData e)
    {
        return;
    }
}
