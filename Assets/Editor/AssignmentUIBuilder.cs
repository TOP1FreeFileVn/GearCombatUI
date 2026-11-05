#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Native, editable reconstruction of the two Level 12 layouts in the assignment.
// Only the world backdrop and sample ad are raster crops; controls and gear stay separate.
public static class AssignmentUIBuilder
{
    const string Art = "Assets/GearFinal/Assets/";
    const string Ref = "Assets/CombatUI/ReferenceSprites/";
    static TMP_FontAsset font;
    static readonly Color Ink = new Color32(44, 58, 83, 255);

    [MenuItem("Combat UI/Match Assignment Reference")]
    public static void Build() => CombatUIDemoBuilder.Build();

    public static void Apply(UIPreviewSwitcher preview)
    {
        foreach (string p in AssetDatabase.FindAssets("t:Texture2D", new[] {Ref.TrimEnd('/')}))
        {
            var path = AssetDatabase.GUIDToAssetPath(p);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;
            ti.SaveAndReimport();
        }
        font = preview.states[0].hud.GetComponentsInChildren<TMP_Text>(true).First(t => t.font != null).font;
        for (int i = 0; i < preview.states.Length; i++)
        {
            var st = preview.states[i];
            st.worldUI.SetActive(false);
            st.background.SetActive(false);
            // The reference backdrop below replaces the Stage 5 world snapshot.
            st.worldUI = null;
            st.background = null;
            BuildHUD(st.hud, i == 1);
        }
        preview.ShowState(0);
        preview.ShowPopup(-1);
    }

    static void BuildHUD(GameObject hud, bool battle)
    {
        foreach (Transform c in hud.transform)
            if (c.name != "Popups") { c.gameObject.SetActive(false); c.name = "Snapshot_" + c.name; }
        var scaler = hud.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(740, 1616);
        scaler.matchWidthOrHeight = 1;
        var responsive = hud.GetComponent<ResponsiveCanvasScaler>();
        responsive.minWidth = 740;
        var ui = Rect("ReferenceUI", hud.transform);
        Stretch(ui);
        var layout = hud.AddComponent<AssignmentResponsiveLayout>();
        layout.battle = battle;
        layout.sea = Img("Sea", ui, Ref+"SeaReference.png", 0,0,740,1616).rectTransform;
        layout.sea.GetComponent<Image>().sprite = null;
        layout.sea.GetComponent<Image>().color = new Color32(22, 112, 151, 255);
        layout.sky = Img("Sky", ui, Ref+"SkyReference.png",0,0,740,260).rectTransform;
        layout.field = Img("Battlefield",ui,Ref+(battle?"BattlefieldReference.png":"BuyFieldReference.png"),0,260,740,595).rectTransform;
        layout.header = Rect("LevelProgressBar", ui);
        Header(layout.header, battle);
        layout.board = Rect("GearContainer", ui);
        Board(layout.board, battle);
        layout.shop = Rect("ShopContainer", ui);
        Shop(layout.shop);
        layout.boosters = Rect("BoosterContainer", ui);
        Boosters(layout.boosters);
        layout.reward = Button("CoinRewardBtn",ui,Art+"Panel/btnRewardCoin.png",0,0,217,100).GetComponent<RectTransform>();
        Label("50",layout.reward,"50",118,20,70,50,44,Color.white);
        Img("Ads",layout.reward,Art+"Panel/icon video ad.png",166,-9,55,40);
        layout.banner = Rect("SampleBanner", ui);
        layout.banner.gameObject.AddComponent<Image>().color = Color.black;
        var bannerImage=Img("ReferenceAd",layout.banner,Ref+"BannerReference.png",0,0,740,180);
        Stretch(bannerImage.rectTransform);bannerImage.rectTransform.pivot=new Vector2(.5f,.5f);
        bannerImage.preserveAspect=true;
        // Popups remain above the native main screen and keep their existing close buttons.
        var popups = hud.transform.Find("Popups");
        if (popups) popups.SetAsLastSibling();
        layout.Refresh();
    }

