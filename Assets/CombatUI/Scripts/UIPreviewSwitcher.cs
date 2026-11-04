using UnityEngine;

// Preview-only helper (not game logic): show each popup of the combat UI with number keys.
// 0 = hide all popups, 1..9 = show popup N (order of children under "Popups").
public class UIPreviewSwitcher : MonoBehaviour
{
    public Transform popupsRoot;

    void Start() => Show(-1);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) Show(-1);
        for (int i = 1; i <= 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) Show(i - 1);
    }

    public void Show(int index)
    {
        if (popupsRoot == null) return;
        for (int i = 0; i < popupsRoot.childCount; i++)
        {
            var child = popupsRoot.GetChild(i);
            bool on = i == index;
            var cg = child.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = on ? 1 : 0; cg.blocksRaycasts = on; cg.interactable = on; }
            // panels that slide in from off-screen are reset to the center
            foreach (RectTransform rt in child)
                if (on && Mathf.Abs(rt.anchoredPosition.y) > 1500) rt.anchoredPosition = Vector2.zero;
        }
    }
}
