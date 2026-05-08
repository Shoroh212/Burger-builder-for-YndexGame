using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkinRoller : MonoBehaviour
{
    [SerializeField] private List<ShopCard> _cells;
    [SerializeField] private Button _spinButton;
    [SerializeField] private float _firstDelay = 0.05f;
    [SerializeField] private float _slowdownFactor = 1.15f;
    [SerializeField] private float _stopDelay = 0.35f;
    [SerializeField] private float _highlightLife = 3f;

    private Coroutine _rollRoutine;
    private ShopCard _current;

    private void Awake()
    {
        _spinButton.onClick.AddListener(StartRoll);
    }

    public void StartRoll()
    {
        if(_rollRoutine != null) 
            return;

        var purchased = GameManager.Instance.PurchasedSkins;
        int freeCount = _cells.Count(c => !purchased.Contains(c.SkinID));

        if(freeCount == 0)
        {
            return;
        }

        _rollRoutine = StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        _spinButton.interactable = false;

        ShopCard finalCell = GetRandomByRarity();

        float delay = _firstDelay;

        while(delay < _stopDelay)
        {
            LightRandomCell();
            yield return new WaitForSeconds(delay);
            delay *= _slowdownFactor;
        }

        HighlightWin(finalCell);
        yield return new WaitForSeconds(0.6f);

        GiveReward(finalCell);

        _spinButton.interactable = true;
        _rollRoutine = null;
    }

    private ShopCard GetRandomByRarity()
    {
        var purchased = GameManager.Instance.PurchasedSkins;

        var commons = _cells.Where(c => c.RareType == RareType.Common && !purchased.Contains(c.SkinID)).ToList();
        var rares = _cells.Where(c => c.RareType == RareType.Rare && !purchased.Contains(c.SkinID)).ToList();
        var epics = _cells.Where(c => c.RareType == RareType.Epic && !purchased.Contains(c.SkinID)).ToList();

        if(commons.Count + rares.Count + epics.Count == 0)
            return _cells[Random.Range(0, _cells.Count)];

        const float Wc = 56.25f, Wr = 31.25f, We = 6.25f;

        var weighted = new List<(List<ShopCard> pool, float w)>
        {
            (commons, Wc), (rares, Wr), (epics, We)
        }.Where(p => p.pool.Count > 0).ToList();

        float total = weighted.Sum(p => p.w);
        float roll = Random.Range(0f, total);

        float acc = 0f;
        foreach(var (pool, w) in weighted)
        {
            acc += w;
            if(roll <= acc)
                return pool[Random.Range(0, pool.Count)];
        }

        return weighted[0].pool[0];
    }

    private void LightRandomCell()
    {
        var purchased = GameManager.Instance.PurchasedSkins;
        var free = _cells.Where(c => !purchased.Contains(c.SkinID)).ToList();

        ShopCard random = free[Random.Range(0, free.Count)];
        HighlightCell(random);
    }

    private void HighlightCell(ShopCard cell)
    {
        if(_current != null)
            Pulse(_current, false);

        _current = cell;
        Pulse(cell, true);
    }

    private void Pulse(ShopCard card, bool on)
    {
        if(card == null) return;

        var rt = (RectTransform)card.transform;

        rt.DOKill(true); 

        Vector3 baseScale = rt.localScale;

        if(on)
        {
            rt.DOPunchScale(Vector3.one * .15f, .25f, 8, .8f)
              .OnComplete(() => rt.localScale = baseScale);
        }
    }
    
    private void GiveReward(ShopCard winner)
    {
        winner.CardButton.onClick.Invoke();
    }

    private void HighlightWin(ShopCard card)
    {
        Image img = card.GetComponent<Image>();
        if(img == null) img = card.GetComponentInChildren<Image>();
        if(img == null) return;

        Color32 originalColor = img.color;

        Tween glow = img.DOColor(new Color32(255, 255, 160, 255), 0.25f).SetLoops(-1, LoopType.Yoyo);

        Tween pulse = card.transform.DOScale(1.15f, 0.4f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);

        DOVirtual.DelayedCall(_highlightLife, () =>
        {
            glow.Kill();
            pulse.Kill();

            img.DOColor(originalColor, 0.25f);

            card.transform.DOScale(1f, 0.25f);
        });
    }
}