    static void Header(RectTransform p, bool battle)
    {
        Label("LevelTitle",p,"Level12",255,72,230,44,29,Color.white);
        Button("SettingsBtn",p,Art+"Panel/Setting.png",17,127,55,63);
        Button("BackBtn",p,Art+"Panel/closeBtn.png",672,128,55,58);
        Img("ProgressBack",p,Art+"Panel/slider gameplay_background.png",108,148,515,20);
        Img("ProgressFill",p,Art+"Panel/slider gameplay_fill_cyan.png",111,152,214,12);
        foreach (float x in new float[] {202,310,525}) Img("Skull",p,Art+"Icon/stage.png",x,119,28,49);
        Img("Finish",p,Art+"Icon/icon_flag finish.png",614,122,25,35);
        Img("Distance",p,Art+"Panel/distance pointer.png",254,163,84,34);
        Label("distanceText",p,"123m",255,168,82,23,19,Color.white);
        var hard = Img("Hard",p,Art+"UI/bg_grey.png",315,214,99,32);
        hard.color = new Color32(230,85,105,255);
        Label("difficultyText",p,"Hard",315,212,99,34,25,Color.white);
        var coin = Img("StatCoin",p,Art+"UI/bg_grey.png",0,207,143,51);
        coin.color = new Color32(43,64,91,255);
        Img("Sun",p,Art+"Icon/icon_sun.png",9,202,52,53);
        Label("textCoin",p,"165",65,208,73,47,28,new Color32(255,220,43,255),false);
        var x2 = Button("SpeedBtn",p,Art+"Panel/button_booster.png",673,204,53,53);
        Label("Text",x2.transform,"x2",2,5,49,38,25,Color.white);
    }

