using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace HeadWeb
{
  public static class Translation
  {
    #region Private variables

    private static Dictionary<string, string> _translations;
    private static readonly string _path = string.Empty;
    private static readonly DateTimeFormatInfo _info;

    #endregion

    #region Constructor

    static Translation()
    {
      string lang;

      try
      {
        lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
        _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      }
      catch (Exception)
      {
        lang = CultureInfo.CurrentUICulture.Name;
        _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      }

      Log.Info("HeadWeb: Using language " + lang);

      _path = Config.GetSubFolder(Config.Dir.Language, "HeadWeb");

      if (!System.IO.Directory.Exists(_path))
        System.IO.Directory.CreateDirectory(_path);

      LoadTranslations(lang);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the translated strings collection in the active language
    /// </summary>
    public static Dictionary<string, string> Strings
    {
      get
      {
        if (_translations == null)
        {
          _translations = new Dictionary<string, string>();
          Type transType = typeof(Translation);
          FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
          foreach (FieldInfo field in fields)
          {
            _translations.Add(field.Name, field.GetValue(transType).ToString());
          }
        }
        return _translations;
      }
    }

    #endregion

    #region Public Methods

    public static int LoadTranslations(string lang)
    {
      XmlDocument doc = new XmlDocument();
      Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
      string langPath = "";
      try
      {
        langPath = Path.Combine(_path, lang + ".xml");
        doc.Load(langPath);
      }
      catch (Exception e)
      {
        if (lang == "en")
          return 0; // otherwise we are in an endless loop!

        if (e.GetType() == typeof(FileNotFoundException))
          Log.Warn("HeadWeb: Cannot find translation file {0}.  Failing back to English", langPath);
        else
        {
          Log.Error("HeadWeb: Error in translation xml file: {0}. Failing back to English", lang);
          Log.Error(e);
        }

        return LoadTranslations("en");
      }
      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        if (stringEntry.NodeType == XmlNodeType.Element)
          try
          {
            TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
          }
          catch (Exception ex)
          {
            Log.Error("HeadWeb: Error in Translation Engine");
            Log.Error(ex);
          }
      }

      Type TransType = typeof(Translation);
      FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
      foreach (FieldInfo fi in fieldInfos)
      {
        if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
          TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
        else
          Log.Info("HeadWeb: Translation not found for field: {0}.  Using hard-coded English default.", fi.Name);
      }
      return TranslatedStrings.Count;
    }

    public static string GetByName(string name)
    {
      if (!Strings.ContainsKey(name))
        return name;

      return Strings[name];
    }

    public static string GetByName(string name, params object[] args)
    {
      return String.Format(GetByName(name), args);
    }

    /// <summary>
    /// Takes an input string and replaces all ${named} variables with the proper translation if available
    /// </summary>
    /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
    /// <returns>translated input string</returns>
    public static string ParseString(string input)
    {
      Regex replacements = new Regex(@"\$\{([^\}]+)\}");
      MatchCollection matches = replacements.Matches(input);
      foreach (Match match in matches)
      {
        input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
      }
      return input;
    }




    public static string GetDayName(DayOfWeek dayOfWeek)
    {
      return _info.GetDayName(dayOfWeek);
    }
    public static string GetShortestDayName(DayOfWeek dayOfWeek)
    {
      return _info.GetShortestDayName(dayOfWeek);
    }

    #endregion

    #region Translations / Strings

    /// <summary>
    /// These will be loaded with the language files content
    /// if the selected lang file is not found, it will first try to load en-US.xml as a backup
    /// if that also fails it will use the hardcoded strings as a last resort.
    /// </summary>


    // A
    public static string AddToFavorites = "Add to favorites";
    public static string AddToFavoritesText = "The movie \"{0}\" was added to your favorites";
    public static string AreYouSure = "Are you sure you wish to rent this movie?";

    // B
    public static string BestSellers = "Best sellers";
    public static string Buffered = "buffered";

    // C

    // D

    // E
    public static string Expired = "Expired";
    public static string EnterPinCode = "Enter Pincode";
    public static string EnterSMSCode = "Enter SMS Code";
    public static string EnterUsername = "Enter Username";
    public static string EnterPassword = "Enter Password";

    // F
    public static string Favorites = "Favorites";

    // G
    public static string Genres = "Genres";

    // H

    // I
    public static string InvalidUsernameOrPassword = "Invalid username or password";

    // L
    public static string LogInToHeadWeb = "Login to HeadWeb";
    public static string LogIn = "Login";
    public static string LogOut = "Logout";

    // M
    public static string MyRentals = "My Rentals";
    public static string MovieWillStart = "Movie will start when buffering is done";

    // N
    public static string NotLoggedIn = "Not logged in";
    public static string Newest = "Newest";

    // O
    public static string OnlyFreeMovies = "Only free movies";
    public static string OnlyHDMovies = "Only HD movies";

    // P
    public static string Password = "Password";
    public static string PinCode = "Pincode";
    public static string PinCodeIncorrect = "Pincode incorrect";
    public static string Price = "Price";

    // R
    public static string RateMovie = "Rate movie";
    public static string RentMovie = "Rent movie";
    public static string RentMovieBySMS = "Rent movie by SMS";
    public static string RemoveFromFavorites = "Remove from favorites";
    public static string RemoveFromFavoritesText = "The movie \"{0}\" was removed from your favorites";
    public static string RentalFailed = "Rental failed";
    public static string RentalSuccessful = "Rental successful";

    // S
    public static string Search = "Search";
    public static string Set = "Set";
    public static string SMSPurchase1 = "SMS Purchase";
    public static string SMSPurchase2 = "You already purchased this movie,";
    public static string SMSPurchase3 = "to activate it, enter the key";
    public static string SMSPurchase4 = "you received in the SMS.";
    public static string SMSPurchase5 = "SMS Purchase, {0}.";
    public static string SMSPurchase6 = "To purchase this movie, send";
    public static string SMSPurchase7 = "\"{0}\" to the number \"{1}\"";
    public static string SMSPurchase8 = "You will recieve a SMS with a code.";
    public static string SelectBitrate = "Select bitrate";

    // T
    public static string ToPurchaseThisMovie = "to purchase this movie";
    public static string Title = "Title";

    // U
    public static string Username = "Username";

    // V

    // W
    public static string WatchPreview = "Watch preview";
    public static string WatchMovie = "Watch movie";
    public static string WatchTrailer = "Watch trailer";

    // Y


    #endregion

  }

}