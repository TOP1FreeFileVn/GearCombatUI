using UnityEngine;

// Preview-only helper (not game logic).
// Tab  = switch screen state: Buy gear <-> Battle
// 0    = hide all popups, 1..8 = show popup N (order of children under "Popups")
public class UIPreviewSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ScreenState
    {
        public string name;
        public GameObject hud;        // screen-space combat HUD snapshot
        public GameObject worldUI;    // HP bars / damage numbers in world space
        public GameObject background; // game world render (not UI)
    }

    public ScreenState[] states;
    public int currentState;

    void Start()
    {
        ShowState(currentState);
        ShowPopup(-1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ShowState((currentState + 1) % states.Length);
        if (Input.GetKeyDown(KeyCode.Alpha0)) ShowPopup(-1);
        for (int i = 1; i <= 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) ShowPopup(i - 1);
    }

    public void ShowState(int index)
    {
        currentState = index;
        for (int i = 0; i < states.Length; i++)
        {
            bool on = i == index;
            if (states[i].hud) states[i].hud.SetActive(on);
            if (states[i].worldUI) states[i].worldUI.SetActive(on);
            if (states[i].background) states[i].background.SetActive(on);
        }
    }

    Transform PopupsRoot()
    {
        var hud = states[currentState].hud;
        return hud ? hud.transform.Find("Popups") : null;
    }

    public void ShowPopup(int index)
    {
        foreach (var s in states)
        {
            var root = s.hud ? s.hud.transform.Find("Popups") : null;
            if (root == null) continue;
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                bool on = i == index && root == PopupsRoot();
                var cg = child.GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = on ? 1 : 0; cg.blocksRaycasts = on; cg.interactable = on; }
                // panels that slide in from off-screen are reset to the center
                foreach (RectTransform rt in child)
                    if (on && Mathf.Abs(rt.anchoredPosition.y) > 1500) rt.anchoredPosition = Vector2.zero;
            }
        }
    }
}