    static void Board(RectTransform p, bool battle)
    {
        float w = battle ? 570 : 698;
        var panel = Img("Panel",p,Art+"Panel/panel_rec_lightgreen.png",0,0,w,700);
        Stretch(panel.rectTransform);
        panel.type = Image.Type.Sliced;
        panel.pixelsPerUnitMultiplier = 2.8f;
        // All contents use 698-wide coordinates. The combat board scales the same layout down.
        var contents = Rect("Contents",p);
        contents.anchorMin=contents.anchorMax=new Vector2(0,1);
        contents.pivot=new Vector2(0,1);
        contents.anchoredPosition=new Vector2(0,0);
        contents.sizeDelta=new Vector2(698,700);
        contents.localScale=Vector3.one*(w/698);
        var speed=Img("StatSpeed",contents,Art+"Panel/white rec.png",37,26,310,50);
        speed.type=Image.Type.Sliced;speed.pixelsPerUnitMultiplier=4;
        speed.color=battle?new Color32(173,58,43,255):new Color32(42,60,94,255);
        var power=Img("StatDamage",contents,Art+"Panel/white rec.png",354,26,312,50);
        power.type=Image.Type.Sliced;power.pixelsPerUnitMultiplier=4;
        power.color=battle?new Color32(0,143,38,255):new Color32(42,60,94,255);
        Img("SpeedIcon",contents,Art+"Icon/icon_move speed.png",46,34,43,36);
        Img("DamageIcon",contents,Art+"Icon/icon_fire rate.png",369,34,46,35);
        Label("textSpeed",contents,"52.3mph",102,31,191,42,24,Color.white,false);
        Label("PowerText",contents,"73.3",421,31,191,42,24,Color.white,false);
        if (battle)
        {
            Img("Down",contents,Art+"UI/arrow down.png",295,41,29,20);
            Img("Up",contents,Art+"UI/arrow up.png",621,41,29,20);
        }
        string[] weapons={"boat wheel.png","Gun_pea_slot.png","gun_berry_slot.png",null,null,"boat sail.png"};
        for(int i=0;i<6;i++)
        {
            float x=37+i*106;
            Img("WeaponSlot"+i,contents,Ref+"WeaponSlotInactive.png",x,85,95,137);
            if(weapons[i]!=null)
            {
                Img("WeaponSlot"+i,contents,i<2?Ref+"WeaponSlotConnected.png":Art+"Slot gear - Empty.png",x,85,95,137);
                Img("Weapon",contents,Art+"Weapon/"+weapons[i],x+4,98,85,92).preserveAspect=true;
                if(i<3) Label("Rate",contents,i==0?"0.24/s":(i==1?"1.17/s":""),x,176,93,24,20,Color.white);
            }
            Img("Connected",contents,i<2?Ref+"GearConnectorActive.png":Art+"Gear/Gear connected.png",x+18,197,60,60);
        }
        var pads=Rect("Grid",contents);
        // Measured independently in the two PPT screenshots, then normalized to a 698-wide board.
        float pitchX=battle?107.6f:106.6f, pitchY=battle?107.6f:104.7f;
        Pos(pads,battle?26.2f:38.8f,battle?266.9f:259.9f,637,418);
        Image Cell(string name,string path,int c,int r,float width=93,float height=93,float dx=0,float dy=0)
        {
            return Img(name,pads,path,c*pitchX+46.5f-width*.5f+dx,
                r*pitchY+46.5f-height*.5f+dy,width,height);
        }
        void Flower(string name,int c,int r)
        {
            float gw=battle?113:112,gh=battle?116:115;
            if(name=="GearPurple1"){gw=battle?110:107;gh=battle?113:112;}
            if(name=="GearOrange2" && !battle){gw=109;gh=112;}
            if(name=="GearRed8" && !battle)gh=112;
            Cell(name,Ref+name+".png",c,r,gw,gh,battle?5:0,battle?-15:-3);
        }
        // Inactive pad is the lighter green leaf, including empty cells behind gears.
        for(int r=0;r<4;r++)for(int c=0;c<6;c++)
        {
            bool active=(r==0&&c==2)||(r==1&&c==1)||(r==2&&c<3);
            var leaf=Cell("Pad_"+r+"_"+c,Art+"Gear/"+(active?"pad-red.png":"pad.png"),c,r);
            leaf.rectTransform.pivot = new Vector2(.5f,.5f);
            leaf.rectTransform.anchoredPosition += new Vector2(46.5f,-46.5f);
            leaf.rectTransform.localEulerAngles=new Vector3(0,0,(r*83+c*47)%360);
        }
        Flower("GearOrange2",0,0); Flower("GearRed8",1,0); Flower(battle?"GearPinkPotato":"GearPinkSmile",4,0);
        Flower("GearCyan4",0,1); Flower("GearPurple1",2,1); Flower("GearCyan4",3,1); Flower("GearPurple1",4,1);
        // The rotor body occupies only 164/232 of its source texture, so its rect must be larger.
        var wheel=Cell("Rotor",Art+"Gear/Gear Rotor.png",1,1,136,136,0,battle?-1.6f:5.3f);
        wheel.rectTransform.pivot = new Vector2(.5f,.5f);
        wheel.rectTransform.anchoredPosition += new Vector2(68,-68);
        wheel.rectTransform.localEulerAngles=new Vector3(0,0,-135);
        Flower("GearSunPlus",0,3);
        if (battle)
        {
            Flower("GearDamage",0,2); Flower("GearShield",2,3);
            Flower("GearCyan4",3,2);
            Cell("Shield",Ref+"ShieldIcon.png",3,2,43,43,5,-15);
            Cell("WeaponFlower",Art+"Gear/Gear - Weapon.png",3,3,113,116,5,-15);
            Cell("CombatPlant",Ref+"CombatPlantGear.png",3,0,113,116,5,-15);
            Cell("CombatWeapon",Ref+"CombatWeaponGear.png",4,2,113,116,5,-15);
            Cell("CombatStone",Ref+"CombatStoneGear.png",5,3,107,111,5,-15);
        }
        else
        {
            Flower("GearOrange2",3,2); Flower("GearPurple1",4,2); Flower("GearPinkPotato",3,3);
            Cell("BlueFlower",Art+"Gear/Gear - Stats.png",5,3,112,115,0,-3);
            Cell("Plus",Ref+"GreenPlus.png",5,3,46,49,0,-3);
        }
    }

    static void Shop(RectTransform p)
    {
        var bg=Img("Panel",p,Art+"Panel/bar bottom buy gear.png",0,0,740,364);
        Stretch(bg.rectTransform);
        for(int i=0;i<4;i++)
        {
            float x=29+i*177;
            Img("Offer"+i,p,Art+"Panel/button_buy gear.png",x,43,148,150);
            if(i==3)continue;
            Img("Gear",p,Ref+new[]{"GearPurple1.png","GearOrange2.png","GearCyan4.png"}[i],x+24,64,105,105);
            var price=Img("Price",p,Art+"UI/bg_grey.png",x+19,151,112,39);
            price.color=new Color32(67,70,101,210);
            Img("Sun",p,Art+"Icon/icon_sun.png",x+24,153,29,29);
            Label("price_text",p,i==0?"5":i==1?"10":"Free",x+55,151,75,36,26,Color.white,false);
            if(i==2)
            {
                Img("Info",p,Art+"Panel/info.png",x+1,41,40,40);
                Img("Ads",p,Art+"Panel/icon video ad.png",x+98,40,57,40);
            }
        }
        var strip=Img("ButtonStrip",p,Art+"Panel/white rec.png",27,212,686,136);
        strip.type=Image.Type.Sliced;
        strip.color=new Color32(109,73,182,255);
        var refresh=Button("RefreshBtn",p,Art+"Panel/button_big_green.png",113,233,185,99);
        Label("Label",refresh.transform,"Refresh",4,5,177,43,28,Color.white);
        Img("Sun",refresh.transform,Art+"Icon/icon_sun.png",60,50,33,34);
        Label("Price",refresh.transform,"5",91,48,42,37,29,Color.white);
        var start=Button("StartGameBtn",p,Art+"Panel/btn_blue.png",314,233,318,99);
        Label("Label",start.transform,"START!",0,8,318,79,40,Color.white);
    }

