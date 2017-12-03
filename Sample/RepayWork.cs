﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Greatbone.Core;
using static Greatbone.Core.Modal;
using static Greatbone.Sample.Repay;

namespace Greatbone.Sample
{
    public abstract class RepayWork<V> : Work where V : RepayVarWork
    {
        protected RepayWork(WorkContext wc) : base(wc)
        {
            CreateVar<V, int>();
        }
    }

    [Ui("结款")]
    [Auth(User.OPRMGR)]
    public class OprRepayWork : RepayWork<OprRepayVarWork>
    {
        public OprRepayWork(WorkContext wc) : base(wc)
        {
        }

        public void @default(ActionContext ac)
        {
            string shopid = ac[-1];
            using (var dc = ac.NewDbContext())
            {
                if (dc.Query("SELECT * FROM repays WHERE shopid = @1", p => p.Set(shopid)))
                {
                    ac.GiveBoardPage(200, dc.ToArray<Repay>(), (h, o) =>
                    {
                        h.CAPTION(false, "截至" + o.till);
                        h.FIELD(o.total, "金额", box:6).FIELD(o.payer, "划款",box:6);
                    }, false, 3);
                }
                else
                {
                    ac.GiveSheetPage(200, (Repay[]) null, null, null);
                }
            }
        }
    }

    [Ui("结款")]
    public class AdmRepayWork : RepayWork<AdmRepayVarWork>
    {
        public AdmRepayWork(WorkContext wc) : base(wc)
        {
        }

        [Ui("列新结"), Tool(Anchor)]
        public void @default(ActionContext ac, int page)
        {
            using (var dc = ac.NewDbContext())
            {
                dc.Query("SELECT * FROM repays WHERE status = " + CREATED + " ORDER BY id DESC, status LIMIT 20 OFFSET @1", p => p.Set(page * 20));
                ac.GiveSheetPage(200, dc.ToArray<Repay>(),
                    h => h.TH("网点").TH("截至日期").TH("金额").TH("转款"),
                    (h, o) => h.TD(o.shopname).TD(o.till).TD(o.total).TD(o.payer)
                );
            }
        }

        [Ui("列已转"), Tool(Anchor)]
        public void old(ActionContext ac, int page)
        {
            using (var dc = ac.NewDbContext())
            {
                dc.Query("SELECT * FROM repays ORDER BY id DESC, status = " + PAID + " LIMIT 20 OFFSET @1", p => p.Set(page * 20));
                ac.GiveSheetPage(200, dc.ToArray<Repay>(),
                    h => h.TH("网点").TH("截至日期").TH("金额").TH("转款"),
                    (h, o) => h.TD(o.shopname).TD(o.till).TD(o.total).TD(o.payer)
                );
            }
        }

        [Ui("结算", "结算各网点已完成的订单"), Tool(ButtonShow)]
        public async Task reckon(ActionContext ac)
        {
            DateTime fro; // till/before date
            DateTime till; // till/before date
            if (ac.GET)
            {
                using (var dc = ac.NewDbContext())
                {
                    fro = (DateTime) dc.Scalar("SELECT till FROM repays ORDER BY id DESC LIMIT 1");
                    ac.GivePane(200, m =>
                    {
                        m.FORM_();
                        m.DATE(nameof(fro), fro, "起始", @readonly: true);
                        m.DATE(nameof(till), DateTime.Today, "截至", max: DateTime.Today);
                        m._FORM();
                    });
                }
                return;
            }

            var f = await ac.ReadAsync<Form>();
            fro = f[nameof(fro)];
            till = f[nameof(till)];
            using (var dc = ac.NewDbContext(IsolationLevel.ReadUncommitted))
            {
                dc.Execute(@"INSERT INTO repays (shopid, shopname, fro, till, orders, total) 
                    SELECT shopid, shopname, @1, @2, COUNT(*), SUM(total) FROM orders WHERE status = 4 AND completed >= @1 AND completed < @2 GROUP BY shopid", p => p.Set(fro).Set(till));
            }
            ac.GivePane(200);
        }

        struct Transfer
        {
            internal int id;
            internal string mgrwx;
            internal string mgr;
            internal decimal cash;
        }

        [Ui("转款", "按照结算单转款给网点"), Tool(ButtonConfirm)]
        public async Task pay(ActionContext ac)
        {
            List<Transfer> lst = new List<Transfer>(16);
            using (var dc = ac.NewDbContext())
            {
                // retrieve
                if (dc.Query("SELECT r.id, mgrwx, mgr, cash FROM repays AS r, shops AS s WHERE r.shopid = s.id AND r.status = 0"))
                {
                    while (dc.Next())
                    {
                        Transfer tr;
                        dc.Let(out tr.id).Let(out tr.mgrwx).Let(out tr.mgr).Let(out tr.cash);
                        lst.Add(tr);
                    }
                }
            }

            // transfer for each
            foreach (var tr in lst)
            {
                string err = await WeiXinUtility.PostTransferAsync(tr.id, tr.mgrwx, tr.mgr, tr.cash, "订单结款");

                // update status
                using (var dc = ac.NewDbContext())
                {
                    if (err != null)
                    {
                        dc.Execute("UPDATE repays SET err = @1 WHERE id = @2", p => p.Set(err).Set(tr.id));
                    }
                    else
                    {
                        User prin = (User) ac.Principal;
                        dc.Execute("UPDATE repays SET payer = @1, paid = localtimestamp, err = NULL, status = 1 WHERE id = @2", p => p.Set(prin.name).Set(tr.id));
                    }
                }
            }
            ac.GiveRedirect();
        }
    }
}