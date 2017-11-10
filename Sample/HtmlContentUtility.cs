﻿using Greatbone.Core;

namespace Greatbone.Sample
{
    public static class HtmlContentUtility
    {
        public static HtmlContent TOPBAR_(this HtmlContent h, string title)
        {
            h.T("<header data-sticky-container>");
            h.T("<div class=\"sticky\" style=\"width: 100%\" data-sticky  data-options=\"anchor: page; marginTop: 0; stickyOn: small;\">");
            h.T("<form>");
            h.T("<div class=\"top-bar\">");
            if (title != null)
            {
                h.T("<div class=\"top-bar-title\">").T(title).T("</div>");
            }
            h.T("<div class=\"top-bar-left\">");
            return h;
        }

        public static HtmlContent _TOPBAR(this HtmlContent h)
        {
            h.T("</div>"); // closing of top-bar-left

            h.T("<div class=\"top-bar-right\">");
            h.T("<a class=\"float-right\" href=\"/my//pre/\"><i class=\"fi-shopping-cart\" style=\"font-size: 1.5rem; \"></i></a>");
            h.T("</div>");

            h.T("</div>");
            h.T("</form>");
            h.T("</div>");
            h.T("</header>");
            return h;
        }

        public static HtmlContent DROPDOWN_(this HtmlContent h, string name)
        {
            h.T("<a type=\"button hollow\" class=\"button circle primary float-right\"  data-toggle=\"dropdown").T(name).T("\">购买</a>");
            h.T("<div class=\"dropdown-pane\" id=\"dropdown").T(name).T("\" data-position=\"top\" data-alignment=\"right\" style=\"box-shadow:0 0 2px #0a0a0a;\" data-dropdown>");
            return h;
        }

        public static HtmlContent DROPDOWNPANE_(this HtmlContent h, string name)
        {
            h.T("<a type=\"button hollow\" class=\"button circle primary float-right\"  data-toggle=\"dropdown").T(name).T("\">购买</a>");
            h.T("<div class=\"dropdown-pane\" id=\"dropdown").T(name).T("\" data-position=\"top\" data-alignment=\"right\" style=\"box-shadow:0 0 2px #0a0a0a;\" data-dropdown>");
            return h;
        }

        public static HtmlContent _DROPDOWNPANE(this HtmlContent h)
        {
            h.T("</div>");
            return h;
        }
    }
}