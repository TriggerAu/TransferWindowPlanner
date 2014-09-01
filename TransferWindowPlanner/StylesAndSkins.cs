using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    internal class Styles
    {
        #region Styles
        internal static GUIStyle styleButton;
        private static GUIStyle styleButtonMain;
        private static GUIStyle styleButtonSettings;

        private static GUIStyle styleButtonUnity;
        private static GUIStyle styleButtonMainUnity;
        private static GUIStyle styleButtonSettingsUnity;

        internal static GUIStyle styleDropDownButton;
        internal static GUIStyle styleDropDownButtonUnity;

        internal static GUIStyle styleDropDownListBox;
        internal static GUIStyle styleDropDownListBoxUnity;

        internal static GUIStyle styleDropDownListItem;

        private static GUIStyle stylePanel;

        //internal static GUIStyle styleAlarmButton;

        internal static GUIStyle styleBarName;
        internal static GUIStyle styleBarDef;

        internal static GUIStyle styleBarBlue;
        internal static GUIStyle styleBarBlue_Back;
        internal static GUIStyle styleBarBlue_Thin;

        internal static GUIStyle stylePlotLegendImage;
        internal static GUIStyle stylePlotText,stylePlotLegendText;
        internal static GUIStyle stylePlotXText, stylePlotYText, stylePlotXLabel, stylePlotYLabel;

        internal static GUIStyle stylePlotCrossHair;

        internal static GUIStyle stylePlotTransferMarker, stylePlotTransferMarkerXAxis, stylePlotTransferMarkerYAxis, stylePlotTransferMarkerDV;

        internal static GUIStyle styleSettingsArea;

        internal static GUIStyle styleText, styleTextHeading;
        internal static GUIStyle styleTextDetails, styleTextDetailsLabel;

        internal static GUIStyle styleTextCenter;
        internal static GUIStyle styleTextCenterGreen;
        internal static GUIStyle styleTextGreen;
        internal static GUIStyle styleTextYellow;
        internal static GUIStyle styleTextYellowBold;

        internal static GUIStyle styleTextFieldDefault;
        internal static GUIStyle styleTextField;
        internal static GUIStyle styleTextFieldLabel;
        internal static GUIStyle styleTextFieldError;

        //internal static GUIStyle styleStageText;
        //internal static GUIStyle styleStageTextHead;
        ////internal static GUIStyle styleStageButton;

        //internal static GUIStyle stylePartWindowPanel;
        //internal static GUIStyle stylePartWindowPanelUnity;
        //internal static GUIStyle stylePartWindowHead;

        internal static GUIStyle styleTooltipStyle;

        internal static GUIStyle styleToggle;

        //internal static GUIStyle styleSettingsArea;
        //internal static GUIStyle styleResourceSettingsArea;

        internal static GUIStyle styleDropDownGlyph;

        internal static GUIStyle styleSeparatorV;
        internal static GUIStyle styleSeparatorH;

        /// <summary>
        /// This one sets up the styles we use
        /// </summary>
        internal static void InitStyles()
        {
            MonoBehaviourExtended.LogFormatted("Configuring Styles");

            #region Styles For Skins
            stylePanel = new GUIStyle();
            stylePanel.normal.background = Resources.texPanel;
            stylePanel.border = new RectOffset(6, 6, 6, 6);
            stylePanel.padding = new RectOffset(8, 3, 7, 0);

            styleButton = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
            styleButton.name = "ButtonGeneral";
            styleButton.normal.background = SkinsLibrary.DefKSPSkin.button.normal.background;
            styleButton.hover.background = SkinsLibrary.DefKSPSkin.button.hover.background;
            styleButton.normal.textColor = new Color(207, 207, 207);
            styleButton.fontStyle = FontStyle.Normal;
            styleButton.fixedHeight = 20;
            styleButton.padding.top = 2;
            //styleButton.alignment = TextAnchor.MiddleCenter;

            styleButtonUnity = new GUIStyle(styleButton);
            styleButtonUnity.normal.background = SkinsLibrary.DefUnitySkin.button.normal.background;
            styleButtonUnity.hover.background = SkinsLibrary.DefUnitySkin.button.hover.background;


            styleButtonMain = new GUIStyle(styleButton);
            styleButtonMain.name = "ButtonMain";
            styleButtonMain.fixedHeight = 20;

            styleButtonMainUnity = new GUIStyle(styleButtonMain);
            styleButtonMainUnity.normal.background = SkinsLibrary.DefUnitySkin.button.normal.background;
            styleButtonMainUnity.hover.background = SkinsLibrary.DefUnitySkin.button.hover.background;


            styleButtonSettings = new GUIStyle(styleButton);
            styleButtonSettings.name = "ButtonSettings";
            styleButtonSettings.padding = new RectOffset(1, 1, 1, 1);
            styleButtonSettings.onNormal.background = styleButtonSettings.active.background;
            //styleButtonSettings.fixedWidth = 40;

            styleButtonSettingsUnity = new GUIStyle(styleButtonSettings);
            styleButtonSettingsUnity.normal.background = SkinsLibrary.DefUnitySkin.button.normal.background;
            styleButtonSettingsUnity.hover.background = SkinsLibrary.DefUnitySkin.button.hover.background;
            styleButtonSettingsUnity.onNormal.background = SkinsLibrary.DefUnitySkin.button.active.background;

            styleTooltipStyle = new GUIStyle();
            styleTooltipStyle.name = "Tooltip";
            styleTooltipStyle.fontSize = 12;
            styleTooltipStyle.normal.textColor = new Color32(207, 207, 207, 255);
            styleTooltipStyle.stretchHeight = true;
            styleTooltipStyle.wordWrap = true;
            styleTooltipStyle.normal.background = Resources.texBox;
            //Extra border to prevent bleed of color - actual border is only 1 pixel wide
            styleTooltipStyle.border = new RectOffset(3, 3, 3, 3);
            styleTooltipStyle.padding = new RectOffset(4, 4, 6, 4);
            styleTooltipStyle.alignment = TextAnchor.MiddleCenter;

            styleDropDownButton = new GUIStyle(styleButton);
            styleDropDownButton.padding.right = 20;
            styleDropDownButtonUnity = new GUIStyle(styleButtonUnity);
            styleDropDownButtonUnity.padding.right = 20;

            styleDropDownListBox = new GUIStyle();
            styleDropDownListBox.normal.background = Resources.texBox;
            //Extra border to prevent bleed of color - actual border is only 1 pixel wide
            styleDropDownListBox.border = new RectOffset(3, 3, 3, 3);

            styleDropDownListBoxUnity = new GUIStyle();
            styleDropDownListBoxUnity.normal.background = Resources.texBoxUnity;
            //Extra border to prevent bleed of color - actual border is only 1 pixel wide
            styleDropDownListBoxUnity.border = new RectOffset(3, 3, 3, 3);


            styleDropDownListItem = new GUIStyle();
            styleDropDownListItem.normal.textColor = new Color(207, 207, 207);
            Texture2D texBack = Styles.CreateColorPixel(new Color(207, 207, 207));
            styleDropDownListItem.hover.background = texBack;
            styleDropDownListItem.onHover.background = texBack;
            styleDropDownListItem.hover.textColor = Color.black;
            styleDropDownListItem.onHover.textColor = Color.black;
            styleDropDownListItem.padding = new RectOffset(4, 4, 3, 4);

            #endregion

            #region Common Styles

            styleBarName = new GUIStyle() { fixedHeight = 16, fixedWidth = 32 };
            styleBarName.normal.textColor = Color.white;
            //styleBarName.alignment = TextAnchor.MiddleCenter;

            //styleBarDef = new GUIStyle(GUI.skin.box);
            styleBarDef = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
            styleBarDef.border = new RectOffset(2, 2, 2, 2);
            styleBarDef.normal.textColor = Color.white;
            styleBarDef.fixedHeight = 15;
            styleBarDef.alignment = TextAnchor.UpperCenter;

            styleBarBlue = new GUIStyle(styleBarDef);
            styleBarBlue.normal.background = Resources.texBarBlue;
            styleBarBlue_Back = new GUIStyle(styleBarDef);
            styleBarBlue_Back.normal.background = Resources.texBarBlue_Back;
            styleBarBlue_Thin = new GUIStyle(styleBarBlue);
            styleBarBlue_Thin.border = new RectOffset(0, 0, 0, 0);

            styleText = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
            styleText.fontSize = 12;
            styleText.alignment = TextAnchor.MiddleLeft;
            styleText.normal.textColor = new Color(207, 207, 207);
            styleText.wordWrap = false;

            styleTextHeading = new GUIStyle(styleText);
            styleTextHeading.fontStyle = FontStyle.Bold;
            styleTextHeading.wordWrap = false;

            styleTextDetails = new GUIStyle(styleText);
            styleTextDetails.padding.top = 2;

            styleTextDetailsLabel = new GUIStyle(styleTextDetails);
            styleTextDetailsLabel.alignment = TextAnchor.MiddleRight;
            styleTextDetailsLabel.fontStyle = FontStyle.Bold;

            styleTextGreen = new GUIStyle(styleTextDetails);
            styleTextGreen.normal.textColor = new Color32(183, 254, 0, 255); ;
            styleTextYellow = new GUIStyle(styleTextDetails);
            styleTextYellow.normal.textColor = Color.yellow;
            styleTextYellowBold = new GUIStyle(styleTextYellow);
            styleTextYellowBold.fontStyle = FontStyle.Bold;

            styleTextCenter = new GUIStyle(styleTextDetails);
            styleTextCenter.alignment = TextAnchor.MiddleCenter;
            styleTextCenterGreen = new GUIStyle(styleTextCenter);
            styleTextCenterGreen.normal.textColor = new Color32(183, 254, 0, 255);

            #region TextField Stuff for entry fields
            styleTextFieldDefault = new GUIStyle(SkinsLibrary.DefUnitySkin.textField);
            styleTextFieldDefault.fontStyle = FontStyle.Normal;

            styleTextField = new GUIStyle(styleTextFieldDefault);
            styleTextField.stretchWidth = true;
            styleTextField.alignment = TextAnchor.UpperLeft;
            styleTextField.normal.textColor = Color.yellow;

            styleTextFieldLabel = new GUIStyle(styleText);
            styleTextFieldLabel.alignment = TextAnchor.MiddleRight;
            styleTextFieldLabel.fontStyle = FontStyle.Bold;
            styleTextFieldLabel.padding.top = 4;

            styleTextFieldError = new GUIStyle(styleTextField);
            styleTextFieldError.normal.textColor = Color.red; 
            #endregion

            stylePlotLegendImage = new GUIStyle();
            stylePlotText = new GUIStyle(styleText);
            stylePlotText.fontSize = 10;
            stylePlotText.alignment = TextAnchor.MiddleCenter;
            stylePlotLegendText = new GUIStyle(stylePlotText);
            stylePlotLegendText.alignment = TextAnchor.MiddleLeft;
            stylePlotXText = new GUIStyle(stylePlotText);
            stylePlotYText = new GUIStyle(stylePlotText);
            stylePlotYText.alignment = TextAnchor.MiddleRight;
            stylePlotXLabel = new GUIStyle(stylePlotText);
            stylePlotXLabel.fontStyle = FontStyle.BoldAndItalic;
            stylePlotYLabel = new GUIStyle(stylePlotText);
            stylePlotYLabel.fontStyle = FontStyle.BoldAndItalic;

            stylePlotCrossHair = new GUIStyle();
            stylePlotCrossHair.normal.background = CreateColorPixel(new Color32(255, 25, 255, 192));

            stylePlotTransferMarkerDV = new GUIStyle();
            stylePlotTransferMarkerDV.normal.background = Resources.texSelectedDV;
            stylePlotTransferMarkerDV.border = new RectOffset(8, 8, 3, 3);


            styleSettingsArea = new GUIStyle(HighLogic.Skin.textArea);
            styleSettingsArea.padding = new RectOffset(0, 0, 0, 4);

            styleToggle = new GUIStyle(HighLogic.Skin.toggle);
            styleToggle.normal.textColor = new Color(207, 207, 207);
            styleToggle.fixedHeight = 20;
            styleToggle.padding = new RectOffset(6, 0, -2, 0);

            styleDropDownGlyph = new GUIStyle();
            styleDropDownGlyph.alignment = TextAnchor.MiddleCenter;

            styleSeparatorV = new GUIStyle();
            styleSeparatorV.normal.background = Resources.texSeparatorV;
            styleSeparatorV.border = new RectOffset(0, 0, 6, 6);
            styleSeparatorV.fixedWidth = 2;

            styleSeparatorH = new GUIStyle();
            styleSeparatorH.normal.background = Resources.texSeparatorH;
            styleSeparatorH.border = new RectOffset(6, 6, 0, 0);
            styleSeparatorH.fixedHeight = 2;
            #endregion
        }
        #endregion


        /// <summary>
        /// This one creates the skins, adds em to the skins library and adds needed styles
        /// </summary>
        internal static void InitSkins()
        {
            //Default Skin
            GUISkin DefKSP = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.KSP);
            //DefKSP.window = stylePanel;
            DefKSP.font = SkinsLibrary.DefUnitySkin.font;
            //DefKSP.window.padding = new RectOffset(8, 3, 27, 0);
            //DefKSP.horizontalSlider.margin.top = 8;
            SkinsLibrary.AddSkin("Default", DefKSP);

            //Adjust Default Skins
            SkinsLibrary.List["Default"].button = new GUIStyle(styleButton);
            SkinsLibrary.List["Default"].label = new GUIStyle(styleText);

            //Add Styles once skin is added
            SkinsLibrary.AddStyle("Default", styleTooltipStyle);
            SkinsLibrary.AddStyle("Default", styleButton);
            SkinsLibrary.AddStyle("Default", styleButtonMain);
            SkinsLibrary.AddStyle("Default", styleButtonSettings);
            SkinsLibrary.AddStyle("Default", "DropDownButton", styleDropDownButton);
            SkinsLibrary.AddStyle("Default", "DropDownListBox", styleDropDownListBox);
            SkinsLibrary.AddStyle("Default", "DropDownListItem", styleDropDownListItem);

            //Now a Unity Style one
            GUISkin DefUnity = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.Unity);
            DefUnity.window = DefUnity.box;
            DefUnity.window.border = new RectOffset(6, 6, 6, 6);
            DefUnity.window.padding = new RectOffset(8, 3, 27, 0);
            DefUnity.horizontalSlider.margin.top = 8;
            SkinsLibrary.AddSkin("Unity", DefUnity);

            //Adjust Default Skins
            SkinsLibrary.List["Unity"].button = new GUIStyle(styleButtonUnity);
            SkinsLibrary.List["Unity"].label = new GUIStyle(styleText);

            //Add Styles once skin is added
            GUIStyle styleTooltipUnity = new GUIStyle(styleTooltipStyle);
            styleTooltipUnity.normal.background = GUI.skin.box.normal.background;
            styleTooltipUnity.normal.textColor = Color.white;
            SkinsLibrary.AddStyle("Unity", styleTooltipUnity);
            SkinsLibrary.AddStyle("Unity", styleButtonUnity);
            SkinsLibrary.AddStyle("Unity", styleButtonMainUnity);
            SkinsLibrary.AddStyle("Unity", styleButtonSettingsUnity);
            SkinsLibrary.AddStyle("Unity", "DropDownButton", styleDropDownButtonUnity);
            SkinsLibrary.AddStyle("Unity", "DropDownListBox", styleDropDownListBoxUnity);
            SkinsLibrary.AddStyle("Unity", "DropDownListItem", styleDropDownListItem);

            ////Now a Unity Style one with KSP buttons
            GUISkin UnityWKSPButtons = SkinsLibrary.CopySkin("Unity");
            UnityWKSPButtons.button = DefKSP.button;
            UnityWKSPButtons.toggle = DefKSP.toggle;
            SkinsLibrary.AddSkin("UnityWKSPButtons", UnityWKSPButtons);

            //Adjust Default Skins
            SkinsLibrary.List["UnityWKSPButtons"].button = new GUIStyle(styleButton);
            SkinsLibrary.List["UnityWKSPButtons"].label = new GUIStyle(styleText);

            //Add Styles once skin is added
            SkinsLibrary.AddStyle("UnityWKSPButtons", styleTooltipUnity);
            SkinsLibrary.AddStyle("UnityWKSPButtons", styleButton);
            SkinsLibrary.AddStyle("UnityWKSPButtons", styleButtonMain);
            SkinsLibrary.AddStyle("UnityWKSPButtons", styleButtonSettings);
            SkinsLibrary.AddStyle("UnityWKSPButtons", "DropDownButton", styleDropDownButton);
            SkinsLibrary.AddStyle("UnityWKSPButtons", "DropDownListBox", styleDropDownListBox);
            SkinsLibrary.AddStyle("UnityWKSPButtons", "DropDownListItem", styleDropDownListItem);

        }

        /// <summary>
        /// Creates a 1x1 texture
        /// </summary>
        /// <param name="Background">Color of the texture</param>
        /// <returns></returns>
        internal static Texture2D CreateColorPixel(Color32 Background)
        {
            Texture2D retTex = new Texture2D(1, 1);
            retTex.SetPixel(0, 0, Background);
            retTex.Apply();
            return retTex;
        }
    }
}
