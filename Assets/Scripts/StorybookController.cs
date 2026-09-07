using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StorybookController : MonoBehaviour
{
    readonly string[] titles = {
        "THE LITTLE MATCH GIRL", "SCENE 1", "SCENE 2", "SCENE 3", "SCENE 4", "CREDITS"
    };
    readonly string[] subtitles = {
        "An Interactive Storybook", "A Winter Street", "The First Match", "The Second Match", "The Final Match", "The Little Match Girl"
    };
    readonly string[] narration = {
        "On a snowy New Year's Eve, a little girl walks alone through the cold streets.",
        "Cold and alone, she holds a small box of matches tightly in her hands.",
        "She strikes the first match. A warm iron stove seems to glow before her.",
        "Another match reveals a bright room and a table filled with a wonderful feast.",
        "In the brightest light, she sees her beloved grandmother and reaches toward her.",
        "Story inspired by Hans Christian Andersen. Thank you for reading."
    };
    readonly string[] eventTexts = {
        "The story begins...", "Tap! She prepares a match.", "The flame grows warm and bright.",
        "The feast shimmers like a dream.", "Grandmother appears in a gentle glow.", "Return to the beginning."
    };

    int page = 0;
    Canvas canvas;
    Image background;
    Text titleText, subtitleText, narrationText, eventText;
    Button backButton, homeButton, nextButton, storyButton;
    GameObject eventPanel;
    readonly List<RectTransform> snow = new List<RectTransform>();
    AudioSource audioSource;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;
        BuildUI();
        ShowPage(0, false);
    }

    void BuildUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvasGO = new GameObject("StorybookCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        background = CreateImage("Background", canvas.transform, new Color(.05f,.08f,.18f,1));
        Stretch(background.rectTransform);

        CreateDecorations();
        CreateSnow(42);

        var banner = CreateImage("Banner", canvas.transform, new Color(.08f,.10f,.20f,.94f));
        SetRect(banner.rectTransform, .07f,.86f,.93f,.96f);

        titleText = CreateText("Title", banner.transform, 40, TextAnchor.MiddleCenter, FontStyle.Bold);
        SetRect(titleText.rectTransform, 0,.43f,1,1);
        titleText.color = new Color(1f,.95f,.84f,1);

        subtitleText = CreateText("Subtitle", banner.transform, 21, TextAnchor.MiddleCenter, FontStyle.Normal);
        SetRect(subtitleText.rectTransform, 0,0,1,.45f);
        subtitleText.color = new Color(.78f,.85f,.95f,1);

        var narrationPanel = CreateImage("NarrationPanel", canvas.transform, new Color(.06f,.07f,.13f,.92f));
        SetRect(narrationPanel.rectTransform, .08f,.13f,.92f,.30f);
        narrationText = CreateText("Narration", narrationPanel.transform, 26, TextAnchor.MiddleCenter, FontStyle.Normal);
        Stretch(narrationText.rectTransform, 18);
        narrationText.color = Color.white;

        eventPanel = CreateImage("EventPanel", canvas.transform, new Color(.34f,.18f,.12f,.95f)).gameObject;
        SetRect(eventPanel.GetComponent<RectTransform>(), .12f,.38f,.88f,.50f);
        eventText = CreateText("EventText", eventPanel.transform, 24, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(eventText.rectTransform, 14);
        eventText.color = new Color(1f,.94f,.78f,1);
        eventPanel.SetActive(false);

        storyButton = CreateButton("StoryInteraction", canvas.transform, "TAP STORY OBJECT", new Color(.85f,.44f,.22f,.88f));
        SetRect(storyButton.GetComponent<RectTransform>(), .26f,.53f,.74f,.61f);
        storyButton.onClick.AddListener(StoryEvent);

        backButton = CreateButton("Back", canvas.transform, "BACK", new Color(.10f,.12f,.22f,.95f));
        homeButton = CreateButton("Home", canvas.transform, "HOME", new Color(.10f,.12f,.22f,.95f));
        nextButton = CreateButton("Next", canvas.transform, "NEXT", new Color(.10f,.12f,.22f,.95f));
        SetRect(backButton.GetComponent<RectTransform>(), .05f,.025f,.32f,.095f);
        SetRect(homeButton.GetComponent<RectTransform>(), .365f,.025f,.635f,.095f);
        SetRect(nextButton.GetComponent<RectTransform>(), .68f,.025f,.95f,.095f);
        backButton.onClick.AddListener(()=>ShowPage(page-1,true));
        homeButton.onClick.AddListener(()=>ShowPage(0,true));
        nextButton.onClick.AddListener(()=>ShowPage(page+1,true));

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void CreateDecorations()
    {
        // moon
        var moon = CreateImage("Moon", canvas.transform, new Color(1f,.90f,.60f,.95f));
        SetRect(moon.rectTransform,.70f,.68f,.86f,.77f);
        // street / horizon
        var street = CreateImage("Street", canvas.transform,new Color(.06f,.07f,.10f,1));
        SetRect(street.rectTransform,0,0,1,.25f);
        // stylised houses
        for(int i=0;i<5;i++)
        {
            var h = CreateImage("House"+i, canvas.transform, new Color(.09f,.10f,.16f,1));
            float x=.02f+i*.20f;
            SetRect(h.rectTransform,x,.25f,x+.16f,.50f+(i%2)*.07f);
        }
        // warm window
        var window = CreateImage("WarmWindow", canvas.transform,new Color(1f,.58f,.20f,1));
        SetRect(window.rectTransform,.73f,.34f,.81f,.42f);
        // girl silhouette
        var girl = CreateImage("Girl", canvas.transform,new Color(.30f,.18f,.28f,1));
        SetRect(girl.rectTransform,.38f,.30f,.58f,.58f);
    }

    void CreateSnow(int count)
    {
        for (int i=0;i<count;i++)
        {
            var flake = CreateImage("Snow"+i, canvas.transform, new Color(1,1,1,Random.Range(.25f,.85f)));
            var rt=flake.rectTransform;
            float s=Random.Range(3f,8f);
            rt.sizeDelta=new Vector2(s,s);
            rt.anchorMin=rt.anchorMax=new Vector2(Random.value,Random.Range(.15f,1f));
            rt.anchoredPosition=Vector2.zero;
            snow.Add(rt);
        }
    }

    void Update()
    {
        for(int i=0;i<snow.Count;i++)
        {
            var rt=snow[i];
            rt.anchoredPosition += new Vector2(8f,-45f) * Time.deltaTime;
            if(rt.anchoredPosition.y < -700)
            {
                rt.anchorMin=rt.anchorMax=new Vector2(Random.value,1.05f);
                rt.anchoredPosition=Vector2.zero;
            }
        }
    }

    void ShowPage(int p, bool withSound)
    {
        page=Mathf.Clamp(p,0,5);
        titleText.text=titles[page];
        subtitleText.text=subtitles[page];
        narrationText.text=narration[page];
        eventPanel.SetActive(false);

        Color[] colors={
            new Color(.04f,.07f,.16f,1), new Color(.05f,.09f,.19f,1), new Color(.18f,.10f,.12f,1),
            new Color(.22f,.13f,.12f,1), new Color(.08f,.09f,.21f,1), new Color(.04f,.05f,.12f,1)
        };
        background.color=colors[page];

        backButton.gameObject.SetActive(page>0);
        homeButton.gameObject.SetActive(page>0 && page<5);
        nextButton.gameObject.SetActive(page<5);
        nextButton.GetComponentInChildren<Text>().text=page==0 ? "START" : "NEXT";
        storyButton.GetComponentInChildren<Text>().text = page==5 ? "RETURN HOME" : (page==0 ? "BEGIN" : "TAP STORY OBJECT");

        if(withSound) PlayTone(480,0.12f,.10f);
    }

    void StoryEvent()
    {
        eventText.text=eventTexts[page];
        eventPanel.SetActive(true);
        StartCoroutine(Pulse(eventPanel.GetComponent<RectTransform>()));
        PlayTone(page>=2 && page<=4 ? 740 : 560, .28f, .16f);
        if(page==0) StartCoroutine(DelayedPage(1));
        if(page==5) StartCoroutine(DelayedPage(0));
    }

    IEnumerator DelayedPage(int p)
    {
        yield return new WaitForSeconds(.65f);
        ShowPage(p,true);
    }

    IEnumerator Pulse(RectTransform rt)
    {
        float t=0;
        while(t<.3f)
        {
            t+=Time.deltaTime;
            float s=Mathf.Lerp(.92f,1f,t/.3f);
            rt.localScale=Vector3.one*s;
            yield return null;
        }
        rt.localScale=Vector3.one;
    }

    void PlayTone(float frequency,float duration,float volume)
    {
        int rate=44100;
        int samples=Mathf.CeilToInt(rate*duration);
        var clip=AudioClip.Create("tone",samples,1,rate,false);
        float[] data=new float[samples];
        for(int i=0;i<samples;i++)
        {
            float t=(float)i/rate;
            float env=Mathf.Exp(-5f*t/duration);
            data[i]=Mathf.Sin(2*Mathf.PI*frequency*t)*volume*env;
        }
        clip.SetData(data,0);
        audioSource.PlayOneShot(clip);
        Destroy(clip,duration+.2f);
    }

    Font BuiltinFont() { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }

    Image CreateImage(string name,Transform parent,Color color)
    {
        var go=new GameObject(name);
        go.transform.SetParent(parent,false);
        var img=go.AddComponent<Image>();
        img.color=color;
        return img;
    }

    Text CreateText(string name,Transform parent,int size,TextAnchor anchor,FontStyle style)
    {
        var go=new GameObject(name);
        go.transform.SetParent(parent,false);
        var txt=go.AddComponent<Text>();
        txt.font=BuiltinFont();
        txt.fontSize=size;
        txt.alignment=anchor;
        txt.fontStyle=style;
        txt.resizeTextForBestFit=true;
        txt.resizeTextMinSize=14;
        txt.resizeTextMaxSize=size;
        return txt;
    }

    Button CreateButton(string name,Transform parent,string label,Color color)
    {
        var img=CreateImage(name,parent,color);
        var b=img.gameObject.AddComponent<Button>();
        var txt=CreateText("Label",img.transform,23,TextAnchor.MiddleCenter,FontStyle.Bold);
        txt.text=label;
        txt.color=new Color(1f,.96f,.88f,1);
        Stretch(txt.rectTransform,8);
        return b;
    }

    static void Stretch(RectTransform rt,float pad=0)
    {
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
        rt.offsetMin=new Vector2(pad,pad); rt.offsetMax=new Vector2(-pad,-pad);
    }

    static void SetRect(RectTransform rt,float x0,float y0,float x1,float y1)
    {
        rt.anchorMin=new Vector2(x0,y0); rt.anchorMax=new Vector2(x1,y1);
        rt.offsetMin=rt.offsetMax=Vector2.zero;
    }
}
