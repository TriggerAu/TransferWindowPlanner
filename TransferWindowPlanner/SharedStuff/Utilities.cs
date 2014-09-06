using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace TransferWindowPlanner
{
    internal static class Utilities
    {
        internal static void CopyTextToClipboard(String CopyText)
        {
            TextEditor t = new TextEditor();
            t.content = new GUIContent(CopyText);
            t.SelectAll();
            t.Copy();
        }

        public static String AppendLine(this String s, String LineToAdd, params object[] args)
        {
            if (!s.EndsWith("\r\n"))
                s += "\r\n";
            s += String.Format(LineToAdd,args);
            return s;
        }
    }
}
