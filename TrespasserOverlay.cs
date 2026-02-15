using System.Collections;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using System.Reflection;

namespace Trespasser
{
    internal enum OrbitDirection
    {
        Clockwise,
        CounterClockwise
    }

    internal static class TrespasserOverlay
    {
        private const float BG_ALPHA_MAX = 0.4f;
        private const float BG_SCALE_MIN = 0.9f;
        private const float BG_SCALE_MAX = 1.1f;
        private const float ICON_DELAY = 0.33f;
        private const float ICON_FADE = 0.27f;
        private const float NAME_DELAY = 0.50f;
        private const float NAME_FADE = 0.17f;
        private const float BULLETS_DELAY = 0.57f;
        private const float BULLETS_FADE = 0.20f;
        private const float WOLF_TOP_DELAY = 3.00f;
        private const float WOLF_BOTTOM_DELAY = 5.00f;
        private const float FLARE_DELAY = 1.00f;
        private const float WOLF_SWEEP_DEGREES = 60f;
        private const float WOLF_TOTAL_DURATION = 15f;
        private const float WOLF_MOVE_DURATION = 15f;
        private const float MACKENZIE_SCALE = 0.5f;
        private const float WOLF_SCALE = 0.5f;
        private const float WOLF_ORBIT_PX = 600f;
        private const float WOLF_FINAL_POSITION = 0f;  // NOTE: Adjusting this will NOT adjust rotation - ONLY orbit position! It is meant to match the orbit animation to the actual rotation of the wolf texture!
        private const float FLARE_PEAK_ALPHA = 0.4f;
        private const float FLARE_GROUP_A_RATIO = 0.705f;
        private const float FLARE_R_PEAK = 0.90f;
        private const float FLARE_R_POWER = 2.0f;
        private const float FLARE_G_PEAK = 0.92f;
        private const float FLARE_G_POWER = 1.5f;
        private const float FLARE_B_BASE = 0.35f;
        private const float FLARE_B_RANGE = 0.65f;
        private const float FLARE_CORE_SIZE = 100f;
        private const float FLARE_SCALE_A_MIN = 0.2f;
        private const float FLARE_SCALE_A_MAX = 0.8f;
        private const float FLARE_SCALE_B_MIN = 0.3f;
        private const float FLARE_SCALE_B_MAX = 0.9f;

        private static readonly Color NAME_COLOR = new(0.98f, 0.98f, 0.98f, 1f);
        private static readonly Color BULLETS_COLOR = new(0.78f, 0.78f, 0.78f, 1f);
        private static readonly Color ICON_COLOR = new(0.98f, 0.98f, 0.98f, 1f);
        private static readonly Color FLARE_CORE_COLOR = new(0.98f, 0.98f, 0.98f, 1f);
        private static readonly Vector2 FLARE_POSITION = new Vector2(25f, 95f);

        // Per-sprite pulse config: (period, pulseWidth, phaseOffset, peakRatio)
        // Indices 0-4 = Group A (BGtexture1-5), 5-8 = Group B (BGtexture6-9)
        private static readonly (float period, float width, float phase, float peak)[] FLARE_PULSES =
        {
            (4.15f, 1.50f, 1.425f, FLARE_GROUP_A_RATIO),  // tex1
            (4.15f, 1.50f, 1.025f, FLARE_GROUP_A_RATIO),  // tex2
            (4.15f, 1.80f, 0.725f, FLARE_GROUP_A_RATIO),  // tex3
            (4.15f, 1.40f, 1.800f, FLARE_GROUP_A_RATIO),  // tex4
            (4.15f, 1.50f, 1.575f, FLARE_GROUP_A_RATIO),  // tex5
            (5.50f, 2.50f, 1.400f, 1.000f),               // tex6
            (5.50f, 2.50f, 1.400f, 1.000f),               // tex7
            (5.50f, 3.00f, 0.900f, 1.000f),               // tex8
            (5.50f, 3.00f, 5.400f, 1.000f),               // tex9
        };

