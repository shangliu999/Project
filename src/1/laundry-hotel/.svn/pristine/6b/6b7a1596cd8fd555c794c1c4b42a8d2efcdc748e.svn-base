using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Customer.Models;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Customer.Controllers
{
    public class RentMatchController : Controller
    {
        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        [Dependency]
        public IRepository<rentmatch> i_rentmatch { get; set; }

        // GET: Customer/RentMatch
        public ActionResult Index()
        {
            int cusId = 0;
            int.TryParse(Request.Params["hotelId"], out cusId);

            List<rentmatch> list = i_rentmatch.Entities.Where(v => v.HotelID == cusId).ToList();

            var data = from a in i_textileclass.Entities.Where(c => c.IsRFID == true && c.IsDelete == false)
                       join cs in i_classsize.Entities on a.ID equals cs.ClassID
                       into cs_join
                       from y in cs_join.DefaultIfEmpty()
                       join s in i_size.Entities.Where(c => c.IsDelete == false) on y.SizeID equals s.ID
                       into u1_join
                       from x in u1_join.DefaultIfEmpty()
                       orderby a.Sort, a.ClassName, x.SizeName
                       select new
                       {
                           a.ID,
                           a.ClassName,
                           SizeID = x == null ? 0 : x.ID,
                           SizeName = x == null ? "" : "(" + x.SizeName + ")"
                       };
            List<OrderDetailsModel> Olist = new List<OrderDetailsModel>();
            OrderDetailsModel item = null;
            rentmatch idetail = null;
            data.ToList().ForEach(c =>
            {
                item = new OrderDetailsModel();
                item.AreaID = c.ID;
                item.SizeID = c.SizeID;
                item.TextileName = c.ClassName;
                item.SizeName = c.SizeName;

                if (list != null)
                {
                    idetail = list.SingleOrDefault(v => v.ClassID == c.ID && v.SizeID == c.SizeID);
                    if (idetail != null && idetail.TextileCount > 0)
                    {
                        item.Count = idetail.TextileCount.ToString();
                    }
                    else
                    {
                        item.Count = "";
                    }
                }

                Olist.Add(item);
            });

            return View(Olist);
        }

        public ActionResult Add(OrderModel model)
        {
            MsgModel msgModel = new MsgModel();

            if (model.Data.Count > 0)
            {
                List<rentmatch> rmList = i_rentmatch.Entities.Where(v => v.HotelID == model.HotelID).ToList();
                if (rmList.Count == 0)
                {
                    rmList = new List<rentmatch>();
                    foreach (var item in model.Data)
                    {
                        rmList.Add(new rentmatch { HotelID = model.HotelID, ClassID = item.AreaID, SizeID = item.SizeID, TextileCount = int.Parse(item.Count) });
                    }

                    i_rentmatch.Insert(rmList);
                }
                else
                {
                    List<rentmatch> addList = new List<rentmatch>();
                    List<rentmatch> upList = new List<rentmatch>();
                    List<rentmatch> delList = new List<rentmatch>();
                    foreach (var item in model.Data)
                    {
                        var t = rmList.Where(v => v.ClassID == item.AreaID && v.SizeID == item.SizeID).FirstOrDefault();
                        if (t == null)
                        {
                            addList.Add(new rentmatch { HotelID = model.HotelID, ClassID = item.AreaID, SizeID = item.SizeID, TextileCount = int.Parse(item.Count) });
                        }
                        else
                        {
                            if (t.TextileCount.ToString() != item.Count)
                            {
                                t.TextileCount = int.Parse(item.Count);
                                upList.Add(t);
                            }
                        }
                    }
                    foreach (var item in rmList)
                    {
                        if (model.Data.Where(v => v.AreaID == item.ClassID && v.SizeID == item.SizeID).Count() == 0)
                        {
                            delList.Add(item);
                        }
                    }

                    i_rentmatch.Insert(addList, false);
                    foreach (var item in upList)
                    {
                        i_rentmatch.Update(item, false);
                    }
                    i_rentmatch.Delete(delList);
                }
            }
            else
            {
                i_rentmatch.Delete(v => v.HotelID == model.HotelID);
            }

            JsonResult jr = new JsonResult();
            jr.Data = msgModel;

            return jr;
        }
    }
}