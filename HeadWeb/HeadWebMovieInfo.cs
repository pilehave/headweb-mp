using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Configuration;
using MediaPortal.Util;
using System.Xml;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data;
using System;
using Action = MediaPortal.GUI.Library.Action;

namespace HeadWeb
{
  public class HeadWebMovieInfo : GUIWindow
  {

    #region skin controls
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnRent = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnRentSMS = null;
    [SkinControlAttribute(4)]
    protected GUIButtonControl btnPreview = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnTrailer = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnAddFavorite = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnRemoveFavorite = null;
    [SkinControlAttribute(8)]
    protected GUIButtonControl btnRateMovie = null;
    #endregion

    public class StreamInfo
    {
      public StreamInfo()
      {

      }
      public string Bitrate { get; set; }
      public string Text { get; set; }
    }

    #region Variables
    private string s_url = string.Empty;
    private string s_name = string.Empty;
    private string s_originalname = string.Empty;
    private string s_plot = string.Empty;
    private string s_year = string.Empty;
    private string s_director = string.Empty;
    private string s_actor = string.Empty;
    private float s_rating = 0;
    private string s_genre = string.Empty;
    private string s_price = string.Empty;
    private string s_rawprice = string.Empty;
    private bool b_ishd = false;
    private string sub_language = string.Empty;
    private string sub_url = string.Empty;
    private int s_runtime = 0;
    private int m_movieId = 0;
    private string m_thumbnailUrl = string.Empty;
    private string stream_id = string.Empty;
    private string stream_url = string.Empty;
    private string stream_url_id = string.Empty;
    List<string> trailer_url = new List<string>();
    List<string> trailer_bitrate = new List<string>();
    List<StreamInfo> sInfo = new List<StreamInfo>();

    // Create the runtime components
    int runtime_hours = 0;
    int runtime_minutes = 0;
    int runtime_seconds = 0;
    int runtime_totalMinutes = 0;
    #endregion

    #region Buffering
    PlayerFactory _bufferingPlayerFactory = null;
    PlayerFactory BufferingPlayerFactory
    {
      get { return _bufferingPlayerFactory; }
      set
      {
        _bufferingPlayerFactory = value;
        GUIPropertyManager.SetProperty("#HeadWeb.buffered", "0");
        GUIPropertyManager.SetProperty("#HeadWeb.IsBuffering", (value != null).ToString());
      }
    }
    #endregion

    public HeadWebMovieInfo()
    {
    }

    public override string GetModuleName()
    {
      return "HeadWeb";
    }

    public override int GetID
    {
      get { return 7772; }
      set { }
    }

    // Init the skin
    public override bool Init()
    {
      // Load the skin-file
      return Load(GUIGraphicsContext.Skin + @"\HeadWebMovieInfo.xml");
    }


    protected override void OnPageLoad()
    {
      Log.Debug("HeadWeb: MovieInfo OnPageLoad()");
      GUIPropertyManager.SetProperty("#HeadWeb.buffered", string.Empty);
      getMovieContent();
    }


    public int movieId
    {
      get { return m_movieId; }
      set { m_movieId = value; }
    }

    public string thumbnailUrl
    {
      get { return m_thumbnailUrl; }
      set { m_thumbnailUrl = value; }
    }


    private bool enterPinCode()
    {
      // Check if skin-file for PinCode dialog exists
      if (System.IO.File.Exists(GUIGraphicsContext.Skin + @"\HeadWebPinCode.xml"))
      {
        try
        {
          GUIPinCode pinCodeDlg = (GUIPinCode)GUIWindowManager.GetWindow(GUIPinCode.ID);
          pinCodeDlg.Reset();
          if (pinCodeDlg != null)
          {
            // Initialize Dialog
            pinCodeDlg.MasterCode = HeadWebSettings.Instance.s_pincode;
            pinCodeDlg.EnteredPinCode = string.Empty;
            pinCodeDlg.SetHeading(Translation.PinCode);
            pinCodeDlg.SetLine(1, Translation.EnterPinCode);
            pinCodeDlg.SetLine(2, Translation.ToPurchaseThisMovie);
            pinCodeDlg.Message = Translation.PinCodeIncorrect;
            pinCodeDlg.DoModal(pinCodeDlg.GetID);
          }
          if (!pinCodeDlg.IsCorrect)
          {
            // Prompt to choose UnProtected View
            Log.Info("HeadWeb: Pincode entered was incorrect");
            return false;
          }
        }
        catch (Exception e)
        {
          Log.Error("HeadWeb: enterPinCode() caused and exeption " + e.Message);
          return false;
        }
      }
      return true;
    }