        private static GameObject mCanvasObject;
        private static RawImage mBgImage;
        private static RectTransform mBgRect;
        private static RawImage mFgImage;
        private static RectTransform mFgRect;
        private static RawImage mIconImage;
        private static TextMeshProUGUI mNameText;
        private static TextMeshProUGUI mBulletsText;
        private static object mActiveCoroutine;
        private static bool mIsSelectFading;
        private static bool mIsDeselectFading;
        private static GameObject mFgGroupObject;
        private static CanvasGroup mFgGroup;
        private static RawImage mWolfTopImage;
        private static RectTransform mWolfTopRect;
        private static RawImage mWolfBottomImage;
        private static RectTransform mWolfBottomRect;
        private static Vector2 mFgCenter;
        private static Vector2 mWolfTopSize;
        private static Vector2 mWolfBotSize;
        private static GameObject mMovingBgClone;
        private static UISprite[] mMovingBgSprites;
        private static object mFlareCoroutine;
        private static float mFlareMasterAlpha;
        private static UITexture mFlareCoreTexture;
        private static float[] mFlareScaleSignY;

        private static OrbitDirection mOrbitDirection = OrbitDirection.CounterClockwise;

        // Temporary diagnostics — scale sampler on original MovingBackground
        private static Transform mSourceBgTransform;
        private static Transform[] mSourceBgChildTransforms;

        internal static OrbitDirection WolfOrbitDirection
        {
            get => mOrbitDirection;
            set => mOrbitDirection = value;
        }
        internal static bool IsSelectFading => mIsSelectFading;
        internal static bool IsDeselectFading => mIsDeselectFading;


        internal static void Initialize(Panel_SelectExperience panel)
        {
            Destroy();

            Panel_SelectExperience.XPModeMenuItem stalkerItem = panel.m_MenuItems[2];
            UITexture stalkerFgTexture = FindNamedTexture(stalkerItem.m_Display, "XPStalkerTexture");
            if (stalkerFgTexture == null)
                return;

            Panel_SelectExperience.XPModeMenuItem interloperItem = panel.m_MenuItems[4];
            UITexture interloperBgTexture = FindNamedTexture(interloperItem.m_Display, "InterloperXPTexture_bg");
            if (interloperBgTexture == null || interloperBgTexture.mainTexture == null)
                return;

            Panel_SelectExperience.XPModeMenuItem trespasserItem = panel.m_MenuItems[3];
            DisableAllNguiWidgets(trespasserItem.m_Display);
            CreateCanvas();
            CreateImage(interloperBgTexture, interloperItem.m_Display, out mBgImage, out mBgRect, "TrespasserBg");
            CreateForegroundGroup(stalkerFgTexture, stalkerItem.m_Display);
            CreateDescElements(interloperItem.m_Display);
            GenerateFlareAnimation(trespasserItem.m_Display);
            ApplyHidden();
        }


        internal static void FadeIn(float duration)
        {
            StopActiveCoroutine();
            mIsSelectFading = true;
            mIsDeselectFading = false;
            mActiveCoroutine = MelonCoroutines.Start(FadeInCoroutine(duration,
                () => { mIsSelectFading = false; }));
        }


        internal static void FadeOut(float duration)
        {
            StopActiveCoroutine();
            mIsDeselectFading = true;
            mIsSelectFading = false;
            mActiveCoroutine = MelonCoroutines.Start(FadeOutCoroutine(duration,
                () =>
                {
                    mIsDeselectFading = false;
                    if (mBgRect != null)
                        mBgRect.localScale = Vector3.one * BG_SCALE_MIN;
                }));
        }


        internal static void Hide()
        {
            StopActiveCoroutine();
            mIsSelectFading = false;
            mIsDeselectFading = false;
            ApplyHidden();
        }


        internal static void Destroy()
        {
            StopActiveCoroutine();
            StopFlareAnimation();
            mSourceBgTransform = null;
            mSourceBgChildTransforms = null;
            mIsSelectFading = false;
            mIsDeselectFading = false;

            if (mCanvasObject != null)
            {
                UnityEngine.Object.Destroy(mCanvasObject);
                mCanvasObject = null;
            }

            mBgImage = null;
            mBgRect = null;
            mFgGroupObject = null;
            mFgGroup = null;
            mFgImage = null;
            mFgRect = null;
            mWolfTopImage = null;
            mWolfTopRect = null;
            mWolfBottomImage = null;
            mWolfBottomRect = null;
            if (mMovingBgClone != null)
            {
                UnityEngine.Object.Destroy(mMovingBgClone);
                mMovingBgClone = null;
            }
            mMovingBgSprites = null;
            mFlareCoreTexture = null;
            mFlareScaleSignY = null;
            mIconImage = null;
            mNameText = null;
            mBulletsText = null;
        }


