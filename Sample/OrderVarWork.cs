using System.Threading.Tasks;
using Greatbone;
using static Greatbone.Modal;

namespace Samp
{
    public abstract class OrderVarWork : Work
    {
        protected OrderVarWork(WorkConfig cfg) : base(cfg)
        {
        }
    }

    public class MyOrderVarWork : OrderVarWork
    {
        public MyOrderVarWork(WorkConfig cfg) : base(cfg)
        {
        }

        [Ui("撤单"), Tool(AShow), OrderState('C')]
        public async Task cancel(WebContext wc, int idx)
        {
            int orderid = wc[this];
            int myid = wc[-2];
            if (wc.GET)
            {
                using (var dc = NewDbContext())
                {
                    var o = dc.Query1<Order>("SELECT * FROM orders WHERE id = @1 AND custid = @2", p => p.Set(orderid).Set(myid));
                    var item = Obtain<Map<string, Item>>()[o.item];
                    wc.GivePane(200, h =>
                    {
                        h.FORM_();
                        h.FIELDUL_("购买数量");
                        h.LI_().LABEL("货品").ICO("/" + o.item + "/icon", css: "uk-width-1-6").SP().T(o.item)._LI();
                        //                        h.NUMBER(nameof(oi.qty), oi.qty, "购量", max: item.max, min: (short) 0, step: item.step);
                        h._FIELDUL();
                        h._FORM();
                    });
                }
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                short qty = f[nameof(qty)];
                using (var dc = NewDbContext())
                {
                    var o = dc.Query1<Order>("SELECT * FROM orders WHERE id = @1 AND custid = @2", p => p.Set(orderid).Set(myid));
                    //                    o.UpdItem(idx, qty);
                    dc.Execute("UPDATE orders SET rev = rev + 1, items = @1, total = @2, net = @3 WHERE id = @4", p => p.Set(o.item).Set(o.total).Set(o.score).Set(o.id));
                }
                wc.GivePane(200);
            }
        }
    }

    public class RegOrderVarWork : OrderVarWork
    {
        public RegOrderVarWork(WorkConfig cfg) : base(cfg)
        {
        }

        [Ui("撤消", "确认要撤销此单吗？实收款项将退回给买家"), Tool(ButtonPickConfirm)]
        public async Task abort(WebContext wc)
        {
            string orgid = wc[-2];
            int orderid = wc[this];
            short rev = 0;
            decimal cash = 0;
            using (var dc = NewDbContext())
            {
                if (dc.Query1("SELECT rev, cash FROM orders WHERE id = @1 AND status = 1", p => p.Set(orderid)))
                {
                    dc.Let(out rev).Let(out cash);
                }
            }
            if (cash > 0)
            {
//                string err = await ((SampService) Service).Hub.PostRefundAsync(orderid + "-" + rev, cash, cash);
//                if (err == null) // success
//                {
//                    using (var dc = NewDbContext())
//                    {
//                        dc.Execute("UPDATE orders SET status = -1, aborted = localtimestamp WHERE id = @1 AND orgid = @2", p => p.Set(orderid).Set(orgid));
//                    }
//                }
            }
            wc.GiveRedirect("../");
        }

        [Ui("送货", group: 1), Tool(ButtonPickPrompt)]
        public async Task receive(WebContext wc)
        {
        }

        [Ui("送货", group: 2), Tool(ButtonPickPrompt)]
        public async Task dgrp(WebContext wc)
        {
        }
    }

    public class TeamOrderVarWork : OrderVarWork
    {
        public TeamOrderVarWork(WorkConfig cfg) : base(cfg)
        {
        }

        [Ui("收货"), Tool(ButtonPickPrompt)]
        public async Task receive(WebContext wc)
        {
            string grpid = wc[-1];
            if (wc.GET)
            {
                int[] key = wc.Query[nameof(key)];
                wc.GivePane(200, h =>
                {
                    using (var dc = NewDbContext())
                    {
                        dc.Sql("SELECT item, SUM(qty) AS num FROM orders WHERE id")._IN_(key).T(" AND status = 3 AND grpid = @1 GROUP BY item");
                        dc.Query(p => p.SetIn(key).Set(grpid));
                        h.FORM_();

                        h.T("仅列出已送达货品");
                        h.T("<table class=\"uk-table\">");
                        while (dc.Next())
                        {
                            dc.Let(out string item).Let(out short num);
                            h.TD(item).TD(num);
                        }
                        h.T("</table>");
                        h.CHECKBOX("", false, "我确认收货", required: true);
                        h._FORM();
                    }
                });
            }
            else // POST
            {
                int[] key = (await wc.ReadAsync<Form>())[nameof(key)];
                using (var dc = NewDbContext())
                {
                    dc.Sql("UPDATE orders SET status = 4 WHERE id")._IN_(key).T(" AND status = 3 AND grpid = @1");
                    dc.Execute(p => p.SetIn(key).Set(grpid));
                }
                wc.GiveRedirect();
            }
        }

        [Ui("递货"), Tool(ButtonPickPrompt)]
        public void give(WebContext wc)
        {
        }
    }

    public class ShopOrderVarWork : OrderVarWork
    {
        public ShopOrderVarWork(WorkConfig cfg) : base(cfg)
        {
        }
    }
}