    private bool getSMSCode()
    {
      Log.Info("HeadWeb: getSMSCode()");
      try
      {
        string sms_id = null;
        // Check if skin-file for SMS Purchase dialog exists
        if (System.IO.File.Exists(GUIGraphicsContext.Skin + @"\HeadWebSMSPurchase.xml"))
        {
          // Send SMS code request
          XmlDocument doc = new XmlDocument();
          string error_title = string.Empty;
          string error_description = string.Empty;
          sms_id = stream_id;
          string requestUrl = Utils.constructUrl("purchase/0", "payment=sms&total=" + s_rawprice + "&item=" + stream_id);
          string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false, false);
          Log.Info("SMS besked:" + rootXML);
          doc.LoadXml(rootXML);
          if (rootXML.IndexOf("headweb.error") > -1 && rootXML.IndexOf("headweb.error.purchasefailedearly") <= 0)
          {
            try
            {
              XmlNode error_title_node = doc.SelectSingleNode("//response/purchase/failed");
              error_title = error_title_node.InnerText;
            }
            catch { }

            try
            {
              XmlNode error_description_node = doc.SelectSingleNode("//response/purchase/item/error");
              error_description = error_description_node.InnerText;
            }
            catch { }

            Log.Info("HeadWeb: Error response: " + error_title + ":" + error_description);

            GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            if (dlgNotify != null)
            {
              // Show notify dialog
              dlgNotify.SetHeading(error_title);
              dlgNotify.SetText(error_description);
              dlgNotify.DoModal(GetID);
            }
            return false;
          }

          // allerede "købt", men vis kode igen
          if (rootXML.IndexOf("headweb.error.purchasefailedearly") > -1)
          {

            // Show SMS code again, user didn't enter it correctly the first time

            XmlNode smsid_node1 = doc.SelectSingleNode("//response/purchase/item");
            sms_id = smsid_node1.Attributes[0].Value;
            MessageBox.Show("sms id:" + sms_id.ToString());
            XmlNode sms_node = doc.SelectSingleNode("//response/purchase/item/name");
            string sms_title = sms_node.InnerText;

            GUISMSPurchase smsCodeDlg1 = (GUISMSPurchase)GUIWindowManager.GetWindow(GUISMSPurchase.ID);

            // Initialize Dialog
            if (smsCodeDlg1 != null)
            {
              smsCodeDlg1.Reset();
              smsCodeDlg1.SetHeading(Translation.SMSPurchase1);
              smsCodeDlg1.SetLine(1, Translation.SMSPurchase2);
              smsCodeDlg1.SetLine(2, Translation.SMSPurchase3);
              smsCodeDlg1.SetLine(3, Translation.SMSPurchase4);
              smsCodeDlg1.DoModal(smsCodeDlg1.GetID);
            }
            if (smsCodeDlg1.IsSMSCodeButtonPressed)
            {
              string enteredSMSCode = string.Empty;
              if (GetKeyboard(ref enteredSMSCode))
              {
                checkSMSCode(enteredSMSCode, sms_id);
              }
            }
          }
          else
          {
            // Show SMS code for the first time
            string amount = string.Empty;
            string code = string.Empty;
            string number = string.Empty;

            XmlElement root = doc.DocumentElement;
            XmlNode list = doc.SelectSingleNode("//response/purchase/sms");
            try { amount = list["amount"].InnerText; }
            catch { }
            try { code = list["code"].InnerText; }
            catch { }
            try { number = list["number"].InnerText; }
            catch { }
            try { number = list["number"].InnerText; }
            catch { }

            GUISMSPurchase smsCodeDlg = (GUISMSPurchase)GUIWindowManager.GetWindow(GUISMSPurchase.ID);

            // Initialize Dialog
            if (smsCodeDlg != null)
            {
              smsCodeDlg.Reset();

              smsCodeDlg.SetHeading(string.Format(Translation.SMSPurchase5, amount));
              smsCodeDlg.SetLine(1, Translation.SMSPurchase6);
              smsCodeDlg.SetLine(2, string.Format(Translation.SMSPurchase7, code, number));
              smsCodeDlg.SetLine(3, Translation.SMSPurchase8);
              smsCodeDlg.DoModal(smsCodeDlg.GetID);
            }
            if (smsCodeDlg.IsSMSCodeButtonPressed)
            {
              string enteredSMSCode = string.Empty;
              if (GetKeyboard(ref enteredSMSCode))
              {
                checkSMSCode(enteredSMSCode, sms_id);
              }
            }
          }

        }

        return true;
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getSMSCode() caused an exception " + e.Message);
      }
      return false;
    }





    private bool checkSMSCode(string smscode, string sms_id)
    {

      // Check Auth code request
      XmlDocument doc = new XmlDocument();
      string requestUrl = Utils.constructUrl("purchase/activate", "id=" + sms_id + "&authcode=" + smscode + "&fields=stream&authmode=raw");
      Log.Info("requestUrl=" + requestUrl);
      string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false, false);
      doc.LoadXml(rootXML);
      if (rootXML.IndexOf("headweb.error") > -1)
      {
        XmlNode errornode = doc.SelectSingleNode("//response/error");
        String error = errornode.InnerText;
        Log.Info("HeadWeb: Error response: " + error);

        GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (dlgNotify != null)
        {
          // Show notify dialog
          dlgNotify.SetHeading("Validation of entered code failed");
          dlgNotify.SetText(error);
          dlgNotify.DoModal(GetID);
        }
        return false;
      }
      else
      {
        Log.Info("HeadWeb: SMS response:" + rootXML);
        string sms_response = rootXML;
        playMovie(sms_response);
      }

