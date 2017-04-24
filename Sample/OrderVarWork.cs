using System.Threading.Tasks;
using Greatbone.Core;

namespace Greatbone.Sample
{
    ///
    ///
    public abstract class OrderVarWork : Work
    {
        protected OrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public abstract class MyOrderVarWork : OrderVarWork
    {
        protected MyOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }


    public class MyCartOrderVarWork : MyOrderVarWork
    {
        public MyCartOrderVarWork(WorkContext wc) : base(wc)
        {
        }

        [Ui("附注/收货地址")]
        public async Task addr(ActionContext ac)
        {
            string buywx = ac[typeof(UserVarWork)];
            long ordid = ac[this];

            if (ac.GET)
            {
            }
            else
            {
            }
            string shopid = ac[0];
            Form frm = await ac.ReadAsync<Form>();
            int[] pk = frm[nameof(pk)];

            using (var dc = ac.NewDbContext())
            {
                dc.Sql("UPDATE orders SET ").setstate()._(" WHERE id = @1 AND shopid = @2 AND ").statecond();
                if (dc.Query(p => p.Set(pk).Set(shopid)))
                {
                    var order = dc.ToArray<Order>();
                }
                else
                {
                }
            }
        }

        [Ui("付款", UiMode.AnchorScript)]
        public async Task prepay(ActionContext ac)
        {
            long ordid = ac[this];
            string buywx = ((User) ac.Principal).wx;

            using (var dc = ac.NewDbContext())
            {
                string prepay_id = null;
                decimal total = 0;
                if (dc.Query1("SELECT prepay_id, total FROM orders WHERE id = @1 AND buywx = @2", p => p.Set(ordid).Set(buywx)))
                {
                    prepay_id = dc.GetString();
                    total = dc.GetDecimal();
                    if (prepay_id == null) // if not yet, call unifiedorder remotely
                    {
                        prepay_id = await WeiXinUtility.PostUnifiedOrderAsync(ordid, total, null, "http://shop.144000.tv/notify");
                        dc.Execute("UPDATE orders SET prepay_id = @1 WHERE id = @2", p => p.Set(prepay_id).Set(ordid));
                    }

                    ac.Give(200, WeiXinUtility.MakePrepayContent(prepay_id));
                }
                else
                {
                    ac.Give(404, "order not found");
                }
            }
        }
    }

    public class MyCurrentOrderVarWork : MyOrderVarWork
    {
        public MyCurrentOrderVarWork(WorkContext wc) : base(wc)
        {
        }

        [Ui("撤销")]
        public async Task abort(ActionContext ac)
        {
            string shopid = ac[0];
            Form frm = await ac.ReadAsync<Form>();
            int[] pk = frm[nameof(pk)];

            if (ac.GET)
            {
                using (var dc = ac.NewDbContext())
                {
                    dc.Sql("SELECT ").columnlst(Order.Empty)._("FROM orders WHERE id = @1 AND shopid = @2");
                    if (dc.Query(p => p.Set(pk).Set(shopid)))
                    {
                        var order = dc.ToArray<Order>();
                    }
                    else
                    {
                    }
                }
            }
            else
            {
                using (var dc = ac.NewDbContext())
                {
                    dc.Sql("UPDATE orders SET ").setstate()._(" WHERE id IN () AND shopid = @1 AND ").statecond();
                    if (dc.Query(p => p.Set(pk).Set(shopid)))
                    {
                        ac.Give(303); // see other
                    }
                    else
                    {
                        ac.Give(303); // see other
                    }
                }
            }
        }
    }

    public class MyHistoryOrderVarWork : MyOrderVarWork
    {
        public MyHistoryOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public abstract class OprOrderVarWork : OrderVarWork
    {
        protected OprOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class OprCreatedOrderVarWork : OprOrderVarWork
    {
        public OprCreatedOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class OprPaidOrderVarWork : OprOrderVarWork
    {
        public OprPaidOrderVarWork(WorkContext wc) : base(wc)
        {
        }

        public void @default(ActionContext ac)
        {
            string shopid = ac[0];
            int id = ac[this];

            using (var dc = Service.NewDbContext())
            {
                dc.Sql("SELECT ").columnlst(Order.Empty)._("FROM orders WHERE id = @1 AND shopid = @2");
                if (dc.Query(p => p.Set(id).Set(shopid)))
                {
                    var order = dc.ToArray<Order>();
                }
                else
                {
                }
            }
        }
    }

    public class OprPackedOrderVarWork : OprOrderVarWork
    {
        public OprPackedOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class OprSentOrderVarWork : OprOrderVarWork
    {
        public OprSentOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class OprDoneOrderVarWork : OprOrderVarWork
    {
        public OprDoneOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class OprAbortedOrderVarWork : OprOrderVarWork
    {
        public OprAbortedOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class DvrSentOrderVarWork : OrderVarWork
    {
        public DvrSentOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }

    public class DvrDoneOrderVarWork : OrderVarWork
    {
        public DvrDoneOrderVarWork(WorkContext wc) : base(wc)
        {
        }
    }
}