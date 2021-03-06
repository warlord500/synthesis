﻿using UnityEngine;

public class Popup
{
    static int popupListHash = "PopupList".GetHashCode();
    static bool listActive = false;

    public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
                             GUIStyle listStyle)
    {
        return List(position, ref showList, ref listEntry, buttonContent, listContent, "button", "box", listStyle);
    }

    public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
                             GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
    {
        int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
        bool done = false;


        if (position.Contains(Event.current.mousePosition))
        {
            if (Input.GetMouseButtonDown(0) && !listActive)
            {
                showList = true;
                listActive = true;
            }
            buttonStyle.normal.background = Resources.Load("Images/popuptexture") as Texture2D;
        }
        else buttonStyle.normal.background = Resources.Load("Images/normalbuttontexture") as Texture2D;
        

        GUI.Label(position, buttonContent, buttonStyle);
        if (showList)
        {
            Rect listRect = new Rect(position.x, position.y+position.height, position.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length);
            GUI.Box(listRect, "", boxStyle);
            //This is SUPPOSED to change the list entry to whatever your mouse is hovering over when clicking, but all it does is show the list.
            listEntry = GUI.SelectionGrid(listRect, listEntry, listContent, 1, listStyle);
            if (Input.GetMouseButtonDown(0))
            {
                if (listRect.Contains(Event.current.mousePosition))
                {
                    //This actually changes the list entry based on the mouse position. It's sort of a cheat, but Unity's built in GUI methods didn't work.
                    listEntry = Mathf.FloorToInt((Event.current.mousePosition.y - listRect.y) / listStyle.CalcHeight(listContent[0], 1.0f));
                    done = true;
                }
                else if (!position.Contains(Event.current.mousePosition))
                {
                    done = true;
                }
            }
        }
        if (done)
        {
            showList = false;
            listActive = false;
        }
        return done;
    }
}