      //XmlNode stream_node = doc.SelectSingleNode("//response/error");
      //String error = errornode.InnerText;

      //stream_id =

      return true;
    }





    private void rentMovie()
    {
      if (HeadWebSettings.Instance.s_pinapprovepurchase)
      {
        bool isAvailable = enterPinCode();
        if (!isAvailable) return;
      }

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (dlgYesNo != null)
      {
        // Show yes/no dialog
        dlgYesNo.SetHeading(Translation.RentMovie);
        dlgYesNo.SetLine(1, Translation.AreYouSure);
        dlgYesNo.SetLine(2, Translation.Title + ": " + s_name);
        dlgYesNo.SetLine(3, Translation.Price + ": " + s_price);
        dlgYesNo.DoModal(GetID);
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }
      }
      else
      {
        return;
      }

      // Charge the users account
      XmlDocument doc = new XmlDocument();
      string requestUrl = Utils.constructUrl("purchase/0", "payment=account&total=" + s_rawprice + "&item=" + stream_id);
      Log.Info("requestUrl=" + requestUrl);
      string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, true, false);
      doc.LoadXml(rootXML);
      if (rootXML.IndexOf("headweb.error") > -1)
      {
        XmlNode errornode = doc.SelectSingleNode("//response/purchase/failed");
        String error = errornode.InnerText;
        Log.Info("HeadWeb Error response: " + error);

        GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (dlgNotify != null)
        {
          // Show notify dialog
          dlgNotify.SetHeading(Translation.RentalFailed);
          dlgNotify.SetText(error);
          dlgNotify.DoModal(GetID);
        }
        return;
      }

      GUIDialogNotify dlgNotify_ok = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (dlgNotify_ok != null)
      {
        // Show notify dialog
        dlgNotify_ok.SetHeading(Translation.RentalSuccessful);
        dlgNotify_ok.SetText(Translation.MovieWillStart);
        dlgNotify_ok.DoModal(GetID);
      }

      // Play movie
      playMovie(string.Empty);
    }






    private void playMovie(string SMSResponse)
    {

      XmlDocument doc = new XmlDocument();
      string rootXML;
      if (SMSResponse.Length > 0)
      {
        rootXML = SMSResponse;
      }
      else
      {
        string requestUrl = Utils.constructUrl("stream/" + stream_id, "fields=stream&authmode=raw");
        rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false, false);
      }

      doc.LoadXml(rootXML);
      XmlElement root = doc.DocumentElement;
      XmlNodeList list = root.SelectNodes("/response/auth");

      foreach (XmlNode elm in list)
      {
        stream_url = elm["streamurl"].InnerText;
        stream_url_id = elm["streamid"].InnerText;
      }

      // get bitrates
      XmlNodeList bitratelist = root.SelectNodes("/response/content/stream/bitrate");
      int bitrate;
      string lsUrl = string.Empty;
      sInfo.Clear();
      foreach (XmlNode elm in bitratelist)
      {
        try
        {
          if (elm["rate"].InnerText.Length > 0)
          {
            StreamInfo streaminfo = new StreamInfo();
            streaminfo.Bitrate = elm["rate"].InnerText;
            if (elm["width"].InnerText == "1280")
            {
              streaminfo.Text = "(720p)";
            }

            else if (elm["width"].InnerText == "1920")
            {
              streaminfo.Text = "(1080p)";
            }
            sInfo.Add(streaminfo);
          }
        }
        catch { }
      }
      int c = sInfo.Count;
      if (c > 1)
      {
        if (HeadWebSettings.Instance.s_video_quality != string.Empty && HeadWebSettings.Instance.s_video_quality != "Let me choose")
        {
          if (HeadWebSettings.Instance.s_video_quality == "Low")
          {
            bitrate = 0;
          }
          else if (HeadWebSettings.Instance.s_video_quality == "High")
          {
            bitrate = sInfo.Count - 1;
          }
          else
          {
            bitrate = (c + 1) / 2;
            bitrate = (int)Math.Floor((double)bitrate);
            if (bitrate < 0) bitrate = 0;
          }
          Log.Info("HeadWeb: Play movie with bitrate: " + sInfo[bitrate].Bitrate);
          lsUrl = sInfo[bitrate].Bitrate;
        }
        else
        {
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg == null)
          {
            return;
          }

          dlg.Reset();
          dlg.SetHeading(Translation.SelectBitrate); // Bitrate options

          foreach (StreamInfo inf in sInfo)
          {
            dlg.Add(inf.Bitrate + " " + inf.Text);
          }

          // show dialog and wait for result
          dlg.DoModal(GetID);
          if (dlg.SelectedId == -1)
          {
            return;
          }
          Log.Info("HeadWeb: Play movie with bitrate: " + dlg.SelectedLabelText);
          lsUrl = sInfo[dlg.SelectedId - 1].Bitrate;
        }
      }











      if (lsUrl.Length > 0)
      {
        stream_url_id = stream_url_id.Replace("?t=", "_" + lsUrl + "?t=");
        lsUrl = stream_url + "/" + stream_url_id;
      }
      else
      {
        lsUrl = stream_url + "/" + stream_url_id;
      }
      Log.Debug("HeadWeb: streamURL=" + lsUrl);

      // translate rtmp urls to the local proxy
      if (new Uri(lsUrl).Scheme.ToLower().StartsWith("rtmp"))
      {
        lsUrl = ReverseProxy.Instance.GetProxyUri(RTMP_LIB.RTMPRequestHandler.Instance,
                        string.Format("http://127.0.0.1/stream.flv?rtmpurl={0}&hostname=fl2.stream.headweb.com&port=443&playpath={1}&live=no",
                        System.Web.HttpUtility.UrlEncode(lsUrl), System.Web.HttpUtility.UrlEncode(stream_url_id)));
      }

      // load and save subs
      XmlNodeList sublist = root.SelectNodes("/response/content/stream/subtitle");
      foreach (XmlNode elm in sublist)
      {

        try { sub_language = elm["language"].InnerText; }
        catch { }
        Log.Debug("HeadWeb: Subtitle found: " + sub_language);
        try { sub_url = elm["url"].InnerText; }
        catch { }
        Utils.saveSub(HeadWebSettings.Instance.s_movietitle, sub_url, sub_language);
      }

      // stop player if currently playing some other video
      if (g_Player.Playing) g_Player.Stop();

      PlayerFactory factory = new PlayerFactory(PlayerType.Internal, lsUrl);

      Log.Info("HeadWeb: Preparing graph for playback of {0}", lsUrl);
      bool? prepareResult = ((OnlineVideosPlayer)factory.PreparedPlayer).PrepareGraph();
      switch (prepareResult)
      {
        case true:// buffer in background
          Gui2UtilConnector.Instance.ExecuteInBackgroundAndCallback(delegate()
          {
            try
            {
              Log.Info("HeadWeb: Start prebuffering...");
              BufferingPlayerFactory = factory;
              if (((OnlineVideosPlayer)factory.PreparedPlayer).BufferFile())
              {
                Log.Info("HeadWeb: Prebuffering finished");
                return true;
              }
              else
              {
                Log.Info("HeadWeb: Prebuffering failed");
                return null;
              }
            }
            finally
            {
              BufferingPlayerFactory = null;
            }
          },
          delegate(bool success, object result)
          {
            Play_Step4(lsUrl, true, factory, result as bool?);
          },
          "StartingPlayback", false);

          break;
        case false:// play without buffering in background
          Play_Step4(lsUrl, true, factory, prepareResult);
          break;
        default: // error building graph
          GUIDialogNotify dlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlg != null)
          {
            dlg.Reset();
            dlg.SetHeading("Error");
            dlg.SetText("UnableToPlayVideo");
            dlg.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;
      }
    }



    void Play_Step4(string lsUrl, bool goFullScreen, PlayerFactory factory, bool? factoryPrepareResult)
    {
      if (factoryPrepareResult == null)
      {
        bool showMessage = true;
        if (factory.PreparedPlayer is OnlineVideosPlayer && (factory.PreparedPlayer as OnlineVideosPlayer).BufferingStopped == true) showMessage = false;
        factory.PreparedPlayer.Dispose();
        if (showMessage)
        {
          GUIDialogNotify dlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlg != null)
          {
            dlg.Reset();
            dlg.SetHeading("Error");
            dlg.SetText("UnableToPlayVideo");
            dlg.DoModal(GUIWindowManager.ActiveWindow);
          }
        }
      }
      else
      {
        (factory.PreparedPlayer as OVSPLayer).GoFullscreen = goFullScreen;

        IPlayerFactory savedFactory = g_Player.Factory;
        g_Player.Factory = factory;
        bool playing = g_Player.Play(lsUrl, g_Player.MediaType.Video);
        g_Player.Factory = savedFactory;

        if (g_Player.Player != null && g_Player.HasVideo)
        {
          setGuiMovie();
        }
      }
    }



    private void playTrailer()
    {
      string lsUrl;
      int trailer;
      if (HeadWebSettings.Instance.s_video_quality != string.Empty && HeadWebSettings.Instance.s_video_quality != "Let me choose" && trailer_url.Count() > 0)
      {
        if (HeadWebSettings.Instance.s_video_quality == "Low")
        {
          trailer = 0;
        }
        else if (HeadWebSettings.Instance.s_video_quality == "High")
        {
          trailer = trailer_url.Count - 1;
        }
        else
        {
          double trailerd = (trailer_url.Count) / 2;
          trailer = (int)Math.Floor((double)trailerd);
          if (trailer < 0) trailer = 0;
          lsUrl = trailer_url[trailer];
        }
        Log.Debug("HeadWeb: Play trailer: " + trailer_bitrate[trailer]);
        lsUrl = trailer_url[trailer];
      }
      else
      {

        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }

        dlg.Reset();
        dlg.SetHeading(Translation.SelectBitrate); // Bitrate options

        foreach (string btr in trailer_bitrate)
        {
          dlg.Add(btr);
        }

        // show dialog and wait for result
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
        {
          return;
        }



        Log.Debug("HeadWeb: Play trailer with bitrate: " + dlg.SelectedLabelText);
        lsUrl = trailer_url[dlg.SelectedId - 1];
      }
      if (g_Player.Playing) g_Player.Stop();
      //MediaPortal.Player.g_Player.PlayVideoStream(trailer_url[dlg.SelectedId-1]);

      PlayerFactory factory = new PlayerFactory(PlayerType.Internal, lsUrl);

      Log.Info("HeadWeb: Preparing graph for playback of {0}", lsUrl);
      bool? prepareResult = ((OnlineVideosPlayer)factory.PreparedPlayer).PrepareGraph();
      switch (prepareResult)
      {
        case true:// buffer in background
          Gui2UtilConnector.Instance.ExecuteInBackgroundAndCallback(delegate()
          {
            try
            {
              Log.Info("HeadWeb: Start prebuffering...");
              BufferingPlayerFactory = factory;
              if (((OnlineVideosPlayer)factory.PreparedPlayer).BufferFile())
              {
                Log.Info("HeadWeb: Prebuffering finished");
                return true;
              }
              else
              {
                Log.Info("HeadWeb: Prebuffering failed");
                return null;
              }
            }
            finally
            {
              BufferingPlayerFactory = null;
            }
          },
          delegate(bool success, object result)
          {
            Play_Step4(lsUrl, true, factory, result as bool?);
          },
          "StartingPlayback", false);
          break;
        case false:// play without buffering in background
          Play_Step4(lsUrl, true, factory, prepareResult);
          break;
        default: // error building graph
          GUIDialogNotify errdlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (errdlg != null)
          {
            errdlg.Reset();
            errdlg.SetHeading("Error");
            errdlg.SetText("UnableToPlayVideo");
            errdlg.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;
      }





      MediaPortal.Player.g_Player.ShowFullScreenWindowVideo();

    }


    private void addFavorite(int itemId)
    {
      Log.Info("HeadWeb: addFavorite()");
      try
      {
        string movieTitle = string.Empty;
        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl("user/watchlist/add", "");
        string postRequestParams = "fields=name&item=" + itemId;
        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "POST", postRequestParams, false, false);
        doc.LoadXml(rootXML);

        if (rootXML.IndexOf("headweb.error") > -1)
        {
          XmlNode errornode = doc.SelectSingleNode("//response/error");
          String error = errornode.InnerText;
          Log.Info("HeadWeb: Error response: " + error);

          GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify != null)
          {
            // Show notify dialog
            dlgNotify.SetHeading("Add favorite failed");
            dlgNotify.SetText(error);
            dlgNotify.DoModal(GetID);
          }
          return;
        }

        XmlElement root = doc.DocumentElement;
        XmlNode name = doc.SelectSingleNode("//response/content/name");
        movieTitle = name.InnerText;

        GUIDialogNotify dlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (dlg != null)
        {
          dlg.Reset();
          if (!string.IsNullOrEmpty(movieTitle))
          {
            HeadWebSettings.Instance.s_favorit.Add(itemId);
            dlg.SetHeading(Translation.AddToFavorites);
            dlg.SetText(string.Format(Translation.AddToFavoritesText, movieTitle));
            GUIControl.ShowControl(GetID, btnRemoveFavorite.GetID);
            GUIControl.HideControl(GetID, btnAddFavorite.GetID);
            GUIControl.FocusControl(GetID, btnRemoveFavorite.GetID);
          }
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: addFavorite() caused an exception " + e.Message);
      }
    }

    private void removeFavorite(int itemId)
    {
      Log.Info("HeadWeb: removeFavorite()");
      try
      {
        string movieTitle = string.Empty;
        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl("user/watchlist/remove", "");
        string postRequestParams = "fields=name&item=" + itemId;
        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "POST", postRequestParams, false, false);
        doc.LoadXml(rootXML);

        if (rootXML.IndexOf("headweb.error") > -1)
        {
          XmlNode errornode = doc.SelectSingleNode("//response/error");
          String error = errornode.InnerText;
          Log.Info("HeadWeb: Error response: " + error);

          GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify != null)
          {
            // Show notify dialog
            dlgNotify.SetHeading("Remove favorite failed");
            dlgNotify.SetText(error);
            dlgNotify.DoModal(GetID);
          }
          return;
        }

        XmlElement root = doc.DocumentElement;

        XmlNode name = doc.SelectSingleNode("//response/content/name");
        movieTitle = name.InnerText;

        GUIDialogNotify dlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (dlg != null)
        {
          dlg.Reset();
          if (!string.IsNullOrEmpty(movieTitle))
          {
            HeadWebSettings.Instance.s_favorit.RemoveAll(element => element == itemId);
            dlg.SetHeading(Translation.RemoveFromFavorites);
            dlg.SetText(string.Format(Translation.RemoveFromFavoritesText, movieTitle));
            GUIControl.ShowControl(GetID, btnAddFavorite.GetID);
            GUIControl.HideControl(GetID, btnRemoveFavorite.GetID);
            GUIControl.FocusControl(GetID, btnAddFavorite.GetID);
          }
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: removeFavorite() caused an exception " + e.Message);
      }
    }




    private void rateMovie(int itemId)
    {
      Log.Info("HeadWeb: rateMovie()");
      try
      {

        XmlDocument doc_get = new XmlDocument();
        string requestUrl_get = Utils.constructUrl("content/" + movieId.ToString() + "/rate", "");
        string rootXML_get = Utils.GetWebData(requestUrl_get, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false, false);
        doc_get.LoadXml(rootXML_get);
        XmlNode rating_get = doc_get.SelectSingleNode("//response/rating");
        double drating = double.Parse(rating_get.InnerText, NumberFormatInfo.InvariantInfo);
        int rating = Convert.ToInt32(drating);

        GUIDialogSetRating dlg = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_RATING);
        if (dlg != null && !string.IsNullOrEmpty(s_name))
        {
          dlg.SetTitle(String.Format("{0}-{1}", s_name, s_year));
          dlg.Rating = rating;
          GUIControl.HideControl(dlg.GetID, 13);
          GUIControl.HideControl(dlg.GetID, 14);
          GUIControl.HideControl(dlg.GetID, 15);
        }
        dlg.DoModal(GetID);

        XmlDocument doc_set = new XmlDocument();
        string requestUrl_set = Utils.constructUrl("content/" + movieId.ToString() + "/rate", "");
        string postRequestParams = "score=" + dlg.Rating;
        string rootXML_set = Utils.GetWebData(requestUrl_set, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "POST", postRequestParams, false, false);
        doc_set.LoadXml(rootXML_set);
        XmlElement root_set = doc_set.DocumentElement;
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: rateMovie() caused an exception " + e.Message);
      }
    }





    private void getMovieContent()
    {
      Log.Info("HeadWeb: getMovieContent()");
      try
      {


        XmlDocument doc_status = new XmlDocument();
        string requestUrl_status = Utils.constructUrl("user/rentals", "&fields=name");
        string rootXML_status = Utils.GetWebData(requestUrl_status, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false);
        doc_status.LoadXml(rootXML_status);
        XmlElement root_status = doc_status.DocumentElement;
        XmlNodeList list_status = root_status.SelectNodes("/response/list/item");
        foreach (XmlNode elm in list_status)
        {
          try
          {
            if (elm["content"].GetAttribute("id") == movieId.ToString() && elm["state"].InnerText == "timeleft")
            {
              GUIControl.HideControl(GetID, btnRentSMS.GetID);
              GUIControl.HideControl(GetID, btnRent.GetID);
              GUIControl.SetControlLabel(GetID, btnPreview.GetID, Translation.WatchMovie);
              GUIControl.FocusControl(GetID, btnPreview.GetID);
            }
          }
          catch { }
        }

        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl("content/" + movieId.ToString(), "");
        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false);
        doc.LoadXml(rootXML);
        XmlElement root = doc.DocumentElement;
        XmlNodeList list = root.SelectNodes("/response/content");
        foreach (XmlNode elm in list)
        {
          try { s_url = elm["url"].InnerText; }
          catch { }

          if (HeadWebSettings.Instance.s_useoriginalname)
          {
            try
            {
              s_name = elm["originalname"].InnerText;
              if (s_name.Length <= 0)
              {
                s_name = elm["name"].InnerText;
              }
              HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(s_name);
            }
            catch
            {
              try { s_name = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(s_name); }
              catch { }
            }
          }
          else
          {
            try { s_name = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(s_name); }
            catch { }
          }
          try { s_originalname = elm["originalname"].InnerText; }
          catch { }
          try { b_ishd = bool.Parse(elm["hd"].InnerText); }
          catch { b_ishd = false; }
          try { s_plot = elm["plot"].InnerText; }
          catch { s_plot = "N/A"; }
          try { s_year = elm["year"].InnerText; }
          catch { s_year = "N/A"; }
          try { s_director = elm["director"].InnerText; }
          catch { s_director = "N/A"; }
          try { s_actor = Utils.getActors(root); }
          catch { s_actor = "N/A"; }
          try { s_rating = (float.Parse(elm["rating"].InnerText) / 5); }
          catch { }
          try { stream_id = elm["stream"].GetAttribute("id"); }
          catch { }
          try { s_genre = Utils.getGenres(root); }
          catch { s_genre = "N/A"; }
        }

        trailer_url.Clear();
        trailer_bitrate.Clear();
        XmlNodeList trailerlist = root.SelectNodes("//videoclip");
        if (trailerlist.Count > 0)
        {
          foreach (XmlNode elm in trailerlist)
          {
            XmlNodeList trailers = elm.SelectNodes("bitrate");
            foreach (XmlNode trailer in trailers)
            {
              string trailerurl = trailer["url"].InnerText;
              string bitrate = trailer.Attributes["rate"].Value;
              trailer_url.Add(trailerurl);
              trailer_bitrate.Add(bitrate);
              Log.Debug("HeadWeb: Trailer bitrate: " + bitrate + ", url: " + trailerurl);
            }
            if (trailer_url.Count > 0)
            {
              GUIControl.ShowControl(GetID, btnTrailer.GetID);
            }
          }
        }
        else
        {
          GUIControl.HideControl(GetID, btnTrailer.GetID);
        }


        XmlNode pricenode = doc.SelectSingleNode("//stream/price");
        s_price = pricenode.InnerText;
        s_rawprice = pricenode.Attributes["raw"].InnerText;

        if (s_rawprice == "0.0")
        {
          GUIControl.HideControl(GetID, btnRentSMS.GetID);
          GUIControl.HideControl(GetID, btnRent.GetID);
          GUIControl.SetControlLabel(GetID, btnPreview.GetID, "Watch movie");
          GUIControl.FocusControl(GetID, btnPreview.GetID);
          return;
        }

        XmlNode pubdatenode = doc.SelectSingleNode("//stream/pubdate");
        DateTime utcDateTime_movie = DateTime.Parse(pubdatenode.InnerText);
        DateTime utcDateTime_now = DateTime.Now;
        if (utcDateTime_movie > utcDateTime_now)
        {
          GUIControl.HideControl(GetID, btnRent.GetID);
          GUIControl.HideControl(GetID, btnRentSMS.GetID);
          GUIControl.HideControl(GetID, btnPreview.GetID);
          Log.Info("HeadWeb: Movie " + s_name + "not released yet");
        }

        XmlNode runtimenode = doc.SelectSingleNode("//stream/runtime");
        s_runtime = Int32.Parse(runtimenode.InnerText);

        // create the time components
        runtime_hours = (s_runtime / 3600);
        runtime_minutes = ((s_runtime / 60) % 60);
        runtime_seconds = (s_runtime % 60);
        runtime_totalMinutes = (s_runtime / 60);


        // Find out if movie is already in favorites
        bool exists = HeadWebSettings.Instance.s_favorit.Exists(element => element == movieId);
        if (exists)
        {
          GUIControl.HideControl(GetID, btnAddFavorite.GetID);
        }
        else
        {
          GUIControl.HideControl(GetID, btnRemoveFavorite.GetID);
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getMovieContent() caused an exeption" + e.Message);
      }


      GUIPropertyManager.SetProperty("#HeadWeb.MovieUrl", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieName", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieOriginalName", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MoviePlot", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieYear", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieDirector", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieActors", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieRating", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieGenre", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MovieThumb", string.Empty);
      GUIPropertyManager.SetProperty("#HeadWeb.MoviePrice", string.Empty);
      if (!string.IsNullOrEmpty(s_url)) GUIPropertyManager.SetProperty("#HeadWeb.MovieUrl", s_url);
      if (!string.IsNullOrEmpty(s_name)) GUIPropertyManager.SetProperty("#HeadWeb.MovieName", s_name);
      if (!string.IsNullOrEmpty(s_originalname)) GUIPropertyManager.SetProperty("#HeadWeb.MovieOriginalName", s_originalname);
      if (!string.IsNullOrEmpty(s_plot)) GUIPropertyManager.SetProperty("#HeadWeb.MoviePlot", s_plot);
      if (!string.IsNullOrEmpty(s_year)) GUIPropertyManager.SetProperty("#HeadWeb.MovieYear", s_year);
      if (!string.IsNullOrEmpty(s_director)) GUIPropertyManager.SetProperty("#HeadWeb.MovieDirector", s_director);
      if (!string.IsNullOrEmpty(s_actor)) GUIPropertyManager.SetProperty("#HeadWeb.MovieActors", s_actor);
      if (!string.IsNullOrEmpty(s_rating.ToString())) GUIPropertyManager.SetProperty("#HeadWeb.MovieRating", s_rating.ToString());
      if (!string.IsNullOrEmpty(s_genre)) GUIPropertyManager.SetProperty("#HeadWeb.MovieGenre", s_genre);
      if (!string.IsNullOrEmpty(thumbnailUrl)) GUIPropertyManager.SetProperty("#HeadWeb.MovieThumb", thumbnailUrl);
      if (!string.IsNullOrEmpty(s_price)) GUIPropertyManager.SetProperty("#HeadWeb.MoviePrice", s_price);
      if (b_ishd) GUIPropertyManager.SetProperty("#HeadWeb.MovieIsHD", "yes"); else GUIPropertyManager.SetProperty("#HeadWeb.MovieIsHD", "no");

      if (!string.IsNullOrEmpty(runtime_hours.ToString() + "h " + runtime_minutes.ToString() + "s")) GUIPropertyManager.SetProperty("#HeadWeb.MovieRuntime", runtime_hours.ToString() + "h " + runtime_minutes.ToString() + "m");
      if (!string.IsNullOrEmpty(runtime_hours.ToString())) GUIPropertyManager.SetProperty("#HeadWeb.MovieRuntimeHours", runtime_hours.ToString());
      if (!string.IsNullOrEmpty(runtime_minutes.ToString())) GUIPropertyManager.SetProperty("#HeadWeb.MovieRuntimeMinutes", runtime_minutes.ToString());
      if (!string.IsNullOrEmpty(runtime_totalMinutes.ToString())) GUIPropertyManager.SetProperty("#HeadWeb.MovieRuntimeTotalMinutes", runtime_totalMinutes.ToString());


    }

    public void setGuiMovie()
    {

      new System.Threading.Thread(delegate()
      {
        System.Threading.Thread.Sleep(2000);
        Log.Info("HeadWeb: Setting Video Properties");
        if (!string.IsNullOrEmpty(s_name)) GUIPropertyManager.SetProperty("#Play.Current.Title", s_name);
        if (!string.IsNullOrEmpty(s_plot)) GUIPropertyManager.SetProperty("#Play.Current.Plot", s_plot);
        if (!string.IsNullOrEmpty(this.thumbnailUrl)) GUIPropertyManager.SetProperty("#Play.Current.Thumb", this.thumbnailUrl);
        if (!string.IsNullOrEmpty(s_year)) GUIPropertyManager.SetProperty("#Play.Current.Year", s_year);
      }) { IsBackground = true, Name = "OnlineVideosInfosSetter" }.Start();


      if (!string.IsNullOrEmpty(s_name)) GUIPropertyManager.SetProperty("#Play.Current.Title", s_name);
      if (!string.IsNullOrEmpty(s_plot)) GUIPropertyManager.SetProperty("#Play.Current.Plot", s_plot);
      if (!string.IsNullOrEmpty(this.thumbnailUrl)) GUIPropertyManager.SetProperty("#Play.Current.Thumb", this.thumbnailUrl);
      if (!string.IsNullOrEmpty(s_year)) GUIPropertyManager.SetProperty("#Play.Current.Year", s_year);
      if (!string.IsNullOrEmpty(s_genre)) GUIPropertyManager.SetProperty("#Play.Current.Genre", s_genre);
    }

    public void UpdateGui()
    {

      if (HeadWebSettings.Instance.s_loggedin)
      {
        GUIPropertyManager.SetProperty("#HeadWeb.UserName", HeadWebSettings.Instance.s_username);
        GUIPropertyManager.SetProperty("#HeadWeb.FirstName", HeadWebSettings.Instance.s_firstname);
        GUIPropertyManager.SetProperty("#HeadWeb.LastName", HeadWebSettings.Instance.s_lastname);
        GUIPropertyManager.SetProperty("#HeadWeb.Balance", HeadWebSettings.Instance.s_balance);
        GUIControl.EnableControl(GetID, btnRent.GetID);
        GUIControl.EnableControl(GetID, btnAddFavorite.GetID);
        GUIControl.EnableControl(GetID, btnRemoveFavorite.GetID);
        GUIControl.EnableControl(GetID, btnRateMovie.GetID);
      }
      else
      {
        GUIPropertyManager.SetProperty("#HeadWeb.UserName", Translation.NotLoggedIn);
        GUIPropertyManager.SetProperty("#HeadWeb.FirstName", string.Empty);
        GUIPropertyManager.SetProperty("#HeadWeb.LastName", string.Empty);
        GUIPropertyManager.SetProperty("#HeadWeb.Balance", string.Empty);
        GUIControl.DisableControl(GetID, btnRent.GetID);
        GUIControl.DisableControl(GetID, btnAddFavorite.GetID);
        GUIControl.DisableControl(GetID, btnRemoveFavorite.GetID);
        GUIControl.DisableControl(GetID, btnRateMovie.GetID);
      }
    }

    public override void Process()
    {
      // Update the gui
      UpdateGui();
      base.Process();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnRent)
      {
        if (HeadWebSettings.Instance.s_loggedin)
        {
          rentMovie();
        }
      }

      if (control == btnRentSMS)
      {
        getSMSCode();
      }

      else if (control == btnPreview)
      {
        playMovie(string.Empty);
      }

      else if (control == btnTrailer)
      {
        playTrailer();
      }

      else if (control == btnAddFavorite)
      {
        if (HeadWebSettings.Instance.s_loggedin)
        {
          addFavorite(movieId);
        }
      }

      else if (control == btnRemoveFavorite)
      {
        if (HeadWebSettings.Instance.s_loggedin)
        {
          removeFavorite(movieId);
        }
      }

      else if (control == btnRateMovie)
      {
        if (HeadWebSettings.Instance.s_loggedin)
        {
          rateMovie(movieId);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    protected virtual bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }
  }
}
