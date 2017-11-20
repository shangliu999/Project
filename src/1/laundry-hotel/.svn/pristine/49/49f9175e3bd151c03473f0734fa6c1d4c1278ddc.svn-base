using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Report.Models;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Report.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class CustomerServiceController : Controller
    {
        [Dependency]
        public IRepository<complain> i_compain { get; set; }

        [Dependency]
        public IRepository<complainattach> i_complainattach { get; set; }

        [Dependency]
        public IRepository<complainmessage> i_complainmessage { get; set; }

        [Dependency]
        public IRepository<sys_user> i_user { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }
        // GET: Report/CustomerService
        public ActionResult Index()
        {
            List<region> list = i_region.Entities.Where(p => p.RegionType == 1).ToList();
            ViewData["regionList"] = list;
            return View();
        }
        public ActionResult GetList(ComplaintModel model)
        {
            var query = from c in i_compain.Entities
                        join u in i_user.Entities on c.UserID equals u.UserID into j_user
                        from u in j_user.DefaultIfEmpty()
                        join r in i_region.Entities on c.HotelID equals r.ID into j_region
                        from r in j_region.DefaultIfEmpty()
                        select new
                        {
                            HotelName = r.RegionName,
                            UserName = u.UName,
                            Cause = c.Cause.Length/2 > 15 ? c.Cause.Substring(0, 15) + "..." : c.Cause,
                            CreateTime = c.CreateTime,
                            Comfirmed = c.Comfirmed,
                            Remarks = c.Remarks,
                            CompleteCause=c.Cause,
                            ID=c.ID
                        };
            query = query.Where(p => p.Comfirmed == (model.State == "待处理" ? false : true));
            if (model.HotelName!="全部")
            {
                query = query.Where(p => p.HotelName == model.HotelName);
            }
            model.iDisplayLength = model.Count;
            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();
            var JsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(JsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Handle()
        {
            string Id = Request["ID"];
            complain complain= i_compain.Entities.Where(p => p.ID == Id).FirstOrDefault();
            complain.Comfirmed = true;
            i_compain.Update(complain);
            complainmessage msg = new complainmessage();
            msg.ComplainID = Id;
            msg.PublishMessage = Request["PublishMessage"];
            msg.PublishTime = DateTime.Now;
            msg.PublishType = 1;
            msg.PublishUserID = LoginUserManage.GetInstance().GetLoginUserId();
            int a=i_complainmessage.Insert(msg);

            return Json(a, JsonRequestBehavior.AllowGet);
        }
    }
}