    static void Boosters(RectTransform p)
    {
        for(int i=0;i<4;i++)
        {
            float y=i*135;
            var b=Button("Booster"+i,p,Art+"Panel/button_booster.png",0,y,112,112);
            if(i==0) Img("Heart",b.transform,Art+"Icon/icon_heart.png",23,21,68,63);
            var badge=Img("Count",b.transform,Art+"Panel/dot_green.png",79,77,41,41);
            Label("countText",badge.transform,"3",0,0,41,39,30,Color.white);
        }
    }

    static RectTransform Rect(string name,Transform p)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(p,false); r.localScale=Vector3.one; return r;
    }
    static Image Img(string name,Transform p,string path,float x,float y,float w,float h)
    {
        var rt=Rect(name,p); Pos(rt,x,y,w,h);
        var im=rt.gameObject.AddComponent<Image>(); im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path); im.raycastTarget=false;
        if(!im.sprite)Debug.LogError("Missing reference sprite: "+path);
        return im;
    }
    static Button Button(string name,Transform p,string path,float x,float y,float w,float h)
    {
        var im=Img(name,p,path,x,y,w,h); im.raycastTarget=true;
        var b=im.gameObject.AddComponent<Button>(); b.targetGraphic=im; return b;
    }
    static TMP_Text Label(string name,Transform p,string value,float x,float y,float w,float h,float size,Color color,bool outline=true)
    {
        var rt=Rect(name,p);Pos(rt,x,y,w,h);
        var t=rt.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font;t.text=value;t.fontSize=size;t.color=color;
        t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.enableWordWrapping=false;
        if(outline){t.outlineColor=Ink;t.outlineWidth=.16f;}
        return t;
    }
    static void Pos(RectTransform r,float x,float y,float w,float h)
    {
        r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
    }
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}

    [MenuItem("Combat UI/Capture Full Screens")]
    public static void CaptureFullScreens()
    {
        EditorSceneManager.OpenScene("Assets/CombatUI/CombatUI_Demo.unity");
        var preview=Object.FindObjectOfType<UIPreviewSwitcher>();
        var cam=Camera.main;
        Directory.CreateDirectory("Screenshots");
        for(int ratio=0;ratio<3;ratio++)for(int state=0;state<2;state++)
        {
            // The slide images have slightly different portrait dimensions. Keep each exact ratio.
            int width=ratio==0?1080:ratio==1?1536:state==0?1116:1124;
            int height=ratio==0?2520:ratio==1?2048:state==0?2436:2444;
            string prefix=ratio==0?"r921":ratio==1?"r34":"slide";
            preview.ShowState(state);preview.ShowPopup(-1);
            var hud=preview.states[state].hud;
            hud.GetComponent<ResponsiveCanvasScaler>().enabled=false;
            hud.GetComponent<CanvasScaler>().enabled=false;
            hud.GetComponent<Canvas>().scaleFactor=Mathf.Min(width/740f,height/1616f);
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;
            for(int k=0;k<3;k++){Canvas.ForceUpdateCanvases();hud.GetComponent<AssignmentResponsiveLayout>().Refresh();}
            cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes("Screenshots/"+prefix+"_"+(state==0?"buy":"battle")+".png",tex.EncodeToPNG());
            RenderTexture.active=null;cam.targetTexture=null;Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);
        }
        // Reload to discard temporary rendering settings without modifying the saved scene.
        EditorSceneManager.OpenScene("Assets/CombatUI/CombatUI_Demo.unity");
        Debug.Log("ASSIGNMENT_FULL_SCREEN_CAPTURE_COMPLETE");
    }

    public static void Verify()
    {
        CombatUIDemoBuilder.Build();
        var preview=Object.FindObjectOfType<UIPreviewSwitcher>();
        var cam=Camera.main;
        Directory.CreateDirectory("Screenshots");
        for(int ratio=0;ratio<2;ratio++)for(int state=0;state<2;state++)
        {
            int width=ratio==0?1080:1536,height=ratio==0?2520:2048;
            preview.ShowState(state);preview.ShowPopup(-1);
            var hud=preview.states[state].hud;
            hud.GetComponent<ResponsiveCanvasScaler>().enabled=false;
            hud.GetComponent<CanvasScaler>().enabled=false;
            hud.GetComponent<Canvas>().scaleFactor=Mathf.Min(width/740f,height/1616f);
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;
            for(int k=0;k<3;k++){Canvas.ForceUpdateCanvases();hud.GetComponent<AssignmentResponsiveLayout>().Refresh();}
            cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes("Screenshots/"+(ratio==0?"r921":"r34")+"_"+(state==0?"buy":"battle")+".png",tex.EncodeToPNG());
            RenderTexture.active=null;cam.targetTexture=null;Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);
            hud.GetComponent<ResponsiveCanvasScaler>().enabled=true;hud.GetComponent<CanvasScaler>().enabled=true;
        }
        // Save production settings, not temporary render scale factors.
        CombatUIDemoBuilder.Build();
        Debug.Log("ASSIGNMENT_UI_VERIFY_COMPLETE");
    }

    static int smokeFrames;
    public static void Smoke()
    {
        EditorSceneManager.OpenScene("Assets/CombatUI/CombatUI_Demo.unity");
        EditorApplication.playModeStateChanged += change =>
        {
            if(change==PlayModeStateChange.EnteredPlayMode)
            {smokeFrames=0;EditorApplication.update+=SmokeTick;}
        };
        // Persist callback across domain reload using a SessionState-backed initializer.
        SessionState.SetBool("AssignmentSmokePending",true);
        EditorApplication.isPlaying=true;
    }

    [InitializeOnLoadMethod]
    static void ResumeSmoke()
    {
        if(!SessionState.GetBool("AssignmentSmokePending",false))return;
        EditorApplication.playModeStateChanged += change =>
        {
            if(change==PlayModeStateChange.EnteredPlayMode)
            {smokeFrames=0;EditorApplication.update+=SmokeTick;}
        };
    }
    static void SmokeTick()
    {
        if(!EditorApplication.isPlaying || ++smokeFrames<20)return;
        EditorApplication.update-=SmokeTick;
        SessionState.SetBool("AssignmentSmokePending",false);
        try
        {
            var p=Object.FindObjectOfType<UIPreviewSwitcher>();
            var buy=p.states[0].hud.transform;
            if(p.currentState!=0)throw new System.Exception("Expected gear state on load");
            buy.Find("ReferenceUI/ShopContainer/StartGameBtn").GetComponent<Button>().onClick.Invoke();
            if(p.currentState!=1 || !p.states[1].hud.activeSelf || p.states[0].hud.activeSelf)
                throw new System.Exception("START did not switch to combat");
            var hud=p.states[1].hud.transform;
            hud.Find("ReferenceUI/LevelProgressBar/SettingsBtn").GetComponent<Button>().onClick.Invoke();
            var settings=hud.Find("Popups/SettingsPopup").GetComponent<CanvasGroup>();
            if(settings.alpha<.99f || !settings.blocksRaycasts)throw new System.Exception("Settings did not open");
            hud.Find("Popups/SettingsPopup/BG/CloseBtn").GetComponent<Button>().onClick.Invoke();
            if(settings.alpha>.01f || settings.blocksRaycasts)throw new System.Exception("Settings did not close");
            hud.Find("ReferenceUI/LevelProgressBar/BackBtn").GetComponent<Button>().onClick.Invoke();
            hud.Find("Popups/PopupGiveUp/GiveUpUI/Button/Exit").GetComponent<Button>().onClick.Invoke();
            if(p.currentState!=0)throw new System.Exception("Quit did not restore gear state");
            Debug.Log("ASSIGNMENT_UI_SMOKE_COMPLETE");EditorApplication.Exit(0);
        }
        catch(System.Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
#endif
