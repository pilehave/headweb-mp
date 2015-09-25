﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;
using System.Xml;
using System.ComponentModel;

namespace HeadWeb
{
﻿  public class GUISMSPurchase : GUIDialogWindow
   {

﻿  ﻿  public const int ID = 7770;
      public bool is_correct = false;

﻿  ﻿  public GUISMSPurchase()
      {
        GetID = ID;
      }﻿
     
    public override int GetID
      {
        get { return ID; }
      }


    [SkinControlAttribute(10)] protected GUIButtonControl btnEnterCode = null;
    private bool bIsSMSCodeButtonPressed = false;
﻿  ﻿  
﻿  ﻿  public string SMSId { get; set; }

﻿  ﻿  public bool IsCorrect
      {
        get { return is_correct; }
        set { is_correct = value; }
      }

﻿  ﻿  /// <summary>
﻿  ﻿  /// MediaPortal will set #currentmodule with GetModuleName()
﻿  ﻿  /// </summary>
﻿  ﻿  /// <returns>Window Name</returns>
﻿  ﻿  public override string GetModuleName()
      {
﻿  ﻿  ﻿  return "SMSPurchaseDialog";
      }

      public bool IsSMSCodeButtonPressed
      {
        get { return bIsSMSCodeButtonPressed; }
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
﻿  ﻿  ﻿  return Load(GUIGraphicsContext.Skin + @"\HeadWebSMSPurchase.xml");
      }

﻿  ﻿  public override void OnAction(Action action)
      {
﻿  ﻿  ﻿  switch (action.wID)
         {
﻿  ﻿  ﻿  ﻿  case Action.ActionType.ACTION_PREVIOUS_MENU:
﻿  ﻿  ﻿  ﻿  case Action.ActionType.ACTION_CLOSE_DIALOG:
﻿  ﻿  ﻿  ﻿  case Action.ActionType.ACTION_CONTEXT_MENU:﻿
             bIsSMSCodeButtonPressed = false;
            PageDestroy();
            return;
         }
﻿  ﻿  ﻿  base.OnAction(action);
      }

﻿  ﻿  protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
      {
        if (control == btnEnterCode)
        {
          bIsSMSCodeButtonPressed = true;
          PageDestroy();
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
        GUIControl.SetControlLabel(GetID, btnEnterCode.GetID, Translation.EnterSMSCode);
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
