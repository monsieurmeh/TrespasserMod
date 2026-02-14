using System.Collections;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;

namespace Trespasser
{
    internal static class TrespasserOverlay
    {
        private const float BG_ALPHA_MAX = 0.4f;
        private const float BG_SCALE_MIN = 0.9f;
        private const float BG_SCALE_MAX = 1.1f;

        // Cascade timing — absolute values matched from Interloper's native animation
        private const float ICON_DELAY = 0.33f;
        private const float ICON_FADE = 0.27f;
        private const float NAME_DELAY = 0.50f;
        private const float NAME_FADE = 0.17f;
        private const float BULLETS_DELAY = 0.57f;
        private const float BULLETS_FADE = 0.20f;

        private static readonly Color NAME_COLOR = new(0.98f, 0.98f, 0.98f, 1f);
        private static readonly Color BULLETS_COLOR = new(0.78f, 0.78f, 0.78f, 1f);
        private static readonly Color ICON_COLOR = new(0.98f, 0.98f, 0.98f, 1f);

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

        internal static bool IsSelectFading => mIsSelectFading;
        internal static bool IsDeselectFading => mIsDeselectFading;


        internal static void Initialize(Panel_SelectExperience panel)
        {
            Destroy();

            Panel_SelectExperience.XPModeMenuItem stalkerItem = panel.m_MenuItems[2];
            UITexture stalkerFgTexture = FindNamedTexture(stalkerItem.m_Display, "XPStalkerTexture");
            if (stalkerFgTexture == null || stalkerFgTexture.mainTexture == null)
            {
                return;
            }

            Panel_SelectExperience.XPModeMenuItem interloperItem = panel.m_MenuItems[4];
            UITexture interloperBgTexture = FindNamedTexture(interloperItem.m_Display, "InterloperXPTexture_bg");
            if (interloperBgTexture == null || interloperBgTexture.mainTexture == null)
            {
                return;
            }

            Panel_SelectExperience.XPModeMenuItem trespasserItem = panel.m_MenuItems[3];
            DisableAllNguiWidgets(trespasserItem.m_Display);
            CreateCanvas();
            CreateImage(interloperBgTexture, interloperItem.m_Display, out mBgImage, out mBgRect, "TrespasserBg");
            CreateImage(stalkerFgTexture, stalkerItem.m_Display, out mFgImage, out mFgRect, "TrespasserFg");
            CreateDescElements(interloperItem.m_Display);
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
            mIsSelectFading = false;
            mIsDeselectFading = false;

            if (mCanvasObject != null)
            {
                UnityEngine.Object.Destroy(mCanvasObject);
                mCanvasObject = null;
            }

            mBgImage = null;
            mBgRect = null;
            mFgImage = null;
            mFgRect = null;
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

            if (srcSprite != null)
                mIconImage = CreateSpriteIcon(srcSprite, nguiCam, pixelSize);

            RectTransform iconRect = mIconImage != null ?
                mIconImage.gameObject.GetComponent<RectTransform>() : null;

            float iconCenterX, iconBottomY;
            if (iconRect != null)
            {
                iconCenterX = (iconRect.anchorMin.x + iconRect.anchorMax.x) / 2f;
                iconBottomY = iconRect.anchorMin.y;
            }
            else
            {
                iconCenterX = 0.735f;
                iconBottomY = 0.73f;
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
            }

            if (srcBullets != null && tmpFont != null)
            {
                mBulletsText = CreateTextElement("TrespasserBullets",
                    "\u2022  A stepping stone; A dip in the freezing lake.\n" +
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
            UIAtlas atlas = srcSprite.atlas;
            if (atlas == null || atlas.spriteMaterial == null || atlas.spriteMaterial.mainTexture == null)
            {
                return null;
            }

            Texture mainTex = atlas.spriteMaterial.mainTexture;
            UISpriteData spriteData = atlas.GetSprite(srcSprite.spriteName);
            if (spriteData == null)
            {
                return null;
            }
            GameObject iconObj = new("TrespasserIcon");
            iconObj.transform.SetParent(mCanvasObject.transform, false);

            RawImage iconImage = iconObj.AddComponent<RawImage>();
            iconImage.texture = mainTex;
            iconImage.uvRect = new Rect(
                (float)spriteData.x / mainTex.width,
                1f - (float)(spriteData.y + spriteData.height) / mainTex.height,
                (float)spriteData.width / mainTex.width,
                (float)spriteData.height / mainTex.height);
            iconImage.color = new Color(ICON_COLOR.r, ICON_COLOR.g, ICON_COLOR.b, 0f);

            RectTransform rect = iconObj.GetComponent<RectTransform>();
            PositionFromNguiWidget(rect, srcSprite, nguiCam, pixelSize);

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
            if (mFgImage != null)
                mFgImage.color = new Color(1f, 1f, 1f, 0f);
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
            float elapsed = 0f;
            float totalDuration = Mathf.Max(duration, BULLETS_DELAY + BULLETS_FADE);

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                float imgT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                mFgImage.color = new Color(1f, 1f, 1f, imgT);
                mBgImage.color = new Color(1f, 1f, 1f, imgT * BG_ALPHA_MAX);
                mBgRect.localScale = Vector3.one * Mathf.Lerp(BG_SCALE_MIN, BG_SCALE_MAX, imgT);
                float iconAlpha = DescElementAlpha(elapsed, ICON_DELAY, ICON_FADE);
                float nameAlpha = DescElementAlpha(elapsed, NAME_DELAY, NAME_FADE);
                float bulletsAlpha = DescElementAlpha(elapsed, BULLETS_DELAY, BULLETS_FADE);

                SetImageAlpha(mIconImage, ICON_COLOR, iconAlpha);
                SetTextAlpha(mNameText, NAME_COLOR, nameAlpha);
                SetTextAlpha(mBulletsText, BULLETS_COLOR, bulletsAlpha);

                yield return null;
            }

            mFgImage.color = new Color(1f, 1f, 1f, 1f);
            mBgImage.color = new Color(1f, 1f, 1f, BG_ALPHA_MAX);
            mBgRect.localScale = Vector3.one * BG_SCALE_MAX;
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
                mFgImage.color = new Color(1f, 1f, 1f, fadeAlpha);
                mBgImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(BG_ALPHA_MAX, 0f, t));
                mBgRect.localScale = Vector3.one * BG_SCALE_MAX;
                SetDescAlpha(fadeAlpha);

                yield return null;
            }

            mFgImage.color = new Color(1f, 1f, 1f, 0f);
            mBgImage.color = new Color(1f, 1f, 1f, 0f);
            mBgRect.localScale = Vector3.one * BG_SCALE_MAX;
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