        private static void DisableAllNguiWidgets(GameObject display)
        {
            UIWidget[] widgets = display.GetComponentsInChildren<UIWidget>(true);
            foreach (UIWidget widget in widgets)
                widget.enabled = false;
        }


        private static void GenerateFlareAnimation(GameObject trespasserDisplay)
        {
            Transform grandParent = trespasserDisplay.transform.parent.parent;
            if (grandParent == null)
                return;

            Transform source = null;
            for (int i = 0; i < grandParent.childCount; i++)
            {
                Transform child = grandParent.GetChild(i);
                if (child.name.Contains("MovingBackground"))
                {
                    source = child;
                    break;
                }
            }

            if (source == null)
                return;

            // Capture original transform for scale sampling
            mSourceBgTransform = source;
            List<Transform> childTransforms = new();
            for (int c = 0; c < source.childCount; c++)
                childTransforms.Add(source.GetChild(c));
            mSourceBgChildTransforms = childTransforms.ToArray();

            mMovingBgClone = UnityEngine.Object.Instantiate(source.gameObject, trespasserDisplay.transform);
            mMovingBgClone.transform.localPosition += new Vector3(FLARE_POSITION.x, FLARE_POSITION.y, 0);
            mMovingBgClone.transform.localScale = source.localScale * 0.2f;
            mMovingBgClone.name = "TrespasserFlare";

            // Kill the cloned Animator — we drive alpha ourselves
            Animator animator = mMovingBgClone.GetComponent<Animator>();
            if (animator != null)
                UnityEngine.Object.Destroy(animator);

            mMovingBgSprites = mMovingBgClone.GetComponentsInChildren<UISprite>(true);

            // Capture Y-sign per sprite (tex4/tex5 are mirrored with negative Y)
            mFlareScaleSignY = new float[mMovingBgSprites.Length];
            for (int i = 0; i < mMovingBgSprites.Length; i++)
                mFlareScaleSignY[i] = mMovingBgSprites[i].transform.localScale.y < 0f ? -1f : 1f;

            Color hidden = new(0f, 0f, FLARE_B_BASE, 0f);
            foreach (UISprite sprite in mMovingBgSprites)
                sprite.color = hidden;

            // Bright white-blue core point at the center of the flare
            CreateFlareCore();

            mMovingBgClone.SetActive(false);
        }


