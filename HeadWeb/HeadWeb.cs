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
using System.Xml.Serialization;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data;
using System;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace HeadWeb
{
  [PluginIcons("HeadWeb.Plugin.png", "HeadWeb.PluginDisabled.png")]
  public class HeadWeb : GUIWindow, ISetupForm, IShowPlugin
  {

    public const int ID = 7771;

    enum Show
    {
      BestSellers = 0,
      Newest = 1,
      Genres = 2,
      Search = 3,
      Rentals = 4,
      Favorites = 5,
      Free = 6,
    }

    #region IShowPlugin Implementation
    bool IShowPlugin.ShowDefaultHome()
    {
      return true;
    }
    #endregion

    #region MapSettings class
    [Serializable]
    public class MapSettings
    {
      protected int _ViewAs;

      public MapSettings()
      {
        // Set default view
        _ViewAs = (int)View.List;
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }
    }
    #endregion

    #region ShowSettings class
    [Serializable]
    public class ShowSettings
    {
      protected int _ShowAs;

      public ShowSettings()
      {
        // Set default view
        _ShowAs = (int)Show.BestSellers;
      }

      [XmlElement("ShowAs")]
      public int ShowAs
      {
        get { return _ShowAs; }
        set { _ShowAs = value; }
      }
    }
    #endregion

    protected virtual Layout CurrentLayout
    {
      get { return currentLayout; }
      set { currentLayout = value; }
    }

    public class MovieInfo
    {
      public MovieInfo()
      {

      }
      public int MovieID { get; set; }
      public string Directors { get; set; }
      public string Genres { get; set; }
      public string Plot { get; set; }
    }


    #region variables
    MapSettings mapSettings = new MapSettings();
    ShowSettings showSettings = new ShowSettings();
    public int movieId = 0;
    bool firstLoadDone = false;
    bool isGenre = false;
    bool isGenreContent = false;
    bool isGenreContentShown = false;
    private bool OnlyFree = false;
    private bool OnlyHD = false;
    int rememberIndex = -1;
    int rememberGenreIndex = -1;
    int rememberGenreId = 0;
    protected Layout currentLayout = Layout.List;
    protected ViewHandler handler;
    List<string> directors = new List<string>();
    List<MovieInfo> mInfo = new List<MovieInfo>();


    #endregion

    #region skin controls
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnSwitchView = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnBestSellers = null;
    [SkinControlAttribute(4)]
    protected GUIButtonControl btnNewest = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnGenres = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnSearch = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnMyRentals = null;
    [SkinControlAttribute(8)]
    protected GUIButtonControl btnFavorites = null;
    [SkinControlAttribute(9)]
    protected GUIButtonControl btnOnlyFree = null;
    [SkinControlAttribute(10)]
    protected GUICheckButton btnOnlyHD = null;
    [SkinControlAttribute(11)]
    protected GUIButtonControl btnLogOut = null;
    [SkinControlAttribute(12)]
    protected GUIButtonControl btnLogIn = null;
    [SkinControlAttribute(50)]
    protected GUIFacadeControl facadeLayout = null;
    #endregion

    public HeadWeb()
    {
    }

    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return "HeadWeb";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Headweb is a Swedish online video store. For more information, visit http://www.headweb.com";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "pilehave";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      HeadWebConfig config = new HeadWebConfig();
      config.ShowDialog();
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // Get Windows-ID
    public int GetWindowId()
    {
      // WindowID of windowplugin belonging to this setup
      // enter your own unique code
      return 7771;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return true;
    }

    // indicates if a plugin has it's own setup screen
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// If the plugin should have it's own button on the main menu of MediaPortal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true : plugin needs it's own button on home
    /// false : plugin does not need it's own button on home</returns>

    public bool GetHome(out string strButtonText, out string strButtonImage,
      out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = PluginName();
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return true;
    }

    public override string GetModuleName()
    {
      return "HeadWeb";
    }

    // With GetID it will be an window-plugin / otherwise a process-plugin
    // Enter the id number here again
    public override int GetID
    {
      get
      {
        return 7771;
      }

      set
      {
      }
    }
    #endregion

    // Init the skin
    public override bool Init()
    {
      // Load the skin-file
      return Load(GUIGraphicsContext.Skin + @"\HeadWeb.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (!firstLoadDone) DoFirstLoad();

      if (OnlyHD)
      {
        btnOnlyHD.Selected = true;
      }

      Log.Info("HeadWeb: OnPageLoad()");
      if (isGenreContentShown)
      {
        if (rememberGenreId == 128535)
        {
          getContent("genre/" + rememberGenreId);
        }
        else
        {
          getContent("genre/" + rememberGenreId + "/toprate");
        }

        isGenreContent = true;
      }
      else
      {
        switch ((Show)showSettings.ShowAs)
        {
          case Show.BestSellers:
            getContent("content/bestsell");
            break;
          case Show.Genres:
            getGenres();
            break;
          case Show.Newest:
            getContent("content/latest");
            break;
          case Show.Search:
            getSearch();
            break;
          case Show.Rentals:
            getRentals();
            break;
          case Show.Favorites:
            getFavorites(false);
            break;
          case Show.Free:
            OnlyFree = true;
            getContent("content");
            break;
        }
      }

      SwitchLayout();
      UpdateButtonStates();
    }

    private void DoFirstLoad()
    {
      // Set layout to Listview
      facadeLayout.CurrentLayout = GUIFacadeControl.Layout.List;

      // Load settings from XML-file
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "HeadWeb.xml")))
      {
        HeadWebSettings.Instance.s_username = xmlreader.GetValueAsString("Settings", "Username", "");
        HeadWebSettings.Instance.s_password = xmlreader.GetValueAsString("Settings", "Password", "");
        HeadWebSettings.Instance.s_pincode = xmlreader.GetValueAsString("Settings", "PINCode", "");
        HeadWebSettings.Instance.s_video_quality = xmlreader.GetValueAsString("Settings", "VideoQuality", "");

        if (xmlreader.GetValueAsInt("Settings", "AutoLogin", 0) != 0) HeadWebSettings.Instance.s_autologin = true;
        if (xmlreader.GetValueAsInt("Settings", "PinApprovePurchase", 0) != 0) HeadWebSettings.Instance.s_pinapprovepurchase = true;
        if (xmlreader.GetValueAsInt("Settings", "DisableAdultMovies", 0) != 0) HeadWebSettings.Instance.s_disableadultmovies = true;
        if (xmlreader.GetValueAsInt("Settings", "UseOrginalName", 0) != 0) HeadWebSettings.Instance.s_useoriginalname = true;
      }

      // Try to log in (if auto-login is enabled)
      if (HeadWebSettings.Instance.s_autologin)
      {
        bool loginresult = loginUser(HeadWebSettings.Instance.s_username, HeadWebSettings.Instance.s_password);
        if (loginresult)
        {
          HeadWebSettings.Instance.s_loggedin = true;

          // Load favorites
          getFavorites(true);
        }
      }

      // Add a special reversed proxy handler for rtmp
      ReverseProxy.Instance.AddHandler(RTMP_LIB.RTMPRequestHandler.Instance);

      // Replace g_player's ShowFullScreenWindowVideo
      g_Player.ShowFullScreenWindowVideo = ShowFullScreenWindowHandler;
      g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);

      // Load translated strings into GUI
      foreach (string name in Translation.Strings.Keys)
      {
        GUIPropertyManager.SetProperty("#HeadWeb.Translation." + name + ".Label", Translation.Strings[name]);
      }

      // First load done, don't do it again
      firstLoadDone = true;
    }

    public override void OnAction(Action action)
    {
      int actwin = GUIWindowManager.ActiveWindowEx;
      GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            if (isGenreContent)
            {
              rememberIndex = rememberGenreIndex;
              getGenres();
              isGenreContentShown = false;
            }
            else
            {
              base.OnAction(action);
            }
            break;
          }
        default:
          base.OnAction(action);
          break;
      }

    }

    void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
    {
    }

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowVideo
    /// </summary>
    /// <returns></returns>
    private static bool ShowFullScreenWindowHandler()
    {
      if (g_Player.HasVideo && (g_Player.Player.GetType().Assembly == typeof(HeadWeb).Assembly))
      {
        if (GUIWindowManager.ActiveWindow == GUIOnlineVideoFullscreen.WINDOW_FULLSCREEN_ONLINEVIDEO) return true;

        Log.Info("HeadWeb: ShowFullScreenWindow switching to fullscreen");
        GUIWindowManager.ActivateWindow(GUIOnlineVideoFullscreen.WINDOW_FULLSCREEN_ONLINEVIDEO);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return g_Player.ShowFullScreenWindowVideoDefault();
    }

    // Log in user with SSL+POST request
    public bool loginUser(string username = null, string password = null)
    {
      Log.Info("HeadWeb: loginUser()");
      try
      {
        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl("user/login", "");
        string postRequestParams = "username=" + username + "&password=" + password;

        // Clear web-cache by setting cache to 0
        int tempCacheTime = HeadWebSettings.Instance.CacheTimeout;
        HeadWebSettings.Instance.CacheTimeout = 0;

        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "POST", postRequestParams, true);
        Log.Debug("HeadWeb: Login result=" + rootXML);
        HeadWebSettings.Instance.CacheTimeout = tempCacheTime;
        doc.LoadXml(rootXML);

        if (rootXML.IndexOf("headweb.error") > -1)
        {
          XmlNode error_node = doc.SelectSingleNode("//response/error");
          String error = error_node.InnerText;
          Log.Info("HeadWeb: Error response: " + error);

          GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify != null)
          {
            // Show notify dialog
            dlgNotify.SetHeading(Translation.InvalidUsernameOrPassword);
            dlgNotify.SetText(error);
            dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          }
          return false;
        }

        XmlNode user_node = doc.SelectSingleNode("response/user");
        if (user_node["username"].InnerText != "")
          {
            Log.Info("HeadWeb: User " + user_node["username"].InnerText + " logged in");
            HeadWebSettings.Instance.s_username = user_node["username"].InnerText;
            HeadWebSettings.Instance.s_firstname = user_node["firstname"].InnerText;
            HeadWebSettings.Instance.s_lastname = user_node["lastname"].InnerText;
            HeadWebSettings.Instance.s_balance = user_node["balance"].InnerText;
            return true;
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: loginUser() caused an exeption" + e.Message);
      }

      return false;
    }

    // Get movies in a list
    private void getContent(string cPath = "")
    {

      List<String> filters = new List<String>();
      string finalfilters = string.Empty;

      filters.Add("stream[flash]");

      if (OnlyFree)
      {
        filters.Add("maxprice[0.0]");
      }
      if (OnlyHD)
      {
        filters.Add("hd");
      }
      if (HeadWebSettings.Instance.s_disableadultmovies)
      {
        filters.Add("-adult");
      }
        string s = string.Join(",", filters.ToArray() as string[]);
        finalfilters = "/filter(" + s + ")";

      Log.Info("HeadWeb: getContent() for " + cPath + finalfilters);
      try
      {
        isGenre = false;
        isGenreContent = false;
        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl(cPath + finalfilters, "fields=name,plot,year,rating,genre,stream,cover,director&limit=40");
        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null);
        doc.LoadXml(rootXML);
        XmlElement root = doc.DocumentElement;
        facadeLayout.Clear();
        mInfo.Clear();
        GUIListItem item = null;
        MovieInfo movieinfo = null;

        XmlNodeList list = root.SelectNodes("/response/list/content");
        foreach (XmlNode elm in list)
        {
          string best = Utils.getBestCover(elm);
          item = new GUIListItem();
          item.ItemId = Int32.Parse(elm.Attributes["id"].Value);
          movieinfo = new MovieInfo();
          movieinfo.MovieID = item.ItemId;
          if (HeadWebSettings.Instance.s_useoriginalname)
          {
            try
            {
              item.Label = elm["originalname"].InnerText;
              if (item.Label.Length <= 0)
              {
                item.Label = elm["name"].InnerText;
              }
              HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label);
            }
            catch
            {
              try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
              catch { }
            }
          }
          else
          {
            try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
            catch { }
          }

          item.ThumbnailImage = best;

          XmlNode streamnode_type = elm.SelectSingleNode("stream/type");
          string stream_type = string.Empty;
          try { stream_type = streamnode_type.InnerText; }
          catch { }

          XmlNode pricenode = elm.SelectSingleNode("stream/price");
          try { item.Label2 = pricenode.InnerText; }
          catch { }

          try { item.Year = Int32.Parse(elm["year"].InnerText); }
          catch { }

          XmlNode runtimenode = elm.SelectSingleNode("stream/runtime");
          string runtime = string.Empty;
          try { item.Duration = Int32.Parse(runtimenode.InnerText); }
          catch { }

          try { item.Rating = float.Parse(elm["rating"].InnerText); }
          catch { }

          try { movieinfo.Plot = elm["plot"].InnerText; }
          catch { }

          try
          {
            string finalDirector = string.Empty;
            string[] list_directors = new string[] { };
            List<string> directorlist = new List<string>();

            XmlNodeList directornode = elm.SelectNodes("director/person");
            foreach (XmlNode directorelm in directornode)
            {
              string newDirector = directorelm.InnerText;
              directorlist.Add(newDirector);
            }
            list_directors = directorlist.ToArray();
            finalDirector = string.Join(", ", list_directors);
            movieinfo.Directors = finalDirector;
          }
          catch { }

          try
          {
            string finalGenre = string.Empty;
            string[] list_genres = new string[] { };
            List<string> genrelist = new List<string>();

            XmlNodeList genrenode = elm.SelectNodes("genre");
            foreach (XmlNode genreelm in genrenode)
            {
              string newGenre = genreelm.InnerText;
              genrelist.Add(newGenre);
            }
            list_genres = genrelist.ToArray();
            finalGenre = string.Join(", ", list_genres);
            movieinfo.Genres = finalGenre;
          }
          catch { }

          if (stream_type == "flash")
          {
            facadeLayout.Add(item);
            mInfo.Add(movieinfo);

          }
        }
        int iTotalItems = facadeLayout.Count;
        GUIPropertyManager.SetProperty("#itemcount", iTotalItems.ToString());
        if (rememberIndex > -1)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, rememberIndex);
        }
        else
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
        }

      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getContent() caused an exception " + e.Message);
      }
    }

    // Get genres in a list
    private void getGenres()
    {
      Log.Info("HeadWeb: getGenres()");
      try
      {
        isGenre = true;
        isGenreContent = false;
        XmlDocument doc = new XmlDocument();
        string requestUrl = string.Empty;
        GUIPropertyManager.SetProperty("#year", string.Empty);
        GUIPropertyManager.SetProperty("#runtime", string.Empty);
        GUIPropertyManager.SetProperty("#rating", string.Empty);
        GUIPropertyManager.SetProperty("#director", string.Empty);
        GUIPropertyManager.SetProperty("#genre", string.Empty);
        GUIPropertyManager.SetProperty("#plot", string.Empty);

        if (HeadWebSettings.Instance.s_disableadultmovies)
        {
          requestUrl = Utils.constructUrl("genre/filter(-adult)", "");
        }
        else
        {
          requestUrl = Utils.constructUrl("genre", "");
        }

        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null);
        doc.LoadXml(rootXML);
        XmlElement root = doc.DocumentElement;
        facadeLayout.Clear();
        GUIListItem item = null;
        XmlNodeList list = root.SelectNodes("/response/list/genre");
        foreach (XmlNode elm in list)
        {
          item = new GUIListItem();
          item.ItemId = Int32.Parse(elm.Attributes["id"].Value);
          item.Label = elm.InnerText;
          facadeLayout.Add(item);
        }
        int iTotalItems = facadeLayout.Count;
        GUIPropertyManager.SetProperty("#itemcount", iTotalItems.ToString());
        if (rememberIndex > -1)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, rememberIndex);
        }
        else
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getGenres() caused an exception " + e.Message);
      }
    }

    private void getSearch()
    {

      List<String> filters = new List<String>();
      string finalfilters = string.Empty;

      filters.Add("stream[flash]");

      if (OnlyFree)
      {
        filters.Add("maxprice[0.0]");
      }
      if (OnlyHD)
      {
        filters.Add("hd");
      }
      if (HeadWebSettings.Instance.s_disableadultmovies)
      {
        filters.Add("-adult");
      }
        string s = string.Join(",", filters.ToArray() as string[]);
        finalfilters = "/filter(" + s + ")";

      Log.Info("HeadWeb: getSearch()");
      try
      {
        isGenre = false;
        isGenreContent = false;
        string searchString = HeadWebSettings.Instance.s_search_history;
        if (rememberIndex <= -1)
        {
          GetKeyboard(ref searchString, "Search");
        }
        if (searchString.Length > 0)
        {
          HeadWebSettings.Instance.s_search_history = searchString;
          XmlDocument doc = new XmlDocument();
          string requestUrl = Utils.constructUrl("search/" + System.Web.HttpUtility.UrlEncode(searchString) + finalfilters, "");
          Log.Debug("HeadWeb: Search: " + searchString);
          string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null);
          doc.LoadXml(rootXML);
          XmlElement root = doc.DocumentElement;
          facadeLayout.Clear();
          mInfo.Clear();
          GUIListItem item = null;
          MovieInfo movieinfo = null;
          XmlNodeList list = root.SelectNodes("/response/list/content");
          foreach (XmlNode elm in list)
          {
            string best = Utils.getBestCover(elm);
            item = new GUIListItem();
            item.ItemId = Int32.Parse(elm.Attributes["id"].Value);

            if (HeadWebSettings.Instance.s_useoriginalname)
            {
              try
              {
                item.Label = elm["originalname"].InnerText;
                if (item.Label.Length <= 0)
                {
                  item.Label = elm["name"].InnerText;
                }
                HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label);
              }
              catch
              {
                try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
                catch { }
              }
            }
            else
            {
              try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
              catch { }
            }

            item.ThumbnailImage = best;

            XmlNode streamnode_type = elm.SelectSingleNode("stream/type");
            string stream_type = string.Empty;
            try { stream_type = streamnode_type.InnerText; }
            catch { }

            XmlNode pricenode = elm.SelectSingleNode("stream/price");
            try { item.Label2 = pricenode.InnerText; }
            catch { }

            try { item.Year = Int32.Parse(elm["year"].InnerText); }
            catch { }

            XmlNode runtimenode = elm.SelectSingleNode("stream/runtime");
            string runtime = string.Empty;
            try { item.Duration = Int32.Parse(runtimenode.InnerText); }
            catch { }

            try { item.Rating = float.Parse(elm["rating"].InnerText); }
            catch { }

            try { movieinfo.Plot = elm["plot"].InnerText; }
            catch { }

            if (stream_type == "flash")
            {
              facadeLayout.Add(item);
              mInfo.Add(movieinfo);
            }

          }
          int iTotalItems = facadeLayout.Count;
          GUIPropertyManager.SetProperty("#itemcount", iTotalItems.ToString());
          if (rememberIndex > -1)
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, rememberIndex);
          }
          else
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getSearch() caused an exception " + e.Message);
      }
    }

    private void getRentals()
    {
      Log.Info("HeadWeb: getRentals()");
      try
      {
        isGenre = false;
        isGenreContent = false;
        int runtime_hours = 0;
        int runtime_minutes = 0;
        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl("user/rentals", "fields=name,plot,year,rating,genre,stream,cover,director&limit=40");
        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null, false, false);
        doc.LoadXml(rootXML);
        XmlElement root = doc.DocumentElement;
        facadeLayout.Clear();
        mInfo.Clear();
        GUIListItem item = null;
        MovieInfo movieinfo = null;
        XmlNodeList list = root.SelectNodes("/response/list/item");
        foreach (XmlNode elm in list)
        {
          XmlNode node = elm.SelectSingleNode("content");
          string best = Utils.getBestCover(node);
          item = new GUIListItem();
          item.ItemId = Int32.Parse(node.Attributes["id"].Value);
          movieinfo = new MovieInfo();
          movieinfo.MovieID = item.ItemId;

          if (HeadWebSettings.Instance.s_useoriginalname)
          {
            try
            {
              item.Label = elm["originalname"].InnerText;
              if (item.Label.Length <= 0)
              {
                item.Label = elm["name"].InnerText;
              }
              HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label);
            }
            catch
            {
              try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
              catch { }
            }
          }
          else
          {
            try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
            catch { }
          }

          item.ThumbnailImage = best;



          item.Label2 = Translation.Expired;
          try
          {
            DateTime utcDateTime_rentalexpires = DateTime.Parse(elm["expires"].InnerText);
            DateTime utcDateTime_now = DateTime.Now;
            if (utcDateTime_rentalexpires > utcDateTime_now)
            {
              int time = Int32.Parse(elm["timeleft"].InnerText);
              runtime_hours = (time / 3600);
              runtime_minutes = ((time / 60) % 60);
              item.Label2 = runtime_hours.ToString() + "h " + runtime_minutes.ToString() + "m";
            }
          }
          catch { }

          try { item.Year = Int32.Parse(node["year"].InnerText); }
          catch { }

          XmlNode runtimenode = node.SelectSingleNode("stream/runtime");
          string runtime = string.Empty;

          try { item.Duration = Int32.Parse(runtimenode.InnerText); }
          catch { }

          XmlNode ratingnode = elm.SelectSingleNode("content/rating");
          string rating = string.Empty;
          try { item.Rating = float.Parse(ratingnode.InnerText); }
          catch { }

          XmlNode plotnode = elm.SelectSingleNode("content/plot");
          string plot = string.Empty;
          try { movieinfo.Plot = plotnode.InnerText; }
          catch { }

          try
          {
            string finalDirector = string.Empty;
            string[] list_directors = new string[] { };
            List<string> directorlist = new List<string>();

            XmlNodeList directornode = node.SelectNodes("director/person");
            foreach (XmlNode directorelm in directornode)
            {
              string newDirector = directorelm.InnerText;
              directorlist.Add(newDirector);
            }
            list_directors = directorlist.ToArray();
            if (list_directors.Length > 0)
            {
              finalDirector = string.Join(", ", list_directors);
            }
            movieinfo.Directors = finalDirector;
          }
          catch { }

          try
          {
            string finalGenre = string.Empty;
            string[] list_genres = new string[] { };
            List<string> genrelist = new List<string>();

            XmlNodeList genrenode = node.SelectNodes("genre");
            foreach (XmlNode genreelm in genrenode)
            {
              string newGenre = genreelm.InnerText;
              genrelist.Add(newGenre);
            }
            list_genres = genrelist.ToArray();
            if (list_genres.Length > 0)
            {
              finalGenre = string.Join(", ", list_genres);
            }
            movieinfo.Genres = finalGenre;
          }
          catch { }

          facadeLayout.Add(item);
          mInfo.Add(movieinfo);
        }
        int iTotalItems = facadeLayout.Count;
        GUIPropertyManager.SetProperty("#itemcount", iTotalItems.ToString());
        if (rememberIndex > -1)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, rememberIndex);
        }
        else
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getRentals() caused an exception " + e.Message);
      }
    }

    public void getFavorites(bool storeOnly)
    {
      Log.Info("HeadWeb: getFavorites()");
      try
      {
        isGenre = false;
        isGenreContent = false;
        XmlDocument doc = new XmlDocument();
        string requestUrl = Utils.constructUrl("user/watchlist", "");

        // Clear web-cache by setting cache to 0
        int tempCacheTime = HeadWebSettings.Instance.CacheTimeout;
        HeadWebSettings.Instance.CacheTimeout = 0;

        string rootXML = Utils.GetWebData(requestUrl, HeadWebSettings.Instance.s_cc, null, null, false, false, null, "GET", null);
        doc.LoadXml(rootXML);
        XmlElement root = doc.DocumentElement;
        mInfo.Clear();
        if (!storeOnly)
        {
          facadeLayout.Clear();
        }
          GUIListItem item = null;
          MovieInfo movieinfo = null;
        XmlNodeList list = root.SelectNodes("/response/list/content");
        foreach (XmlNode elm in list)
        {
          string best = Utils.getBestCover(elm);
          item = new GUIListItem();
          movieinfo = new MovieInfo();
          item.ItemId = Int32.Parse(elm.Attributes["id"].Value);
          if (!storeOnly)
          {

            if (HeadWebSettings.Instance.s_useoriginalname)
            {
              try
              {
                item.Label = elm["originalname"].InnerText;
                if (item.Label.Length <= 0)
                {
                  item.Label = elm["name"].InnerText;
                }
                HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label);
              }
              catch
              {
                try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
                catch { }
              }
            }
            else
            {
              try { item.Label = elm["name"].InnerText; HeadWebSettings.Instance.s_movietitle = Utils.makeSafeFilename(item.Label); }
              catch { }
            }


            item.ThumbnailImage = best;

            XmlNode streamnode_type = elm.SelectSingleNode("stream/type");
            string stream_type = string.Empty;
            try { stream_type = streamnode_type.InnerText; }
            catch { }

            XmlNode pricenode = elm.SelectSingleNode("stream/price");
            try { item.Label2 = pricenode.InnerText; }
            catch { }

            try { item.Year = Int32.Parse(elm["year"].InnerText); }
            catch { }

            XmlNode runtimenode = elm.SelectSingleNode("stream/runtime");
            string runtime = string.Empty;
            try { item.Duration = Int32.Parse(runtimenode.InnerText); }
            catch { }

            try { item.Rating = float.Parse(elm["rating"].InnerText); }
            catch { }

            try { movieinfo.Plot = elm["plot"].InnerText; }
            catch { }

            facadeLayout.Add(item);
            mInfo.Add(movieinfo);
          }
          HeadWebSettings.Instance.s_favorit.Add(item.ItemId);
        }
        int iTotalItems = facadeLayout.Count;
        GUIPropertyManager.SetProperty("#itemcount", iTotalItems.ToString());
        if (rememberIndex > -1)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, rememberIndex);
        }
        else
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
        }
      }
      catch (Exception e)
      {
        Log.Error("HeadWeb: getFavorites() caused an exception " + e.Message);
      }
    }

    public void UpdateGui()
    {
      if (HeadWebSettings.Instance.s_loggedin)
      {
        GUIPropertyManager.SetProperty("#HeadWeb.UserName", HeadWebSettings.Instance.s_username);
        GUIPropertyManager.SetProperty("#HeadWeb.FirstName", HeadWebSettings.Instance.s_firstname);
        GUIPropertyManager.SetProperty("#HeadWeb.LastName", HeadWebSettings.Instance.s_lastname);
        GUIPropertyManager.SetProperty("#HeadWeb.Balance", HeadWebSettings.Instance.s_balance);
        GUIControl.EnableControl(GetID, btnMyRentals.GetID);
        GUIControl.EnableControl(GetID, btnFavorites.GetID);
        GUIControl.EnableControl(GetID, btnLogOut.GetID);
        GUIControl.DisableControl(GetID, btnLogIn.GetID);
      }
      else
      {
        GUIPropertyManager.SetProperty("#HeadWeb.UserName", Translation.NotLoggedIn);
        GUIPropertyManager.SetProperty("#HeadWeb.FirstName", string.Empty);
        GUIPropertyManager.SetProperty("#HeadWeb.LastName", string.Empty);
        GUIPropertyManager.SetProperty("#HeadWeb.Balance", string.Empty);
        GUIControl.DisableControl(GetID, btnMyRentals.GetID);
        GUIControl.DisableControl(GetID, btnFavorites.GetID);
        GUIControl.DisableControl(GetID, btnLogOut.GetID);
        GUIControl.EnableControl(GetID, btnLogIn.GetID);
      }

      int iItem = facadeLayout.SelectedListItemIndex;
      if (!isGenre && iItem > -1)
      {
        MovieInfo movieinfo = mInfo[iItem];

        GUIPropertyManager.SetProperty("#year", facadeLayout[iItem].Year.ToString());
        GUIPropertyManager.SetProperty("#runtime", (Math.Round((double)facadeLayout[iItem].Duration / 60)).ToString());
        GUIPropertyManager.SetProperty("#rating", (facadeLayout[iItem].Rating / 5).ToString());
        GUIPropertyManager.SetProperty("#director", movieinfo.Directors);
        GUIPropertyManager.SetProperty("#genre", movieinfo.Genres);
        GUIPropertyManager.SetProperty("#plot", movieinfo.Plot);

      }
    }

    private bool getLogin()
    {
      // Check if skin-file for Login dialog exists
      if (System.IO.File.Exists(GUIGraphicsContext.Skin + @"\HeadWebLogin.xml"))
      {
        GUILogin loginDlg = (GUILogin)GUIWindowManager.GetWindow(GUILogin.ID);

        // Initialize Dialog
        if (loginDlg != null)
        {
          loginDlg.Reset();
          loginDlg.SetHeading(Translation.LogInToHeadWeb);
          loginDlg.DoModal(GUIWindowManager.ActiveWindow);
        }

        if (loginDlg.IsUsernameButtonPressed)
        {
          string enteredUsername = string.Empty;
          if (GetKeyboard(ref enteredUsername, Translation.EnterUsername))
          {
            HeadWebSettings.Instance.s_username = enteredUsername;
            getLogin();
          }
        }

        if (loginDlg.IsPasswordButtonPressed)
        {
          string enteredPassword = string.Empty;
          if (GetKeyboard(ref enteredPassword, Translation.EnterPassword))
          {
            HeadWebSettings.Instance.s_password = enteredPassword;
            getLogin();
          }
        }

      }
      return true;
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {

      if (control == facadeLayout) // Clicked on something in the facade
      {
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM) // Execute only for enter keys
        {
          if (isGenreContent)
          {
            isGenreContentShown = true;
          }
          if (isGenre)
          {
            isGenreContent = true;
            isGenreContentShown = false;
          }
          if (isGenre) // Genres displayed, get content
          {
            rememberIndex = -1;
            rememberGenreIndex = facadeLayout.SelectedListItemIndex;
            rememberGenreId = facadeLayout.SelectedListItem.ItemId;
            if (rememberGenreId == 128535)
            {
              getContent("genre/" + facadeLayout.SelectedListItem.ItemId);
            }
            else
            {
              getContent("genre/" + facadeLayout.SelectedListItem.ItemId + "/toprate");
            }
            isGenreContent = true;
          }
          else
          {
            isGenre = false;
            //rememberGenreIndex = -1;
            rememberIndex = facadeLayout.SelectedListItemIndex;
            HeadWebMovieInfo dlgMovieInfo = (HeadWebMovieInfo)GUIWindowManager.GetWindow(7772);
            if (dlgMovieInfo != null)
            {
              dlgMovieInfo.movieId = facadeLayout.SelectedListItem.ItemId;
              dlgMovieInfo.thumbnailUrl = facadeLayout.SelectedListItem.ThumbnailImage;
              GUIWindowManager.ActivateWindow(7772); // Show movie info
            }
          }
        }
      }

      else if (control == btnSwitchView) // Switch layout
      {
        OnShowLayouts();
        SelectCurrentItem();
        UpdateButtonStates();
        GUIControl.FocusControl(GetID, controlId);
      }

      else if (control == btnBestSellers) // Load default view
      {
        showSettings.ShowAs = (int)Show.BestSellers;
        rememberIndex = -1;
        getContent("content/bestsell");
        isGenreContentShown = false;
        OnlyFree = false;
      }

      else if (control == btnNewest) // Load newest
      {
        showSettings.ShowAs = (int)Show.Newest;
        rememberIndex = -1;
        rememberGenreIndex = -1;
        getContent("content/latest");
        isGenreContentShown = false;
        OnlyFree = false;
      }

      else if (control == btnGenres) // Load genres
      {
        showSettings.ShowAs = (int)Show.Genres;
        rememberIndex = -1;
        rememberGenreIndex = -1;
        getGenres();
        OnlyFree = false;
      }

      else if (control == btnSearch) // Prompt for search input
      {
        showSettings.ShowAs = (int)Show.Search;
        rememberIndex = -1;
        rememberGenreIndex = -1;
        getSearch();
        isGenreContentShown = false;
        OnlyFree = false;
      }

      else if (control == btnMyRentals) // Load active rentals
      {
        showSettings.ShowAs = (int)Show.Rentals;
        rememberIndex = -1;
        rememberGenreIndex = -1;
        getRentals();
        isGenreContentShown = false;
        OnlyFree = false;
      }

      else if (control == btnFavorites) // Load favorites
      {
        showSettings.ShowAs = (int)Show.Favorites;
        rememberIndex = -1;
        rememberGenreIndex = -1;
        getFavorites(false);
        isGenreContentShown = false;
        OnlyFree = false;
      }

      else if (control == btnOnlyFree) // Filter free
      {
        showSettings.ShowAs = (int)Show.Newest;
        rememberIndex = -1;
        rememberGenreIndex = -1;
        OnlyFree = true;
        getContent("content");
        isGenreContentShown = false;
      }

      else if (control == btnOnlyHD) // Filter HD
      {

        //get state of button
        if (btnOnlyHD.Selected)
        {
          OnlyHD = true;
        }
        else
        {
          OnlyHD = false;
        }

      }

      else if (control == btnLogOut) // Log out
      {
        HeadWebSettings.Instance.s_cc = null;
        HeadWebSettings.Instance.s_loggedin = false;
        HeadWebSettings.Instance.s_favorit.Clear();
        //UpdateGui();
        //showSettings.ShowAs = (int)Show.Newest;
        //rememberIndex = -1;
        //getFavorites(false);
        //isGenreContentShown = false;
        OnlyFree = false;
      }

      else if (control == btnLogIn) // Log in
      {
        getLogin();
        OnlyFree = false;
      }

      base.OnClicked(controlId, control, actionType);
    }

    public override void Process()
    {
      // Update the gui
      UpdateGui();
      base.Process();
    }

    protected virtual void SelectCurrentItem()
    {
      if (facadeLayout == null)
      {
        return;
      }
      int iItem = facadeLayout.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iItem);
      }
    }

    protected virtual void UpdateButtonStates()
    {
      if (handler != null)
      {
        GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
        GUIPropertyManager.SetProperty("#itemtype", handler.LocalizedCurrentViewLevel);
      }

      if (facadeLayout == null)
      {
        return;
      }

      GUIControl.HideControl(GetID, facadeLayout.GetID);
      int iControl = facadeLayout.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);


      string strLine = string.Empty;
      GUIFacadeControl.Layout layout = CurrentLayout;
      switch (layout)
      {
        case Layout.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case Layout.SmallIcons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case Layout.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
        case Layout.AlbumView:
          strLine = GUILocalizeStrings.Get(529);
          break;
        case Layout.Filmstrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
        case Layout.Playlist:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case Layout.CoverFlow:
          strLine = GUILocalizeStrings.Get(791);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSwitchView.GetID, strLine);
    }

    protected virtual Layout GetLayoutNumber(string s)
    {
      switch (s.Trim().ToLower())
      {
        case "list":
          return Layout.List;
        case "icons":
        case "smallicons":
          return Layout.SmallIcons;
        case "big icons":
        case "largeicons":
          return Layout.LargeIcons;
        case "albums":
        case "albumview":
          return Layout.AlbumView;
        case "filmstrip":
          return Layout.Filmstrip;
        case "playlist":
          return Layout.Playlist;
        case "coverflow":
        case "cover flow":
          return Layout.CoverFlow;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("{0}::GetLayoutNumber: Unknown String - {1}", "WindowPluginBase", s);
      }
      return Layout.List;
    }

    protected virtual void OnShowLayouts()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(792); // Layouts menu
      int dlgItems = 0;
      int totalLayouts = Enum.GetValues(typeof(GUIFacadeControl.Layout)).Length;
      bool[] allowedLayouts = new bool[totalLayouts];
      for (int i = 0; i < totalLayouts; i++)
      {
        string layoutName = Enum.GetName(typeof(GUIFacadeControl.Layout), i);
        GUIFacadeControl.Layout layout = GetLayoutNumber(layoutName);
        if (AllowLayout(layout))
        {
          if (!facadeLayout.IsNullLayout(layout))
          {
            dlg.Add(GUIFacadeControl.GetLayoutLocalizedName(layout));
            dlgItems++;
            allowedLayouts[i] = true;
          }
        }
      }
      dlg.SelectedLabel = -1;
      for (int i = 0; i <= (int)CurrentLayout; i++)
      {
        if (allowedLayouts[i])
        {
          dlg.SelectedLabel++;
        }
      }
      if (dlg.SelectedLabel >= dlgItems)
      {
        dlg.SelectedLabel = dlgItems;
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }
      int iSelectedLayout = dlg.SelectedLabel;
      int allowedItemsFound = -1;
      for (int i = 0; i < allowedLayouts.Length; i++)
      {
        if (allowedLayouts[i])
        {
          iSelectedLayout = i;
          allowedItemsFound++;
          if (allowedItemsFound == dlg.SelectedLabel)
            break;
        }
      }
      CurrentLayout = (Layout)iSelectedLayout;
      SwitchLayout();

      UpdateButtonStates();
    }

    protected virtual bool AllowLayout(Layout layout)
    {
      if (layout == Layout.AlbumView || layout == Layout.Playlist)
      {
        return false;
      }
      return true;
    }

    protected virtual void SwitchLayout()
    {
      if (facadeLayout == null)
      {
        return;
      }

      // if skin has not implemented layout control or requested layout is not allowed
      // then default to list layout
      if (facadeLayout.IsNullLayout(CurrentLayout) || !AllowLayout(CurrentLayout))
      {
        facadeLayout.CurrentLayout = Layout.List;
      }
      else
      {
        facadeLayout.CurrentLayout = CurrentLayout;
      }

    }

    protected virtual bool GetKeyboard(ref string strLine, string strLabel)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.Label = strLabel;
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
