﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Greatbone.Core;

namespace Greatbone.Sample
{
    ///
    ///
    public class ItemFolder : WebFolder
    {
        public ItemFolder(WebFolderContext fc) : base(fc)
        {
            CreateVar<ItemVarFolder>();
        }

        #region /shop/-shopid-/item/

        public void lst(WebActionContext ac)
        {
            string shopid = ac[1];
            using (var dc = ac.NewDbContext())
            {
                if (dc.Query("SELECT * FROM items WHERE shopid = @1 AND enabled", p => p.Set(shopid)))
                {
                    ac.Reply(200, dc.Dump<JsonContent>());
                }
                else
                {
                    ac.Reply(204);
                }
            }
        }

        #endregion

        #region /shop/-shopid-/order/

        [Shop]
        [Ui]
        public void @default(WebActionContext ac)
        {
            string shopid = ac[1];
            using (var dc = ac.NewDbContext())
            {
                if (dc.Query("SELECT * FROM orders WHERE shopid = @1 AND status < 4", p => p.Set(shopid)))
                {
                    ac.ReplyGrid(200, dc.ToList<Item>());
                }
                else
                {
                    ac.ReplyGrid(200, (List<Item>)null);
                }
            }
        }

        [Shop]
        [Ui]
        public void toggle(WebActionContext ac)
        {
            string shopid = ac[1];
            using (var dc = ac.NewDbContext())
            {
                dc.Execute("UPDATE items SET enabled = NOT enabled WHERE shopid = @1", p => p.Set(shopid));
                // ac.SetHeader();
                ac.Reply(303); // see other
            }
        }

        [Shop]
        [Ui]
        public async Task modify(WebActionContext ac)
        {
            string shopid = ac[1];

            if (ac.GET)
            {
                var item = new Item() { };
                ac.ReplyForm(200, item);
            }
            else
            {
                var item = await ac.ReadObjectAsync<Item>();
                using (var dc = ac.NewDbContext())
                {
                    dc.Execute("UPDATE items SET enabled = NOT enabled WHERE shopid = @1", p => p.Set(shopid));
                    // ac.SetHeader();
                    ac.Reply(303); // see other
                }
            }
        }

        #endregion
    }
}