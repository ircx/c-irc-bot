﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using System.Text.RegularExpressions;

namespace IrcBot.Title
{
    public class Title : IPlugin
    {
        // THANKS GRUBER
        static string crazy_regex = @"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'" + "\"" + ".,<>?«»“”‘’]))";

        string IPlugin.Invoke(string source, string message, ref IrcClient client)
        {
            string toSend = ""; // Make csc happy
            // catch urls
            MatchCollection matches = Regex.Matches(message, crazy_regex);
            foreach (Match m in matches)
            {
                if (!(m.Value.StartsWith("http://") || m.Value.StartsWith("https://"))) continue; // boo unprefix

                // Check if that's even an HTML file
                WebRequest wr = WebRequest.Create(m.Value);
                wr.Method = "HEAD";
                string type = wr.GetResponse().ContentType;
                // TODO: Support a whole bunch of wacky shit. img2aa anyone?
                // Go through the types. We StartWith because of encoding info.
                if (type.StartsWith("text/html"))
                { // We could support the other wacky shit like XML
                    toSend = GetHTMLGist(m.Value);
                }
            }
            return toSend != "" ? toSend : null;
        }

        /// <summary>
        /// Gets the title and any other interesting doodads of an HTML document.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The title, maybe other stuff..</returns>
        static string GetHTMLGist(string url)
        {
            // TODO: Support things like YouTube playback length, etc
            HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
            hd.LoadHtml(new WebClient().DownloadString(url));
            return hd.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
        }
    }
}