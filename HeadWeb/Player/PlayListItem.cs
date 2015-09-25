using System;
using MePo = MediaPortal.Playlists;

namespace HeadWeb.Player
{
    public class PlayListItem : MePo.PlayListItem
    {
        public PlayListItem(string description, string fileName) : base(description, fileName) { }

        //public VideoInfo Video { get; set; }

        //public Sites.SiteUtilBase Util { get; set; }

        public string ChosenPlaybackOption { get; set; }

        //public OnlineVideos.PlayerType? ForcedPlayer { get; set; }
    }
}