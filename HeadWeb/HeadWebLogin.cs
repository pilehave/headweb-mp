﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;
using System.Net;
using System.Xml;
using System.ComponentModel;

namespace HeadWeb
{
﻿  public class GUILogin : GUIDialogWindow
   {

﻿  ﻿  public const int ID = 7778;

﻿  ﻿  public GUILogin()
      {
        GetID = ID;
      }﻿
     
    public override int GetID
      {
        get { return ID; }
      }


    [SkinControlAttribute(8)] protected GUIButtonControl btnEnterUsername = null;
    [SkinControlAttribute(9)] protected GUIButtonControl btnEnterPassword = null;
    [SkinControlAttribute(10)] protected GUIButtonControl btnOK = null;
    private bool bIsUsernameButtonPressed = false;
    private bool bIsPasswordButtonPressed = false;
﻿  ﻿  

﻿  ﻿  /// <summary>
﻿  ﻿  /// MediaPortal will set #currentmodule with GetModuleName()
﻿  ﻿  /// </summary>
﻿  ﻿  /// <returns>Window Name</returns>
﻿  ﻿  public override string GetModuleName()
      {
﻿  ﻿  ﻿  return "LoginDialog";
      }

      public bool IsUsernameButtonPressed
      {
        get { return bIsUsernameButtonPressed; }
      }

      public bool IsPasswordButtonPressed
      {
        get { return bIsPasswordButtonPressed; }
      }

﻿  ﻿  public override void Reset()
      {
﻿  ﻿  ﻿  base.Reset();
﻿  ﻿  ﻿  SetHeading("");
﻿  ﻿  ﻿  SetLine(1, "");
﻿  ﻿  ﻿  SetLine(2, "");
﻿  ﻿  ﻿  SetLine(3, "");
﻿  ﻿  ﻿  SetLine(4, "");
      }

﻿  ﻿  public override bool Init()
      {
﻿  ﻿  ﻿  return Load(GUIGraphicsContext.Skin + @"\HeadWebLogin.xml");
      }

﻿  ﻿  public override void OnAction(Action action)
      {
﻿  ﻿  ﻿  switch (action.wID)
         {
﻿  ﻿  ﻿  ﻿  case Action.ActionType.ACTION_PREVIOUS_MENU:
﻿  ﻿  ﻿  ﻿  case Action.ActionType.ACTION_CLOSE_DIALOG:
﻿  ﻿  ﻿  ﻿  case Action.ActionType.ACTION_CONTEXT_MENU:﻿
            PageDestroy();
            return;
         }
﻿  ﻿  ﻿  base.OnAction(action);
      }

﻿  ﻿  protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
      {
        bIsUsernameButtonPressed = false;
        bIsPasswordButtonPressed = false;

        if (control == btnEnterUsername)
        {
          bIsUsernameButtonPressed = true;
          PageDestroy();
        }

        if (control == btnEnterPassword)
        {
          bIsPasswordButtonPressed = true;
          PageDestroy();
        }

        if (control == btnOK)
        {
          HeadWeb hw = (HeadWeb)GUIWindowManager.GetWindow(HeadWeb.ID);
          HeadWebSettings.Instance.s_cc = new CookieContainer();
          bool loginresult = hw.loginUser(HeadWebSettings.Instance.s_username, HeadWebSettings.Instance.s_password);
          if (loginresult)
          {
            HeadWebSettings.Instance.s_loggedin = true;
            hw.getFavorites(true);
            PageDestroy();
          }
        }

        base.OnClicked(controlId, control, actionType);﻿
      }

﻿  ﻿  public override bool OnMessage(GUIMessage message)
      {﻿  ﻿  ﻿  
﻿  ﻿  ﻿  base.OnMessage(message);
         return true;
      }

      public void SetHeading(string strLine)
      {
        //LoadSkin();
        AllocResources();
        InitControls();

        SetControlLabel(GetID, 1, strLine);

        SetLine(1, string.Empty);
        SetLine(2, string.Empty);
        SetLine(3, string.Empty);
        SetLine(4, string.Empty);
        GUIControl.SetControlLabel(GetID, btnOK.GetID, Translation.LogIn);
        GUIPropertyManager.SetProperty("#HeadWeb.Login.UserName", HeadWebSettings.Instance.s_username);
        string paddedPasswordString = "";
        paddedPasswordString = paddedPasswordString.PadLeft(HeadWebSettings.Instance.s_password.Length, '*');
        GUIPropertyManager.SetProperty("#HeadWeb.Login.Password", paddedPasswordString);
      }

      public void SetHeading(int iString)
      {
        if (iString == 0)
        {
          SetHeading(string.Empty);
        }
        else
        {
          SetHeading(GUILocalizeStrings.Get(iString));
        }
      }

      public void SetLine(int iLine, string strLine)
      {
        if (iLine <= 0)
        {
          return;
        }
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + iLine, 0, 0, null);
        msg.Label = strLine;
        OnMessage(msg);
      }

      public void SetLine(int iLine, int iString)
      {
        if (iLine <= 0)
        {
          return;
        }
        if (iString == 0)
        {
          SetLine(iLine, string.Empty);
        }
        SetLine(iLine, GUILocalizeStrings.Get(iString));
      }
﻿  }
 }
