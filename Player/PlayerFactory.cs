using System;
using System.Xml.Serialization;
using MediaPortal.Player;

namespace HeadWeb
{
    enum PlayState { Init, Playing, Paused, Ended };

    public enum PlayerType
    {
      [XmlEnum(Name = "Auto")] Auto,
      [XmlEnum(Name = "Internal")] Internal,
      [XmlEnum(Name = "WMP")] WMP,
      [XmlEnum(Name = "VLC")] VLC
    }

    public interface OVSPLayer
    {
        bool GoFullscreen { get; set; }
    }

    public class PlayerFactory : IPlayerFactory
    {
        public string PreparedUrl { get; protected set; }
        public PlayerType PreparedPlayerType { get; protected set; }
        public IPlayer PreparedPlayer { get; protected set; }

        public PlayerFactory(PlayerType playerType, string url)
        {
            PreparedPlayerType = playerType;
            PreparedUrl = url;
            SelectPlayerType();
            PreparePlayer();
        }

        void SelectPlayerType()
        {
            if (PreparedPlayerType == PlayerType.Auto)
            {
                Uri uri = new Uri(PreparedUrl);

                if (uri.Scheme == "rtsp" || uri.Scheme == "sop" || uri.Scheme == "mms" || uri.PathAndQuery.Contains(".asf"))
                {
                    PreparedPlayerType = PlayerType.Internal;
                }
                else if (uri.PathAndQuery.Contains(".asx"))
                {
                    PreparedPlayerType = PlayerType.WMP;
                }
                else
                {
                    PreparedPlayerType = PlayerType.Internal;
                }
            }
        }

        void PreparePlayer()
        {
            switch (PreparedPlayerType)
            {
                case PlayerType.Internal: PreparedPlayer = new OnlineVideosPlayer(PreparedUrl); break;
            }
        }

        public IPlayer Create(string filename)
        {
            return Create(filename, g_Player.MediaType.Video);
        }  

        public IPlayer Create(string filename, g_Player.MediaType type)
        {
          /*
            if (filename != PreparedUrl)
                throw new OnlineVideosException("Cannot play a different url than this PlayerFactory was created with!");
            else
           */
                return PreparedPlayer;
        }              
    }
}
