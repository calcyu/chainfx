﻿using System;
using System.Text;
using System.Threading.Tasks;
using Greatbone;
using static Samp.SampUtility;
using static Samp.WeiXinUtility;

namespace Samp
{
    /// <summary>
    /// The sample service includes the gospel and the health provision.
    /// </summary>
    [Ui("全粮派")]
    public class SampService : Service<User>, IAuthenticateAsync
    {
        readonly WeiXin weixin;

        public SampService(ServiceConfig cfg) : base(cfg)
        {
            CreateVar<SampVarWork, string>(obj => ((Item) obj).name);

            Create<SampChatWork>("chat"); // chat

            Create<MyWork>("my"); // personal

            Create<TmWork>("tm"); // team

            Create<VdrWork>("vdr"); // vendor

            Create<CtrWork>("ctr"); // center

            Create<PlatWork>("plat"); // platform

            Register(delegate
                {
                    using (var dc = NewDbContext())
                    {
                        dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs WHERE status > 0 ORDER BY id");
                        return dc.Query<string, Org>(proj: 0xff);
                    }
                }, 3600 * 8
            );

            Register(delegate
                {
                    using (var dc = NewDbContext())
                    {
                        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE status > 0 ORDER BY name");
                        return dc.Query<string, Item>(proj: 0xff);
                    }
                }, 3600 * 8
            );

            weixin = DataUtility.FileToObject<WeiXin>(cfg.GetFilePath("$weixin.json"));
        }

        public WeiXin WeiXin => weixin;

        public async Task<bool> AuthenticateAsync(WebContext wc)
        {
            // if principal already in cookie
            if (wc.Cookies.TryGetValue("Token", out var token))
            {
                wc.Principal = Decrypt(token);
                return true;
            }

            // resolve principal thru OAuth2 or HTTP-basic
            User prin = null;
            string state = wc.Query[nameof(state)];
            if (WXAUTH.Equals(state)) // if weixin auth
            {
                string code = wc.Query[nameof(code)];
                if (code == null)
                {
                    return false;
                }
                (_, string openid) = await GetAccessorAsync(code);
                if (openid == null)
                {
                    return false;
                }
                // check in db
                using (var dc = NewDbContext())
                {
                    if (dc.Query1("SELECT * FROM users WHERE wx = @1", p => p.Set(openid)))
                    {
                        prin = dc.ToObject<User>(0xff ^ User.PRIVACY);
                    }
                    else
                    {
                        prin = new User {wx = openid}; // create a minimal principal object
                    }
                }
            }
            else
            {
                string h_auth = wc.Header("Authorization");
                if (h_auth == null || !h_auth.StartsWith("Basic "))
                {
                    return true;
                }
                // decode basic scheme
                byte[] bytes = Convert.FromBase64String(h_auth.Substring(6));
                string orig = Encoding.ASCII.GetString(bytes);
                int colon = orig.IndexOf(':');
                string tel = orig.Substring(0, colon);
                string credential = TextUtility.MD5(orig);
                using (var dc = NewDbContext())
                {
                    if (dc.Query1("SELECT * FROM users WHERE tel = @1", p => p.Set(tel)))
                    {
                        prin = dc.ToObject<User>();
                    }
                }
                // validate
                if (prin == null || !credential.Equals(prin.credential))
                {
                    return true;
                }
            }
            // setup principal and cookie
            if (prin != null)
            {
                // set token success
                wc.Principal = prin;
                wc.SetTokenCookie(prin, 0xff ^ User.PRIVACY);
            }
            return true;
        }

