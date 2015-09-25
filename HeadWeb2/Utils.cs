using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Net;
using MediaPortal.GUI.Library;

namespace HeadWeb
{
  public static class Utils
  {

    /*
    Headweb API KEY.
    Please don't steal MediaPortal's key...
    Send an email to api@headweb.com and you'll get your own for free,
    documentation can be found at http://opensource.headweb.com/api
    */
    static string apiKey = "834028479a4f492ebbd1870f8e48999f";
    static string rootUrl = "http://api.headweb.com/v4/";



    public static string getBestCover(XmlNode coverList)
    {
      List<string> coverListH = new List<string>();
      List<string> coverListW = new List<string>();
      float bestArea = 0;
      string bestCover = string.Empty;

      XmlNodeList covers = coverList.SelectNodes("cover");
      foreach (XmlNode coverelm in covers)
      {
        int height = Int32.Parse(coverelm.Attributes["height"].Value);
        int width = Int32.Parse(coverelm.Attributes["width"].Value);
        int calculated = height * width;
        if (calculated > bestArea)
        {
          bestArea = calculated;
          bestCover = coverelm.InnerText;
        }
      }
      //Log.Info("HeadWeb getBestCover=" + bestCover);
      return bestCover;
    }

    public static string getGenres(XmlElement root)
    {
      string finalGenre = string.Empty;
      string[] genres = new string[] { };
      List<string> genrelist = new List<string>();

      XmlNodeList genrenode = root.SelectNodes("//genre");
      foreach (XmlNode genreelm in genrenode)
      {
        string newGenre = genreelm.InnerText;
        genrelist.Add(newGenre);
      }
      genres = genrelist.ToArray();
      finalGenre = string.Join(", ", genres);
      return finalGenre;
    }

    public static string getActors(XmlElement root)
    {
      string finalActor = string.Empty;
      string[] actors = new string[] { };
      List<string> actorlist = new List<string>();

      XmlNodeList actornode = root.SelectNodes("//actor/person");
      foreach (XmlNode actorelm in actornode)
      {
        string newGenre = actorelm.InnerText;
        actorlist.Add(newGenre);
      }
      actors = actorlist.ToArray();
      finalActor = string.Join(", ", actors);
      Log.Info("actors:" + finalActor);
      return finalActor;
    }






    public static string ToFriendlyCase(string PascalString)
    {
      return Regex.Replace(PascalString, "(?!^)([A-Z])", " $1");
    }

    public static string ReplaceEscapedUnicodeCharacter(string input)
    {
      return System.Text.RegularExpressions.Regex.Replace(
          input,
          @"(?:\\|%)[uU]([0-9A-Fa-f]{4})",
          delegate(System.Text.RegularExpressions.Match match)
          {
            return ((char)Int32.Parse(match.Value.Substring(2), System.Globalization.NumberStyles.HexNumber)).ToString();
          });
    }

    public static DateTime UNIXTimeToDateTime(double unixTime)
    {
      return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime).ToLocalTime();
    }

    public static string PlainTextFromHtml(string input)
    {
      string result = input;
      if (!string.IsNullOrEmpty(result))
      {
        // decode HTML escape character
        result = System.Web.HttpUtility.HtmlDecode(result);

        // Replace &nbsp; with space
        result = Regex.Replace(result, @"&nbsp;", " ", RegexOptions.Multiline);

        // Remove double spaces
        result = Regex.Replace(result, @"  +", "", RegexOptions.Multiline);

        // Replace <br/> with \n
        result = Regex.Replace(result, @"< *br */*>", "\n", RegexOptions.IgnoreCase & RegexOptions.Multiline);

        // Remove remaining HTML tags                
        result = Regex.Replace(result, @"<[^>]*>", "", RegexOptions.Multiline);

        // Remove whitespace at the beginning and end
        result = result.Trim();
      }
      return result;
    }

    public static string[] Tokenize(string text, bool dropToken, params string[] tokens)
    {
      if (tokens.Length > 0)
      {

        string regex = @"([";
        foreach (string s in tokens)
          regex += s;
        regex += "])";
        Regex RE = new Regex(regex);
        if (dropToken)
        {
          string output = RE.Replace(text, " ");
          return (new Regex(@"\s").Split(output));
        }
        else
          return (RE.Split(text));
      }
      return null;
    }

    /// <summary>
    /// Method to change the AllowUnsafeHeaderParsing property of HttpWebRequest.
    /// </summary>
    /// <param name="setState"></param>
    /// <returns></returns>
    public static bool SetAllowUnsafeHeaderParsing(bool setState)
    {
      try
      {
        //Get the assembly that contains the internal class
        Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
        if (aNetAssembly == null)
          return false;

        //Use the assembly in order to get the internal type for the internal class
        Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
        if (aSettingsType == null)
          return false;

        //Use the internal static property to get an instance of the internal settings class.
        //If the static instance isn't created allready the property will create it for us.
        object anInstance = aSettingsType.InvokeMember("Section",
                                                        BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic,
                                                        null, null, new object[] { });
        if (anInstance == null)
          return false;

        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
        if (aUseUnsafeHeaderParsing == null)
          return false;

        // and finally set our setting
        aUseUnsafeHeaderParsing.SetValue(anInstance, setState);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("HeadWeb: Unsafe header parsing setting change failed: " + ex.ToString());
        return false;
      }
    }