        private static void CreateFlareCore()
        {
            int size = (int)FLARE_CORE_SIZE;
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 1f - Mathf.SmoothStep(0f, 1f, dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            GameObject coreObj = new("FlareCore");
            coreObj.transform.SetParent(mMovingBgClone.transform, false);
            coreObj.transform.localPosition = Vector3.zero;

            mFlareCoreTexture = coreObj.AddComponent<UITexture>();
            mFlareCoreTexture.mainTexture = tex;
            mFlareCoreTexture.width = size;
            mFlareCoreTexture.height = size;
            mFlareCoreTexture.color = FLARE_CORE_COLOR;
            mFlareCoreTexture.depth = 999;
        }


        private static void StartFlareAnimation()
        {
            StopFlareAnimation();
            if (mMovingBgSprites == null || mMovingBgSprites.Length == 0)
                return;
            mFlareCoroutine = MelonCoroutines.Start(FlareCoroutine());
        }


        private static void StopFlareAnimation()
        {
            if (mFlareCoroutine != null)
            {
                MelonCoroutines.Stop(mFlareCoroutine);
                mFlareCoroutine = null;
            }
        }


        private static IEnumerator FlareCoroutine()
        {
            float elapsed = 0f;
            int spriteCount = Mathf.Min(mMovingBgSprites.Length, FLARE_PULSES.Length);

            while (true)
            {
                elapsed += Time.deltaTime;

                for (int i = 0; i < spriteCount; i++)
                {
                    (float period, float width, float phase, float peak) pulse = FLARE_PULSES[i];
                    float tInCycle = (elapsed + pulse.phase) % pulse.period;
                    float pulseAlpha = 0f;

                    if (tInCycle < pulse.width)
                    {
                        float sinVal = Mathf.Sin(Mathf.PI * tInCycle / pulse.width);
                        pulseAlpha = FLARE_PEAK_ALPHA * pulse.peak * sinVal * sinVal;
                    }

                    float finalAlpha = pulseAlpha * mFlareMasterAlpha;
                    float intensity = pulse.peak > 0.001f
                        ? Mathf.Clamp01(pulseAlpha / (FLARE_PEAK_ALPHA * pulse.peak))
                        : 0f;

                    float r = Mathf.Pow(intensity, FLARE_R_POWER) * FLARE_R_PEAK;
                    float g = Mathf.Pow(intensity, FLARE_G_POWER) * FLARE_G_PEAK;
                    float b = FLARE_B_BASE + FLARE_B_RANGE * intensity;
                    mMovingBgSprites[i].color = new Color(r, g, b, finalAlpha);

                    // Scale: grow from min→max during pulse, snap back when invisible
                    float scaleMin = i < 5 ? FLARE_SCALE_A_MIN : FLARE_SCALE_B_MIN;
                    float scaleMax = i < 5 ? FLARE_SCALE_A_MAX : FLARE_SCALE_B_MAX;
                    float scale = tInCycle < pulse.width
                        ? Mathf.Lerp(scaleMin, scaleMax, tInCycle / pulse.width)
                        : scaleMin;
                    float signY = mFlareScaleSignY[i];
                    mMovingBgSprites[i].transform.localScale = new Vector3(scale, scale * signY, 1f);
                }

                // Core: constant glow, no pulsing — just tracks master alpha
                if (mFlareCoreTexture != null)
                    mFlareCoreTexture.color = new Color(0.85f, 0.92f, 1f, mFlareMasterAlpha * 0.8f);

                yield return null;
            }
        }


        private static void CreateDescElements(GameObject interloperDisplay)
        {
            Transform descTransform = interloperDisplay.transform.Find("Desc_Interloper");
            if (descTransform == null)
            {
                return;
            }

            Camera nguiCam = UICamera.mainCamera;
            if (nguiCam == null)
                nguiCam = Camera.main;

            UIRoot uiRoot = interloperDisplay.GetComponentInParent<UIRoot>();
            float pixelSize = uiRoot != null ? (float)Screen.height / uiRoot.activeHeight : 1f;

            UISprite srcSprite = FindChildComponent<UISprite>(descTransform, "SpriteInterloper");
            UILabel srcName = FindChildComponent<UILabel>(descTransform, "Label_NameInterloper");
            UILabel srcBullets = FindChildComponent<UILabel>(descTransform, "Label_BulletsInterloper");

            Font gameFont = srcName != null ? srcName.trueTypeFont : null;
            if (gameFont == null && srcBullets != null)
                gameFont = srcBullets.trueTypeFont;

            TMP_FontAsset tmpFont = null;
            if (gameFont != null)
            {
                tmpFont = TMP_FontAsset.CreateFontAsset(gameFont);
            }

            // Compute text anchors from the original sprite position BEFORE icon scaling
            float iconCenterX, iconBottomY;
            if (srcSprite != null)
            {
                Vector3 viewportCenter = nguiCam.WorldToViewportPoint(srcSprite.transform.position);
                float vw = srcSprite.width * pixelSize / Screen.width;
                float vh = srcSprite.height * pixelSize / Screen.height;
                iconCenterX = viewportCenter.x;
                iconBottomY = viewportCenter.y - vh / 2f;
            }
            else
            {
                iconCenterX = 0.735f;
                iconBottomY = 0.73f;
            }

            if (srcSprite != null)
            {
                mIconImage = CreateSpriteIcon(srcSprite, nguiCam, pixelSize);
                mIconImage.rectTransform.anchoredPosition = new Vector2(-30f, 25f); //offet to line up with other icons after poor calculation attempt
            }

            const int NAME_FONT_SIZE = 28;
            const int BULLETS_FONT_SIZE = 18;
            float textLeft = iconCenterX - 0.025f; 
            float textRight = textLeft + 0.23f;
            float nameTop = iconBottomY - 0.02f;
            float nameHeight = 0.04f;
            float bulletsTop = nameTop - nameHeight - 0.007f; 
            float bulletsHeight = 0.16f;

            if (srcName != null && tmpFont != null)
            {
                mNameText = CreateTextElement("TrespasserName", "TRESPASSER",
                    tmpFont, NAME_FONT_SIZE, FontStyles.Normal,
                    NAME_COLOR, TextAlignmentOptions.TopLeft);
                mNameText.characterSpacing = 4f;

                RectTransform nameRect = mNameText.gameObject.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(textLeft, nameTop - nameHeight);
                nameRect.anchorMax = new Vector2(textRight, nameTop);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;
                nameRect.anchoredPosition = new Vector2(0f, 10f); //annoying offset until i can get better with NGUI
            }

            if (srcBullets != null && tmpFont != null)
            {
                mBulletsText = CreateTextElement("TrespasserBullets",
                    "\u2022  A stepping stone. Dip your toes in the freezing lake.\n" +
                    "\u2022  Rare chance for Interloper-banned items.\n" +
                    "\u2022  Tuned to challenge without Interloper's\n" +
                    "    desolation or Stalker's plenty.",
                    tmpFont, BULLETS_FONT_SIZE, FontStyles.Normal,
                    BULLETS_COLOR, TextAlignmentOptions.TopLeft);
                mBulletsText.characterSpacing = 8f;
                mBulletsText.lineSpacing = 4f;

                RectTransform bulletsRect = mBulletsText.gameObject.GetComponent<RectTransform>();
                bulletsRect.anchorMin = new Vector2(textLeft, bulletsTop - bulletsHeight);
                bulletsRect.anchorMax = new Vector2(textRight, bulletsTop);
                bulletsRect.offsetMin = Vector2.zero;
                bulletsRect.offsetMax = Vector2.zero;
            }
        }


        private static RawImage CreateSpriteIcon(UISprite srcSprite, Camera nguiCam, float pixelSize)
        {
            Texture2D iconTex = LoadEmbeddedTexture("trespasser_bw_icon.png");
            if (iconTex == null)
                return null;

            GameObject iconObj = new("TrespasserIcon");
            iconObj.transform.SetParent(mCanvasObject.transform, false);

            RawImage iconImage = iconObj.AddComponent<RawImage>();
            iconImage.texture = iconTex;
            iconImage.color = new Color(ICON_COLOR.r, ICON_COLOR.g, ICON_COLOR.b, 0f);

            RectTransform rect = iconObj.GetComponent<RectTransform>();
            PositionFromNguiWidget(rect, srcSprite, nguiCam, pixelSize);

            // Stretch 50% larger from upper-left corner (expand right and down)
            Vector2 min = rect.anchorMin;
            Vector2 max = rect.anchorMax;
            float w = max.x - min.x;
            float h = max.y - min.y;
            rect.anchorMax = new Vector2(max.x + w * 0.5f, max.y);
            rect.anchorMin = new Vector2(min.x, min.y - h * 0.5f);

            return iconImage;
        }


        private static TextMeshProUGUI CreateTextElement(string name, string content,
            TMP_FontAsset font, int fontSize, FontStyles fontStyle,
            Color color, TextAlignmentOptions alignment)
        {
            GameObject textObj = new(name);
            textObj.transform.SetParent(mCanvasObject.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = new Color(color.r, color.g, color.b, 0f);
            text.alignment = alignment;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;

            return text;
        }


        private static void PositionFromNguiWidget(RectTransform rect, UIWidget srcWidget, Camera nguiCam, float pixelSize)
        {
            Vector3 worldCenter = srcWidget.transform.position;
            Vector3 viewportCenter = nguiCam.WorldToViewportPoint(worldCenter);

            float screenWidth = srcWidget.width * pixelSize;
            float screenHeight = srcWidget.height * pixelSize;

            float viewportWidth = screenWidth / Screen.width;
            float viewportHeight = screenHeight / Screen.height;

            rect.anchorMin = new Vector2(
                viewportCenter.x - viewportWidth / 2f,
                viewportCenter.y - viewportHeight / 2f);
            rect.anchorMax = new Vector2(
                viewportCenter.x + viewportWidth / 2f,
                viewportCenter.y + viewportHeight / 2f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static T FindChildComponent<T>(Transform parent, string childName) where T : Component
        {
            Transform child = parent.Find(childName);
            if (child == null)
                return null;
            return child.GetComponent<T>();
        }


        private static UITexture FindNamedTexture(GameObject display, string childName)
        {
            Transform child = display.transform.Find(childName);
            if (child == null)
                return null;
            return child.GetComponent<UITexture>();
        }


        private static void CreateCanvas()
        {
            mCanvasObject = new GameObject("TrespasserOverlayCanvas");
            mCanvasObject.hideFlags = HideFlags.HideAndDontSave;

            Canvas canvas = mCanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = mCanvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }


        private static void CreateImage(UITexture sourceTexture, GameObject sourceDisplay,
            out RawImage image, out RectTransform rect, string name)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(mCanvasObject.transform, false);

            image = imageObject.AddComponent<RawImage>();
            image.texture = sourceTexture.mainTexture;
            image.color = new Color(1f, 1f, 1f, 0f);

            rect = imageObject.GetComponent<RectTransform>();
            PositionFromNgui(rect, sourceTexture, sourceDisplay);
        }


        private static Texture2D LoadEmbeddedTexture(string filename)
        {
            Assembly assembly = typeof(TrespasserOverlay).Assembly;
            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(filename, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                return null;

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Dispose();

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, bytes))
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }
            return texture;
        }


        private static void CreateForegroundGroup(UITexture stalkerFgTexture, GameObject stalkerDisplay)
        {
            Texture2D mackTex = LoadEmbeddedTexture("trespasser_mackenzie.png");
            Texture2D wolfTopTex = LoadEmbeddedTexture("trespasser_wolftop.png");
            Texture2D wolfBotTex = LoadEmbeddedTexture("trespasser_wolfbottom.png");
            if (mackTex == null || wolfTopTex == null || wolfBotTex == null)
                return;

            Camera nguiCam = UICamera.mainCamera;
            if (nguiCam == null)
                nguiCam = Camera.main;

            Vector3 stalkerViewport = nguiCam.WorldToViewportPoint(stalkerFgTexture.transform.position);
            mFgCenter = new Vector2(stalkerViewport.x, stalkerViewport.y);

            float mackPixelH = mackTex.height * MACKENZIE_SCALE;
            float mackViewportH = mackPixelH / Screen.height;
            float mackViewportW = (mackTex.width * MACKENZIE_SCALE) / Screen.width;

            float wolfTopPixelH = wolfTopTex.height * WOLF_SCALE;
            float wolfBotPixelH = wolfBotTex.height * WOLF_SCALE;
            mWolfTopSize = new Vector2(
                (wolfTopTex.width * WOLF_SCALE) / Screen.width,
                wolfTopPixelH / Screen.height);
            mWolfBotSize = new Vector2(
                (wolfBotTex.width * WOLF_SCALE) / Screen.width,
                wolfBotPixelH / Screen.height);

            mFgGroupObject = new GameObject("TrespasserFgGroup");
            mFgGroupObject.transform.SetParent(mCanvasObject.transform, false);
            RectTransform groupRect = mFgGroupObject.AddComponent<RectTransform>();
            groupRect.anchorMin = Vector2.zero;
            groupRect.anchorMax = Vector2.one;
            groupRect.offsetMin = Vector2.zero;
            groupRect.offsetMax = Vector2.zero;

            mFgGroup = mFgGroupObject.AddComponent<CanvasGroup>();
            mFgGroup.alpha = 1f;

            CreateFgChild("TrespasserWolfTop", wolfTopTex, out mWolfTopImage, out mWolfTopRect);

            CreateFgChild("TrespasserFg", mackTex, out mFgImage, out mFgRect);
            mFgRect.anchorMin = new Vector2(mFgCenter.x - mackViewportW / 2f, mFgCenter.y - mackViewportH / 2f);
            mFgRect.anchorMax = new Vector2(mFgCenter.x + mackViewportW / 2f, mFgCenter.y + mackViewportH / 2f);
            mFgRect.offsetMin = Vector2.zero;
            mFgRect.offsetMax = Vector2.zero;

            CreateFgChild("TrespasserWolfBottom", wolfBotTex, out mWolfBottomImage, out mWolfBottomRect);

            SetWolfOrbitalState(mWolfTopRect, WolfStartAngle(WOLF_FINAL_POSITION), WOLF_FINAL_POSITION, mWolfTopSize);
            SetWolfOrbitalState(mWolfBottomRect, WolfStartAngle(WOLF_FINAL_POSITION + 180f), WOLF_FINAL_POSITION + 180f, mWolfBotSize);
        }


        private static void CreateFgChild(string name, Texture2D texture,
            out RawImage image, out RectTransform rect)
        {
            GameObject obj = new(name);
            obj.transform.SetParent(mFgGroupObject.transform, false);
            image = obj.AddComponent<RawImage>();
            image.texture = texture;
            image.color = new Color(1f, 1f, 1f, 0f);
            rect = obj.GetComponent<RectTransform>();
        }


        private static float WolfStartAngle(float homeAngle)
        {
            float sign = mOrbitDirection == OrbitDirection.Clockwise ? -1f : 1f;
            return homeAngle + sign * WOLF_SWEEP_DEGREES;
        }


        private static void SetWolfOrbitalState(RectTransform rect, float angle, float homeAngle, Vector2 size)
        {
            float rad = angle * Mathf.Deg2Rad;
            float x = mFgCenter.x + (WOLF_ORBIT_PX * Mathf.Sin(rad)) / Screen.width;
            float y = mFgCenter.y + (WOLF_ORBIT_PX * Mathf.Cos(rad)) / Screen.height;

            rect.anchorMin = new Vector2(x - size.x / 2f, y - size.y / 2f);
            rect.anchorMax = new Vector2(x + size.x / 2f, y + size.y / 2f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localEulerAngles = new Vector3(0f, 0f, -(angle - homeAngle));
        }


        private static void PositionFromNgui(RectTransform rect, UITexture sourceTexture, GameObject sourceDisplay)
        {
            Camera nguiCam = UICamera.mainCamera;
            if (nguiCam == null)
                nguiCam = Camera.main;

            Vector3 worldCenter = sourceTexture.transform.position;
            Vector3 viewportCenter = nguiCam.WorldToViewportPoint(worldCenter);

            UIRoot root = sourceDisplay.GetComponentInParent<UIRoot>();
            float pixelSize = root != null ? (float)Screen.height / root.activeHeight : 1f;
            float texScreenWidth = sourceTexture.width * pixelSize;
            float texScreenHeight = sourceTexture.height * pixelSize;

            float viewportWidth = texScreenWidth / Screen.width;
            float viewportHeight = texScreenHeight / Screen.height;

            rect.anchorMin = new Vector2(
                viewportCenter.x - viewportWidth / 2f,
                viewportCenter.y - viewportHeight / 2f);
            rect.anchorMax = new Vector2(
                viewportCenter.x + viewportWidth / 2f,
                viewportCenter.y + viewportHeight / 2f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }


        private static void ApplyHidden()
        {
            if (mBgImage != null)
                mBgImage.color = new Color(1f, 1f, 1f, 0f);
            if (mBgRect != null)
                mBgRect.localScale = Vector3.one * BG_SCALE_MIN;
            if (mFgGroup != null)
                mFgGroup.alpha = 0f;
            if (mFgImage != null)
                mFgImage.color = new Color(1f, 1f, 1f, 0f);
            if (mWolfTopImage != null)
                mWolfTopImage.color = new Color(1f, 1f, 1f, 0f);
            if (mWolfBottomImage != null)
                mWolfBottomImage.color = new Color(1f, 1f, 1f, 0f);
            if (mWolfTopRect != null)
                SetWolfOrbitalState(mWolfTopRect, WolfStartAngle(WOLF_FINAL_POSITION), WOLF_FINAL_POSITION, mWolfTopSize);
            if (mWolfBottomRect != null)
                SetWolfOrbitalState(mWolfBottomRect, WolfStartAngle(WOLF_FINAL_POSITION + 180f), WOLF_FINAL_POSITION + 180f, mWolfBotSize);
            mFlareMasterAlpha = 0f;
            StopFlareAnimation();
            if (mMovingBgClone != null)
                mMovingBgClone.SetActive(false);
            SetDescAlpha(0f);
        }


        private static void SetImageAlpha(RawImage image, Color baseColor, float alpha)
        {
            if (image != null)
                image.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }


        private static void SetTextAlpha(TextMeshProUGUI text, Color baseColor, float alpha)
        {
            if (text != null)
                text.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }


        private static void SetDescAlpha(float alpha)
        {
            SetImageAlpha(mIconImage, ICON_COLOR, alpha);
            SetTextAlpha(mNameText, NAME_COLOR, alpha);
            SetTextAlpha(mBulletsText, BULLETS_COLOR, alpha);
        }


        private static float DescElementAlpha(float elapsed, float delay, float fadeDuration)
        {
            float raw = Mathf.Clamp01((elapsed - delay) / fadeDuration);
            return Mathf.SmoothStep(0f, 1f, raw);
        }




        private static IEnumerator FadeInCoroutine(float duration, Action onComplete)
        {
            mFgGroup.alpha = 1f;
            bool flareActivated = false;
            float elapsed = 0f;
            float wolfTopFade = WOLF_TOTAL_DURATION - WOLF_TOP_DELAY;
            float wolfBotFade = WOLF_TOTAL_DURATION - WOLF_BOTTOM_DELAY;
            float totalDuration = Mathf.Max(
                Mathf.Max(duration, BULLETS_DELAY + BULLETS_FADE),
                WOLF_TOTAL_DURATION);

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                float imgT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                mBgImage.color = new Color(1f, 1f, 1f, imgT * BG_ALPHA_MAX);
                mBgRect.localScale = Vector3.one * Mathf.Lerp(BG_SCALE_MIN, BG_SCALE_MAX, imgT);
                mFgImage.color = new Color(1f, 1f, 1f, imgT);

                // Flare activates after FLARE_DELAY, fades in over duration
                if (!flareActivated && elapsed >= FLARE_DELAY && mMovingBgClone != null)
                {
                    mMovingBgClone.SetActive(true);
                    StartFlareAnimation();
                    flareActivated = true;
                }
                mFlareMasterAlpha = DescElementAlpha(elapsed, FLARE_DELAY, duration);

                float moveT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / WOLF_MOVE_DURATION));
                float topAngle = Mathf.Lerp(WolfStartAngle(WOLF_FINAL_POSITION), WOLF_FINAL_POSITION, moveT);
                float botAngle = Mathf.Lerp(WolfStartAngle(WOLF_FINAL_POSITION + 180f), WOLF_FINAL_POSITION + 180f, moveT);
                SetWolfOrbitalState(mWolfTopRect, topAngle, WOLF_FINAL_POSITION, mWolfTopSize);
                SetWolfOrbitalState(mWolfBottomRect, botAngle, WOLF_FINAL_POSITION + 180f, mWolfBotSize);

                float wolfTopAlpha = DescElementAlpha(elapsed, WOLF_TOP_DELAY, wolfTopFade);
                mWolfTopImage.color = new Color(1f, 1f, 1f, wolfTopAlpha);
                float wolfBotAlpha = DescElementAlpha(elapsed, WOLF_BOTTOM_DELAY, wolfBotFade);
                mWolfBottomImage.color = new Color(1f, 1f, 1f, wolfBotAlpha);

                float iconAlpha = DescElementAlpha(elapsed, ICON_DELAY, ICON_FADE);
                float nameAlpha = DescElementAlpha(elapsed, NAME_DELAY, NAME_FADE);
                float bulletsAlpha = DescElementAlpha(elapsed, BULLETS_DELAY, BULLETS_FADE);

                SetImageAlpha(mIconImage, ICON_COLOR, iconAlpha);
                SetTextAlpha(mNameText, NAME_COLOR, nameAlpha);
                SetTextAlpha(mBulletsText, BULLETS_COLOR, bulletsAlpha);

                yield return null;
            }

            mBgImage.color = new Color(1f, 1f, 1f, BG_ALPHA_MAX);
            mBgRect.localScale = Vector3.one * BG_SCALE_MAX;
            mFgImage.color = new Color(1f, 1f, 1f, 1f);
            mWolfTopImage.color = new Color(1f, 1f, 1f, 1f);
            mWolfBottomImage.color = new Color(1f, 1f, 1f, 1f);
            SetWolfOrbitalState(mWolfTopRect, WOLF_FINAL_POSITION, WOLF_FINAL_POSITION, mWolfTopSize);
            SetWolfOrbitalState(mWolfBottomRect, WOLF_FINAL_POSITION + 180f, WOLF_FINAL_POSITION + 180f, mWolfBotSize);
            mFlareMasterAlpha = 1f;
            SetDescAlpha(1f);
            onComplete?.Invoke();
        }


        private static IEnumerator FadeOutCoroutine(float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                float fadeAlpha = Mathf.Lerp(1f, 0f, t);
                mFgGroup.alpha = fadeAlpha;
                mBgImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(BG_ALPHA_MAX, 0f, t));
                mBgRect.localScale = Vector3.one * BG_SCALE_MAX;
                mFlareMasterAlpha = fadeAlpha;
                SetDescAlpha(fadeAlpha);

                yield return null;
            }

            mFgGroup.alpha = 0f;
            mBgImage.color = new Color(1f, 1f, 1f, 0f);
            mBgRect.localScale = Vector3.one * BG_SCALE_MAX;
            mFlareMasterAlpha = 0f;
            StopFlareAnimation();
            if (mMovingBgClone != null)
                mMovingBgClone.SetActive(false);
            SetDescAlpha(0f);
            onComplete?.Invoke();
        }


        private static void StopActiveCoroutine()
        {
            if (mActiveCoroutine != null)
            {
                MelonCoroutines.Stop(mActiveCoroutine);
                mActiveCoroutine = null;
            }
        }
    }
}
