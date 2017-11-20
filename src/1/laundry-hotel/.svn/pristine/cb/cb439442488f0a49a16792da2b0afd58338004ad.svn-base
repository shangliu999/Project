using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Dictionary.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class BagController : Controller
    {
        [Dependency]
        public IRepository<bag> i_bag { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        // GET: Dictionary/Bag
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetBagList(ParamModel model)
        {
            var query = i_bag.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.BagNo).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       join r in i_region.Entities on t.RegionID equals r.ID into r_join
                       from r in r_join.DefaultIfEmpty()
                       select new
                       {
                           t.ID,
                           t.BagNo,
                           t.BagRFIDNo,
                           t.CreateTime,
                           r.RegionName
                       };

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = data
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBag(int bagId)
        {
            MsgModel msgModel = new MsgModel();

            bag model = i_bag.GetByKey(bagId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddBag(bag requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_bag.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateBag(bag requestParam)
        {
            MsgModel msgModel = new MsgModel();

            bag model = i_bag.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.BagNo = requestParam.BagNo;
                model.BagRFIDNo = requestParam.BagRFIDNo;

                int rtn = i_bag.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelBag(int bagId)
        {
            MsgModel msgModel = new MsgModel();

            bag model = i_bag.GetByKey(bagId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_bag.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckBagName(string BagName, int BagID)
        {
            //id为0表示新增
            bool result = false;
            if (BagID > 0)
            {
                int count = i_bag.LoadEntities(v => v.ID != BagID && v.BagNo == BagName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_bag.LoadEntities(v => v.BagNo == BagName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}