    public static string DictionaryToString(Dictionary<string, string> dic)
    {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true, OmitXmlDeclaration = true };
      using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(sb, settings))
      {
        writer.WriteStartElement("dictionary");
        foreach (string key in dic.Keys)
        {
          writer.WriteStartElement("item");
          writer.WriteStartElement("key");
          writer.WriteCData(key);
          writer.WriteEndElement();
          writer.WriteStartElement("value");
          writer.WriteCData(dic[key]);
          writer.WriteEndElement();
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.Flush();
        writer.Close();
      }
      return sb.ToString();
    }

    public static Dictionary<string, string> DictionaryFromString(string input)
    {
      Dictionary<string, string> dic = new Dictionary<string, string>();
      using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(new System.IO.StringReader(input)))
      {
        bool wasEmpty = reader.IsEmptyElement;
        reader.Read();
        if (wasEmpty) return null;
        reader.ReadStartElement("dictionary");
        while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
        {
          reader.ReadStartElement("item");
          reader.ReadStartElement("key");
          string key = reader.ReadContentAsString();
          reader.ReadEndElement();
          reader.ReadStartElement("value");
          string value = reader.ReadContentAsString();
          reader.ReadEndElement();
          dic.Add(key, value);
          reader.ReadEndElement();
          reader.MoveToContent();
        }
        reader.ReadEndElement();
      }
      return dic;
    }

    public static string GetSaveFilename(string input)
    {
      string safe = input;
      foreach (char lDisallowed in System.IO.Path.GetInvalidFileNameChars())
      {
        safe = safe.Replace(lDisallowed.ToString(), "");
      }
      foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars())
      {
        safe = safe.Replace(lDisallowed.ToString(), "");
      }
      return safe;
    }

    public static string EncryptLine(string strLine)
    {
      if (string.IsNullOrEmpty(strLine)) return string.Empty;
      Ionic.Zlib.CRC32 crc = new Ionic.Zlib.CRC32();
      MemoryStream stream = new MemoryStream();
      StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
      writer.Write(strLine);
      stream.Position = 0;
      return string.Format("{0}", crc.GetCrc32(stream));
    }

    public static string GetThumbFile(string url)
    {
      // gets a CRC code for the given url and returns a full file path to the image: thums_dir\crc.jpg|gif
      string possibleExtension = System.IO.Path.GetExtension(url).ToLower();
      if (possibleExtension != ".gif" & possibleExtension != ".jpg") possibleExtension = ".jpg";
      string name = string.Format("Thumbs{0}L{1}", EncryptLine(url), possibleExtension);
      return System.IO.Path.Combine(HeadWebSettings.Instance.ThumbsDir, name);
    }



    public static bool IsValidUri(string url)
    {
      try
      {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
            Uri.IsWellFormedUriString(Uri.EscapeUriString(url), UriKind.Absolute) ||
            System.IO.Path.IsPathRooted(url);
      }
      catch
      {
        return false;
      }
    }

    public static string GetNextFileName(string fullFileName)
    {
      if (string.IsNullOrEmpty(fullFileName)) throw new ArgumentNullException("fullFileName");
      if (!File.Exists(fullFileName)) return fullFileName;

      string baseFileName = Path.GetFileNameWithoutExtension(fullFileName);
      string ext = Path.GetExtension(fullFileName);

      string filePath = Path.GetDirectoryName(fullFileName);
      var numbersUsed = Directory.GetFiles(filePath, baseFileName + "_(*)" + ext)
              .Select(x => Path.GetFileNameWithoutExtension(x).Substring(baseFileName.Length + 1))
              .Select(x =>
              {
                int result;
                return Int32.TryParse(x.Trim(new char[] { '(', ')' }), out result) ? result : 0;
              })
              .Distinct()
              .OrderBy(x => x)
              .ToList();

      var firstGap = numbersUsed
              .Select((x, i) => new { Index = i, Item = x })
              .FirstOrDefault(x => x.Index != x.Item);
      int numberToUse = firstGap != null ? firstGap.Item : numbersUsed.Count;
      return Path.Combine(filePath, baseFileName) + "_(" + numberToUse + ")" + ext;
    }




    public static string GetWebData(string url, CookieContainer cc = null, string referer = null, IWebProxy proxy = null, bool forceUTF8 = false, bool allowUnsafeHeader = false, string userAgent = null, string method = null, string postDataString = null, bool useSSL = false, bool useCache = true)
    {
      try
      {
        if (useSSL)
        {
          url = url.Replace("http", "https");
        }
        Log.Debug("HeadWeb: get webdata from {0}", url);
       
        // try cache first
        if (useCache)
        {
          string cachedData = WebCache.Instance[url];
          if (cachedData != null) return cachedData;
        }

        // request the data
        if (allowUnsafeHeader) Utils.SetAllowUnsafeHeaderParsing(true);
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        if (request == null) return "";

        // POST or GET
        if (!String.IsNullOrEmpty(method))
        {
          request.Method = method;
        }











        // user agent
        if (!String.IsNullOrEmpty(userAgent))
        {
          request.UserAgent = userAgent;
        }
        else
        {
          request.UserAgent = HeadWebSettings.Instance.UserAgent;
        }

        request.Accept = "*/*";
        request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
        if (!String.IsNullOrEmpty(referer)) request.Referer = referer; // set referer if given
        if (cc != null) request.CookieContainer = cc; // set cookies if given
        if (proxy != null) request.Proxy = proxy;

        if (method == "POST" && !String.IsNullOrEmpty(postDataString))
        {
          // Create POST data and convert it to a byte array.
          string postData = postDataString;
          byte[] byteArray = Encoding.UTF8.GetBytes(postData);
          // Set the ContentType property of the WebRequest.
          request.ContentType = "application/x-www-form-urlencoded";
          // Set the ContentLength property of the WebRequest.
          request.ContentLength = byteArray.Length;
          // Get the request stream.
          using (Stream dataStream = request.GetRequestStream())
          {
            dataStream.Write(byteArray, 0, byteArray.Length);
          }
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream responseStream;
        if (response.ContentEncoding.ToLower().Contains("gzip"))
          responseStream = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
        else if (response.ContentEncoding.ToLower().Contains("deflate"))
          responseStream = new System.IO.Compression.DeflateStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
        else
          responseStream = response.GetResponseStream();
        Encoding encoding = Encoding.UTF8;
        if (!forceUTF8 && !String.IsNullOrEmpty(response.CharacterSet)) encoding = Encoding.GetEncoding(response.CharacterSet.Trim(new char[] { ' ', '"' }));
        using (StreamReader reader = new StreamReader(responseStream, encoding, true))
        {
          string str = reader.ReadToEnd().Trim();
          // add to cache if HTTP Status was 200 and we got more than 500 bytes (might just be an errorpage otherwise)
          if (response.StatusCode == HttpStatusCode.OK && str.Length > 500) WebCache.Instance[url] = str;
          return str;
        }
      }
      finally
      {
        // disable unsafe header parsing if it was enabled
        if (allowUnsafeHeader) Utils.SetAllowUnsafeHeaderParsing(false);
      }
    }





    public static string constructUrl(string path, string qstring)
    {
      if (qstring != "") qstring = "&" + qstring;
      string Url = rootUrl + path + "?apikey=" + apiKey + qstring;
      return Url;
    }











    public static void saveSub(string title, string source, string language)
    {

      XmlTextReader reader = new XmlTextReader(source);
      string newsub = "";
      bool fetchText = false;
      string path = "C:\\Subs\\";
      if(Directory.Exists(path))
      {
      TextWriter tw = new StreamWriter("C:\\Subs\\" + title + "." + language + ".srt", false, Encoding.UTF8); // Create a writer and open the file
      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.Element:
            if (reader.Name == "p") // Subtitle set found
            {
              fetchText = true;
              while (reader.MoveToNextAttribute()) // Read the attributes
              {
                if (reader.Name.IndexOf("id") > -1) // Sub number
                {
                  newsub += reader.Value.Replace("sub", "");
                  newsub += Environment.NewLine;
                }

                if (reader.Name.IndexOf("begin") > -1) // Sub starttime
                {
                  string btime = reader.Value.Replace("s", "");
                  btime = btime.Replace(".", ",");
                  double begintime = Double.Parse(btime);
                  TimeSpan bt = TimeSpan.FromSeconds(begintime);
                  string banswer = string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}",
                                          bt.Hours,
                                          bt.Minutes,
                                          bt.Seconds,
                                          bt.Milliseconds);
                  newsub += banswer;
                }

                if (reader.Name.IndexOf("end") > -1) // Sub starttime
                {
                  string etime = reader.Value.Replace("s", "");
                  etime = etime.Replace(".", ",");
                  double endtime = Double.Parse(etime);
                  TimeSpan et = TimeSpan.FromSeconds(endtime);
                  string eanswer = string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}",
                                          et.Hours,
                                          et.Minutes,
                                          et.Seconds,
                                          et.Milliseconds);
                  newsub += " --> " + eanswer;
                  newsub += Environment.NewLine;
                }
              }
            }
            break;
          case XmlNodeType.Text: //Display the text in each element.
            if(fetchText) newsub += reader.Value;
            break;
          case XmlNodeType.EndElement: //Display the end of the element.
            if (fetchText) newsub += Environment.NewLine + Environment.NewLine; fetchText = false;
            break;
        }
      }

      tw.ToString().TrimEnd('\r', '\n'); // Remove appended newlines
      tw.WriteLine(newsub); // Write subtitle to file
      tw.Close(); // Close file
      }
      else
      {
        Log.Info("HeadWeb: {0} is not a valid directory.", path);
      } 
    }





    public static string makeSafeFilename(string filename)
    {
      Array.ForEach(Path.GetInvalidFileNameChars(), c => filename = filename.Replace(c.ToString(), String.Empty));
      return filename;
    }









  }
}
