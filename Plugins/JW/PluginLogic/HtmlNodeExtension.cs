using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JW
{
    public static class HtmlNodeExtension
    {
        private static void FillHtmlNodes(HtmlNode htmlNode, Predicate<HtmlNode> predicate, ref List<HtmlNode> list)
        {
            if (htmlNode == null)
                return;
            if (predicate(htmlNode))
                list.Add(htmlNode);
            foreach (HtmlNode childNode in htmlNode.ChildNodes)
                FillHtmlNodes(childNode, predicate, ref list);
        }

        public static List<HtmlNode> GetHtmlNodes(this HtmlNode htmlNode, Predicate<HtmlNode> predicate)
        {
            var list = new List<HtmlNode>();
            FillHtmlNodes(htmlNode, predicate, ref list);
            return list;
        }

        public static HtmlNode GetHtmlNode(this HtmlNode htmlNode, Predicate<HtmlNode> predicate)
            => GetHtmlNodes(htmlNode, predicate).FirstOrDefault();
    }
}
