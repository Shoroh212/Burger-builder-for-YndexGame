using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StarSystem : MonoBehaviour
{
    [Header("Star Images")]
    [SerializeField] private List<Image> _lightStars = new List<Image>(3);

    [Header("Animation")]
    [SerializeField] private float _popScale = 1.3f;
    [SerializeField] private float _animDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _starSound;

    public void StartAnim()
    {
        foreach (var img in _lightStars)
        {
            img.gameObject.SetActive(false);
            img.transform.localScale = Vector3.one;
            var c = img.color; c.a = 0;
            img.color = c;
        }

        int starsToShow = CalculateStars();
        StartCoroutine(AnimateStars(starsToShow));
    }

    private int CalculateStars()
    {
        List<Ingredient> recipe = GameManager.Instance.Recipe;
        List<Ingredient> final = GameManager.Instance.FinalIngredients;

        var recipeCopy = new List<Ingredient>(recipe);
        var finalCopy = new List<Ingredient>(final);

        foreach (var ing in final.ToList())
        {
            if (ing.IsBadIngredient)
                continue;

            var match = recipeCopy.FirstOrDefault(r => AreSameIngredient(r, ing));
            if (match != null)
            {
                recipeCopy.Remove(match);
                finalCopy.Remove(ing);
            }
        }

        bool allRecipeFound = recipeCopy.Count == 0;
        bool hasExtra = finalCopy.Any(ing => ing.IsBadIngredient || !recipe.Any(r => AreSameIngredient(r, ing)));

        if (allRecipeFound && !hasExtra)
            return 3;

        if (allRecipeFound && hasExtra)
            return 2;

        return 1;
    }

    private bool AreSameIngredient(Ingredient a, Ingredient b)
    {
        return a == b || a.Icon == b.Icon;
    }

    private IEnumerator AnimateStars(int count)
    {
        for (int i = 0; i < count && i < _lightStars.Count; i++)
        {
            Image star = _lightStars[i];
            star.gameObject.SetActive(true);
            star.transform.localScale = Vector3.one * _popScale;

            // ������������� ����
            if (_audioSource != null && _starSound != null)
            {
                _audioSource.PlayOneShot(_starSound);
            }

            float t = 0f;
            while (t < _animDuration)
            {
                t += Time.unscaledDeltaTime;
                float norm = t / _animDuration;

                star.transform.localScale = Vector3.Lerp(Vector3.one * _popScale, Vector3.one, norm);

                var c = star.color;
                c.a = norm;
                star.color = c;

                yield return null;
            }

            star.transform.localScale = Vector3.one;
            var finalColor = star.color;
            finalColor.a = 1f;
            star.color = finalColor;

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    public int GetStarsCount()
    {
        return CalculateStars();
    }

}
