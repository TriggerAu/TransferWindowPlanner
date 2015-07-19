using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace KSPPluginFramework
{
    public abstract class MonoBehaviourWindowPlus : MonoBehaviourWindow
    {

        #region Draw Control Wrappers
        internal static Boolean DrawButton(String text, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text,options);
        }
        internal static Boolean DrawButton(String text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text,style,options);
        }

        internal static Boolean DrawTextBox(ref String strVar, params GUILayoutOption[] options)
        {
            return DrawTextBox(ref strVar,SkinsLibrary.CurrentSkin.textField,options);
        }
        internal static Boolean DrawTextBox(ref String strVar, GUIStyle style, params GUILayoutOption[] options)
        {
            String strOld = strVar;
            strVar = GUILayout.TextField(strVar, style, options);

            return DrawResultChanged(strOld, strVar, "Text String");
        }

        internal static Boolean DrawTextBox(ref Int32 intVar, params GUILayoutOption[] options)
        {
            return DrawTextBox(ref intVar, SkinsLibrary.CurrentSkin.textField, options);
        }
        internal static Boolean DrawTextBox(ref Int32 intVar, GUIStyle style, params GUILayoutOption[] options)
        {
            String strRef = intVar.ToString();
            DrawTextBox(ref strRef, style, options);
            Int32 intOld = intVar;
            intVar = Convert.ToInt32(strRef);
            return DrawResultChanged(intOld, intVar, "Integer Changed");
        }

        internal static Boolean DrawTextBox(ref Double dblVar, params GUILayoutOption[] options)
        {
            return DrawTextBox(ref dblVar, SkinsLibrary.CurrentSkin.textField, options);
        }
        internal static Boolean DrawTextBox(ref Double dblVar, GUIStyle style, params GUILayoutOption[] options)
        {
            String strRef = dblVar.ToString();
            DrawTextBox(ref strRef, style, options);
            Double dblOld = dblVar;
            dblVar = Convert.ToDouble(strRef);
            return DrawResultChanged(dblOld, dblVar, "Double Changed");
        }


        internal static Boolean DrawHorizontalSlider(ref Int32 intVar, Int32 leftValue, Int32 rightValue, params GUILayoutOption[] options)
        {
            Int32 intOld = intVar;

            intVar = (Int32)GUILayout.HorizontalSlider((Single)intVar, (Single)leftValue, (Single)rightValue, options);
            return DrawResultChanged(intOld, intVar, "Integer HorizSlider");
        }
        internal static Boolean DrawHorizontalSlider(ref Single dblVar, Single leftValue, Single rightValue, params GUILayoutOption[] options)
        {
            Single intOld = dblVar;

            dblVar = GUILayout.HorizontalSlider(dblVar, leftValue, rightValue, options);
            return DrawResultChanged(intOld, dblVar, "Integer HorizSlider");
        }

        /// <summary>
        /// Draws a Toggle Button and sets the boolean variable to the state of the button
        /// </summary>
        /// <param name="blnVar">Boolean variable to set and store result</param>
        /// <param name="ButtonText"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns>True when the button state has changed</returns>
        internal static Boolean DrawToggle(ref Boolean blnVar, String ButtonText, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnOld = blnVar;
            blnVar = GUILayout.Toggle(blnVar, ButtonText, style, options);

            return DrawResultChanged(blnOld, blnVar, "Toggle");
        }

        internal static Boolean DrawToggle(ref Boolean blnVar, Texture image, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnOld = blnVar;
            blnVar = GUILayout.Toggle(blnVar, image, style, options);

            return DrawResultChanged(blnOld, blnVar, "Toggle");
        }

        internal static Boolean DrawToggle(ref Boolean blnVar, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnOld = blnVar;
            blnVar = GUILayout.Toggle(blnVar, content, style, options);

            return DrawResultChanged(blnOld, blnVar, "Toggle");
        }


        internal static void DrawLabel(String Message, params object[] args)
        {
            GUILayout.Label(String.Format(Message, args));
        }


        private static Boolean DrawResultChanged<T>(T Original, T New, String Message) 
        {
            if (Original.Equals(New)) {
                return false;
            } else {
                LogFormatted_DebugOnly("{0} Changed. {1}->{2}", Message, Original.ToString(), New.ToString());
                return true;
            }

        }

        #endregion

        internal DropDownListManager ddlManager = new DropDownListManager();

        internal override void DrawWindowPre(int id)
        {
            ddlManager.DrawBlockingSelectors();
        }
        internal override void DrawWindowPost(int id)
        {
            ddlManager.DrawDropDownLists();
        }
        internal override void OnGUIEvery()
        {
            ddlManager.CloseOnOutsideClicks();
        }

        public class DropDownListManager:List<DropDownList>
        {
            public DropDownListManager()
            {

            }
                       
            internal void AddDDL(DropDownList NewDDL)
            {
                this.Add(NewDDL);
                NewDDL.OnListVisibleChanged += NewDDL_OnListVisibleChanged;
            }

            void NewDDL_OnListVisibleChanged(DropDownList sender, bool VisibleState)
            {
                if (VisibleState)
                {
                    foreach (DropDownList ddlTemp in this)
                    {
                        if (sender != ddlTemp)
                            ddlTemp.ListVisible = false;
                    }
                }
            }

            internal void DrawBlockingSelectors()
            {
                //this is too slow
                //foreach (DropDownList ddlTemp in this.Where(x=>x.ListVisible))
                foreach (DropDownList ddlTemp in this)
                {
                    ddlTemp.DrawBlockingSelector();
                }
            }

            internal void DrawDropDownLists()
            {
                //foreach (DropDownList ddlTemp in this.Where(x=>x.ListVisible))
                foreach (DropDownList ddlTemp in this)
                {
                    ddlTemp.DrawDropDownList();
                }
            }

            internal void CloseOnOutsideClicks()
            {
                //foreach (DropDownList ddlTemp in this.Where(x=>x.ListVisible))
                foreach (DropDownList ddlTemp in this)
                {
                    ddlTemp.CloseOnOutsideClick();
                }
            }

            internal void SetListBoxOffset(Vector2 WrapperOffset)
            {
                foreach (DropDownList ddlTemp in this)
                {
                    ddlTemp.SetListBoxOffset(WrapperOffset);
                }
            }

            internal GUIContentWithStyle DropDownGlyphs {
                set {
                    foreach (DropDownList ddlTemp in this) {
                        ddlTemp.DropDownGlyph = value;
                    }
                }
            }
            internal GUIContentWithStyle DropDownSeparators {
                set {
                    foreach (DropDownList ddlTemp in this) {
                        ddlTemp.DropDownSeparator = value;
                    }
                }
            }
            
        }

        public class DropDownList
        {
            //Constructors
            public DropDownList(IEnumerable<String> Items, Int32 Selected, MonoBehaviourWindow Window)
                : this(Items,Window)
            {
                SelectedIndex = Selected;
            }
            public DropDownList(IEnumerable<String> Items, MonoBehaviourWindow Window)
                : this(Window)
            {
                this.Items = Items.ToList<String>();
            }
            //public DropDownList(Enum Items)
            //    : this()
            //{
            //    this.Items = EnumExtensions.ToEnumDescriptions(Items);
            //}
            public DropDownList(MonoBehaviourWindow Window)
            {
                this.Window = Window;
                //set internal variable so we dont trigger the event before the object exists
                _ListVisible = false;
                SelectedIndex = 0;

                SkinsLibrary.OnSkinChanged += SkinsLibrary_OnSkinChanged;
            }

            //properties to use
            internal List<String> Items { get; set; }
            internal Int32 SelectedIndex { get; set; }
            internal String SelectedValue { get { return Items[SelectedIndex]; } }

            private Boolean _ListVisible;
            internal Boolean ListVisible {
                get { return _ListVisible; }
                set {
                    _ListVisible = value;
                    if (_ListVisible)
                        CalcPagesAndSizes();
                    if (OnListVisibleChanged!=null)
                        OnListVisibleChanged(this, _ListVisible);
                }
            }

            private Rect rectButton;
            private Rect rectListBox;
            /// <summary>
            /// This is for DropDowns inside extra containers like scrollviews - where getlastrect does not cater to the scrollview position 
            /// </summary>
            private Vector2 vectListBoxOffset = new Vector2(0, 0);
            internal void SetListBoxOffset(Vector2 WrapperOffset)
            {
                if (vectListBoxOffset != WrapperOffset)
                {
                    vectListBoxOffset = WrapperOffset;
                    if (_ListVisible)
                     CalcPagesAndSizes();
                }
            }
            
            private GUIStyle stylePager;
            private GUIStyle _styleButton = null;
            internal GUIStyle styleButton { 
                get { return _styleButton; } 
                set {  _styleButton = value; SkinsLibrary_OnSkinChanged(); } }

            private GUIStyle _styleListItem;
            public GUIStyle styleListItem
            {
                get { return _styleListItem; }
                set { _styleListItem = value; SkinsLibrary_OnSkinChanged(); }
            }

            private GUIStyle _styleListBox;
            public GUIStyle styleListBox {
                get { return _styleListBox; }
                set { _styleListBox = value; SkinsLibrary_OnSkinChanged(); }
            }

            internal GUIStyle styleListBlocker = new GUIStyle();

            internal GUIContentWithStyle DropDownSeparator = null;
            internal GUIContentWithStyle DropDownGlyph = null;

            internal Int32 ListItemHeight = 20;
            internal RectOffset ListBoxPadding = new RectOffset(1,1,1,1);

            internal MonoBehaviourWindow Window;
            internal Int32 ListPageLength=0;
            internal Int32 ListPageNum=0;
            internal Boolean ListPageOverflow=false;
            internal Boolean ListPagingRequired = false;


            //event for changes
            public delegate void SelectionChangedEventHandler(DropDownList sender, Int32 OldIndex, Int32 NewIndex);
            public event SelectionChangedEventHandler OnSelectionChanged;
            public delegate void ListVisibleChangedHandler(DropDownList sender, Boolean VisibleState);
            public event ListVisibleChangedHandler OnListVisibleChanged;

            private GUIStyle styleButtonToDraw = null;
            private GUIStyle styleListBoxToDraw = null;
            private GUIStyle styleListItemToDraw = null;

            //Event Handler for SkinChanges
            void SkinsLibrary_OnSkinChanged()
            {
                //check the user provided style
                styleButtonToDraw = CombineSkinStyles(_styleButton, "DropDownButton");
                styleListBoxToDraw = CombineSkinStyles(_styleListBox, "DropDownListBox");
                styleListItemToDraw = CombineSkinStyles(_styleListItem, "DropDownListItem");

            }
            private GUIStyle CombineSkinStyles(GUIStyle UserStyle,String StyleID)
            {
                GUIStyle retStyle;
                if (UserStyle == null)
                {
                    //then look in the skinslibrary
                    if (SkinsLibrary.StyleExists(SkinsLibrary.CurrentSkin, StyleID))
                    {
                        retStyle = SkinsLibrary.GetStyle(SkinsLibrary.CurrentSkin, StyleID);
                    }
                    else
                    {
                        retStyle = null;
                    }
                }
                else
                {
                    retStyle = UserStyle;
                }
                return retStyle;
            }

            //Draw the button behind everything else to catch the first mouse click
            internal void DrawBlockingSelector()
            {
                //do we need to draw the blocker
                if (ListVisible)
                {
                    //This will collect the click event before any other controls under the listrect
                    if (GUI.Button(rectListBox, "", styleListBlocker))
                    {
                        Int32 oldIndex = SelectedIndex;

                        if (!ListPageOverflow)
                            SelectedIndex = (Int32)Math.Floor((Event.current.mousePosition.y - rectListBox.y) / (rectListBox.height / Items.Count));
                        else {
                            //do some maths to work out the actual index - Page Length + 1 for the pager row
                            Int32 SelectedRow = (Int32)Math.Floor((Event.current.mousePosition.y - rectListBox.y) / (rectListBox.height / (ListPageLength+1)));

                            LogFormatted("{0}-{1}-{2}-{3}-{4}", Event.current.mousePosition.y, rectListBox.y, rectListBox.height, ListPageLength, SelectedRow);

                            if (SelectedRow==0) {
                                //this is the paging row...
                                if (Event.current.mousePosition.x > (rectListBox.x + rectListBox.width - 40 - ListBoxPadding.right))
                                    ListPageNum++;
                                else if (Event.current.mousePosition.x > (rectListBox.x + rectListBox.width - 80 - ListBoxPadding.right))
                                    ListPageNum--;
                                if (ListPageNum < 0) ListPageNum = (Int32)Math.Floor((Single)Items.Count / ListPageLength);
                                if (ListPageNum*ListPageLength>Items.Count) ListPageNum = 0;
                                return;
                            }
                            else
                            {
                                SelectedIndex = (ListPageNum * ListPageLength) + (SelectedRow - 1);
                            }
                            if (SelectedIndex >= Items.Count) {
                                SelectedIndex = oldIndex;
                                return;
                            }
                        }
                        //Throw an event or some such from here
                        if ((oldIndex!= SelectedIndex) && (OnSelectionChanged != null))
                            OnSelectionChanged(this,oldIndex, SelectedIndex);
                        ListVisible = false;
                    }
                }
            }

            //Draw the actual button for the list
            internal Boolean DrawButton()
            {
                Boolean blnReturn = false;

                if (styleButtonToDraw == null)
                    blnReturn = GUILayout.Button(SelectedValue);
                else
                    blnReturn = GUILayout.Button(SelectedValue, styleButtonToDraw);

                if (blnReturn) ListVisible = !ListVisible;

                //get the drawn button rectangle
                if (Event.current.type == EventType.repaint)
                    rectButton = GUILayoutUtility.GetLastRect();
                //draw a dropdown symbol on the right edge
                if (DropDownGlyph != null)
                {
                    Rect rectDropIcon = new Rect(rectButton) { x = (rectButton.x + rectButton.width - 20), width = 20 };
                    if (DropDownSeparator != null)
                    {
                        Rect rectDropSep = new Rect(rectDropIcon) { x = (rectDropIcon.x - DropDownSeparator.CalcWidth), width = DropDownSeparator.CalcWidth };
                        if (DropDownSeparator.Style == null){
                            GUI.Box(rectDropSep, DropDownSeparator.Content);
                        } else {
                            GUI.Box(rectDropSep, DropDownSeparator.Content, DropDownSeparator.Style);
                        }
                    }
                    if (DropDownGlyph.Style == null) {
                        GUI.Box(rectDropIcon, DropDownGlyph.Content); 
                    } else {
                        GUI.Box(rectDropIcon, DropDownGlyph.Content, DropDownGlyph.Style);
                    }
                    
                }

                return blnReturn;
            }

            private void CalcPagesAndSizes()
            {
                //raw box size
                rectListBox = new Rect(rectButton) {
                    x = rectButton.x+vectListBoxOffset.x,
                    y = rectButton.y + rectButton.height + vectListBoxOffset.y,
                    height = (Items.Count * ListItemHeight) + (ListBoxPadding.top + ListBoxPadding.bottom)
                };

                //if it doesnt fit below the list
                if ((rectListBox.y+rectListBox.height)>Window.WindowRect.height) {
                    if (rectListBox.height < Window.WindowRect.height - 8)
                    {
                        //move the top up so that the full list is visible
                        ListPageOverflow = false;
                        rectListBox.y = Window.WindowRect.height - rectListBox.height - 4;
                    }
                    else
                    {
                        //Need multipages for this list
                        ListPageOverflow = true;
                        rectListBox.y = 4;
                        rectListBox.height = (Single)(ListItemHeight * Math.Floor((Window.WindowRect.height - 8) / ListItemHeight));
                        ListPageLength = (Int32)(Math.Floor((Window.WindowRect.height - 8) / ListItemHeight) - 1);
                        ListPageNum = (Int32)Math.Floor((Double)SelectedIndex / ListPageLength);
                    }
                } else { 
                    ListPageOverflow = false; 
                }

                stylePager = new GUIStyle(SkinsLibrary.CurrentSkin.label) { fontStyle = FontStyle.Italic };
            }

            //Draw the hovering dropdown
            internal void DrawDropDownList()
            {
                if (ListVisible)
                {
                    if (styleListBoxToDraw == null) styleListBoxToDraw = GUI.skin.box;
                    if (styleListItemToDraw == null) styleListItemToDraw = GUI.skin.label;

                    //and draw it
                    GUI.Box(rectListBox, "", styleListBoxToDraw);

                    Int32 iStart = 0,iEnd = Items.Count,iPad=0;
                    if (ListPageOverflow)
                    {
                        //calc the size of each page
                        iStart = ListPageLength * ListPageNum;
                        
                        if (ListPageLength * (ListPageNum + 1) < Items.Count)
                            iEnd = ListPageLength * (ListPageNum + 1); 

                        //this moves us down a row to fit the paging buttons in the main loop
                        iPad = 1;

                        //Draw paging buttons
                        GUI.Label(new Rect(rectListBox) { x = rectListBox.x + ListBoxPadding.left, height = 20 }, String.Format("Page {0}/{1:0}", ListPageNum + 1, Math.Floor((Single)Items.Count / ListPageLength) + 1), stylePager);
                        GUI.Button(new Rect(rectListBox) { x = rectListBox.x + rectListBox.width - 80 - ListBoxPadding.right, y = rectListBox.y + 2, width = 40, height = 16 }, "Prev");
                        GUI.Button(new Rect(rectListBox) { x = rectListBox.x + rectListBox.width - 40 - ListBoxPadding.right, y = rectListBox.y + 2, width = 40, height = 16 }, "Next");
                    }

                    //now draw each listitem
                    for (int i = iStart; i < iEnd; i++)
                    {
                        Rect ListButtonRect = new Rect(rectListBox) {
                            x = rectListBox.x + ListBoxPadding.left,
                            width = rectListBox.width - ListBoxPadding.left-ListBoxPadding.right,
                            y = rectListBox.y + ((i-iStart+iPad) * ListItemHeight) + ListBoxPadding.top,
                            height = 20
                        };

                        if (GUI.Button(ListButtonRect, Items[i], styleListItemToDraw)) {
                            ListVisible = false;
                            SelectedIndex = i;
                        }
                        if (i == SelectedIndex)
                            GUI.Label(new Rect(ListButtonRect) { x = ListButtonRect.x + ListButtonRect.width - 20 }, "✔");
                    }
                    CloseOnOutsideClick();
                }

            }

            internal Boolean CloseOnOutsideClick()
            {
                if (ListVisible && Event.current.type == EventType.mouseDown && !rectListBox.Contains(Event.current.mousePosition))
                {
                    ListVisible = false;
                    return true;
                }
                else { return false; }
            }
        }
    }

    public class GUIContentWithStyle
    {
        internal GUIContentWithStyle(String text, GUIStyle Style)
        {
            this.Content = new GUIContent(text);
            this.Style = new GUIStyle(Style);
        }
        internal GUIContentWithStyle(GUIContent src, GUIStyle Style)
        {
            this.Content = new GUIContent(src);
            this.Style = new GUIStyle(Style);
        }
        internal GUIContentWithStyle(Texture image, GUIStyle Style)
        {
            this.Content = new GUIContent(image);
            this.Style = new GUIStyle(Style);
        }
        internal GUIContentWithStyle(String text,Texture image, GUIStyle Style)
        {
            this.Content = new GUIContent(text,image);
            this.Style = new GUIStyle(Style);
        }
        internal GUIContentWithStyle(String text)
        {
            this.Content = new GUIContent(text);
        }
        internal GUIContentWithStyle(GUIContent src)
        {
            this.Content = new GUIContent(src);
        }
        internal GUIContentWithStyle(Texture image)
        {
            this.Content = new GUIContent(image);
        }
        internal GUIContentWithStyle(String text, Texture image)
        {
            this.Content = new GUIContent(text, image);
        }

        internal GUIContentWithStyle(){}


        internal GUIContent Content = null;
        internal GUIStyle Style = null;

        internal Single CalcWidth { get {
            Single RunningTotal=0;
            if (Style != null) 
                RunningTotal = Style.CalcSize(Content).x;
            if (Content.image != null)
                RunningTotal += Content.image.width;
            if (RunningTotal == 0)
                RunningTotal = 10;
            return RunningTotal;
            }
        }
        internal Single CalcHeight { get { if (Style != null) return Style.CalcSize(Content).x; else return 20; } }
    }
}