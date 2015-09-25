using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;

namespace HeadWeb
{

  public class HeadWebSettings
  {
    public string UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; de; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3";
    public bool isPurchasePinProtected = true; // enable pin by default -> purchase protection
    public string ThumbsDir;
    public int CacheTimeout = 30; // minutes
    public CultureInfo Locale;

    #region Singleton
    private static HeadWebSettings _Instance = null;
    public static HeadWebSettings Instance
    {
      get
      {
        if (_Instance == null) _Instance = new HeadWebSettings();
        return _Instance;
      }
    }
    #endregion

    static string movietitle = string.Empty;
    static string username = string.Empty;
    static string password = string.Empty;
    static string firstname = string.Empty;
    static string lastname = string.Empty;
    static string pincode = string.Empty;
    static string videoquality = string.Empty;
    static bool autologin = false;
    static bool pinapprovepurchase = false;
    static bool disableadultmovies = false;
    static bool useoriginalname = false;
    static string balance = string.Empty;
    static bool loggedin = false;
    static CookieContainer cc = new CookieContainer();
    List<int> favoritesList = new List<int>();
    string searchHistory = string.Empty;

    public string s_movietitle
    {
      get { return movietitle; }
      set { movietitle = value; }
    }

    public string s_username
    {
      get { return username; }
      set { username = value; }
    }

    public string s_password
    {
      get { return password; }
      set { password = value; }
    }

    public string s_firstname
    {
      get { return firstname; }
      set { firstname = value; }
    }

    public string s_lastname
    {
      get { return lastname; }
      set { lastname = value; }
    }

    public string s_pincode
    {
      get { return pincode; }
      set { pincode = value; }
    }

    public string s_video_quality
    {
      get { return videoquality; }
      set { videoquality = value; }
    }

    public bool s_autologin
    {
      get { return autologin; }
      set { autologin = value; }
    }

    public bool s_pinapprovepurchase
    {
      get { return pinapprovepurchase; }
      set { pinapprovepurchase = value; }
    }

    public bool s_disableadultmovies
    {
      get { return disableadultmovies; }
      set { disableadultmovies = value; }
    }

    public bool s_useoriginalname
    {
      get { return useoriginalname; }
      set { useoriginalname = value; }
    }

    public string s_balance
    {
      get { return balance; }
      set { balance = value; }
    }

    public bool s_loggedin
    {
      get { return loggedin; }
      set { loggedin = value; }
    }

    public CookieContainer s_cc
    {
      get { return cc; }
      set { cc = value; }
    }

    public List<int> s_favorit
    {
      get { return favoritesList; }
      set { favoritesList = value; }
    }

    public string s_search_history
    {
      get { return searchHistory; }
      set { searchHistory = value; }
    }

    private HeadWebSettings()
    {
      Locale = CultureInfo.CurrentUICulture;
    }
  }
}