        public async Task @catch(WebContext wc, int cmd)
        {
            if (cmd == 1) // handle form submission
            {
                var prin = (User) wc.Principal;
                var f = await wc.ReadAsync<Form>();
                string name = f[nameof(name)];
                string tel = f[nameof(tel)];
                string url = f[nameof(url)];
                using (var dc = NewDbContext())
                {
                    var o = dc.Query1<User>("INSERT INTO users (name, wx, tel) VALUES (@1, @2, @3) RETURNING *", p => p.Set(name).Set(prin.wx).Set(tel));
                    wc.SetTokenCookie(o, 0xff ^ User.PRIVACY);
                }
                wc.GiveRedirect(url);
            }
            else if (wc.Except is AccessException ace)
            {
                if (ace.NoPrincipal)
                {
                    // weixin authorization challenge
                    if (wc.ByWeiXinClient) // weixin
                    {
                        wc.GiveRedirectWeiXinAuthorize(NETADDR);
                    }
                    else // challenge BASIC scheme
                    {
                        wc.SetHeader("WWW-Authenticate", "Basic realm=\"APP\"");
                        wc.Give(401); // unauthorized
                    }
                }
                else if (ace.NullResult)
                {
                    string url = wc.Path;
                    wc.GivePage(200, h =>
                    {
                        h.FORM_();
                        string name = null;
                        string tel = null;
                        h.FIELDSET_("填写用户信息");
                        h.TEXT(nameof(name), name, label: "用户昵称", max: 4, min: 2, required: true);
                        h.TEXT(nameof(tel), tel, label: "手　　机", pattern: "[0-9]+", max: 11, min: 11, required: true);
                        h._FIELDSET();
                        h.HIDDEN(nameof(url), url);
                        h.BOTTOMBAR_().BUTTON("/catch", 1, "确定", style: Style.Primary)._BOTTOMBAR();
                        h._FORM();
                    }, title: "用户注册");
                }
                else // IsNotAllowed
                {
                    wc.GivePage(403, h => { h.ALERT("您要使用的功能需要管理员授权。"); }, title: "没有访问权限");
                }
            }
            else
            {
                wc.Give(500, wc.Except.Message);
            }
        }

        public void @default(WebContext wc)
        {
            var arr = Obtain<Map<string, Item>>();
            wc.GivePage(200, h =>
                {
                    h.TOPBAR(true);
                    h.LIST(arr.All(), oi =>
                    {
                        h.ICO_(css: "uk-width-2-3 uk-padding-small").T(oi.name).T("/icon")._ICO();
                        h.COL_(0x23, css: "uk-padding-small");
                        h.T("<h3>").T(oi.name).T("</h3>");
                        h.FI(null, oi.descr);
                        h.ROW_();
                        h.P_(w: 0x23).T("￥<em>").T(oi.price).T("</em>／").T(oi.unit)._P();
                        h.FORM_(css: "uk-width-auto");
                        h.TOOL(nameof(SampVarWork.buy));
                        h._FORM();
                        h._ROW();
                        h._COL();
                    }, "uk-card-body uk-padding-remove");
                }, true, 60
            );
        }

        const string Aliyun = "http://aliyun.com/";


        /// <summary>
        /// WCPay notify, without authentic context.
        /// </summary>
        public async Task onpay(WebContext ac)
        {
            XElem xe = await ac.ReadAsync<XElem>();
            if (!OnNotified(xe, out var trade_no, out var cash))
            {
                ac.Give(400);
                return;
            }

            var orgs = Obtain<Map<string, Org>>();
            var (orderid, _) = trade_no.To2Ints();

            string orgid, custname, custaddr;
            using (var dc = NewDbContext())
            {
                if (!dc.Query1("UPDATE orders SET cash = @1, paid = localtimestamp, status = 1 WHERE id = @2 AND status = 0 RETURNING orgid, custname, custaddr", (p) => p.Set(cash).Set(orderid)))
                {
                    return; // WCPay may send notification more than once
                }
                dc.Let(out orgid).Let(out custname).Let(out custaddr);
            }
            // send weixin message
            var oprwx = orgs[orgid]?.mgrwx;
            if (oprwx != null)
            {
                await PostSendAsync(oprwx, "订单收款", ("¥" + cash + " " + custname + " " + custaddr), NETADDR + "/opr//newo/");
            }
            // return xml to WCPay server
            XmlContent x = new XmlContent(true, 1024);
            x.ELEM("xml", null, () =>
            {
                x.ELEM("return_code", "SUCCESS");
                x.ELEM("return_msg", "OK");
            });
            ac.Give(200, x);
        }
    }
}