﻿using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using System.Net;
using System.Diagnostics;

namespace IrcBot.YouTube
{
    /// <summary>
    /// Posts metadata for YouTube links.
    /// </summary>
    public class YouTube : IPlugin
    {
        const string YouTubeLinkRegex = @"http(?:s?)://(?:www\.)?youtu(?:be\.com/watch\?v=|\.be/)([\w\-]+)(&(amp;)?[\w\?=‌​]*)?";
        const string YouTubeGdataApiPrefix = "https://gdata.youtube.com/feeds/api/videos/";

        // We don't need to worry about titles, as the Title plugin does that
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            string to_send = "";
            MatchCollection matches = Regex.Matches(message, YouTubeLinkRegex);
            foreach (Match m in matches)
            {
                XmlDocument xd = new XmlDocument();
                try
                {
                    xd.Load(YouTubeGdataApiPrefix + m.Groups[1].Value);
                }
                catch (WebException e)
                {
                    Debug.WriteLine(e.ToString(), "TitlePlugin");
                    return null; // we failed
                }
                // Namespace BS
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xd.NameTable);
                nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                nsmgr.AddNamespace("media", "http://search.yahoo.com/mrss/");
                nsmgr.AddNamespace("yt", "http://gdata.youtube.com/schemas/2007");
                // Compile metadata
                string author = xd.SelectSingleNode("//atom:author/atom:name", nsmgr).InnerText;
                string views = xd.SelectSingleNode("//yt:statistics", nsmgr).Attributes["viewCount"].InnerText;
                DateTime published = DateTime.Parse(xd.SelectSingleNode("//atom:published", nsmgr).InnerText);
                TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(xd.SelectSingleNode("//media:group/yt:duration", nsmgr).Attributes["seconds"].InnerText));
                to_send = String.Format("YouTube video posted by {0} on {1}, with {2} views ({3} long)", author, published.ToShortDateString(), views, ts);
            }
            return to_send;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }
}
