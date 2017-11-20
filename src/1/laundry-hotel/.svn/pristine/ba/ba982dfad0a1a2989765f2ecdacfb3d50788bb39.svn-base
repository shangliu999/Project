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
    public class StoreController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        // GET: Dictionary/Store
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetStoreList(ParamModel model)
        {
            var query = i_region.Entities.Where(v => v.RegionType == 3 && v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.Sort).ThenBy(v => v.RegionName).Skip(model.iDisplayStart).Take(model.iDisplayLength);


            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStore(int storeId)
        {
            MsgModel msgModel = new MsgModel();

            region model = i_region.GetByKey(storeId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddStore(region requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            requestParam.RegionType = 3;

            int rtn = i_region.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateStore(region requestParam)
        {
            MsgModel msgModel = new MsgModel();

            region model = i_region.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.RegionName = requestParam.RegionName;
                model.RegionMode = requestParam.RegionMode;
                model.LinkMan = requestParam.LinkMan;
                model.Tel = requestParam.Tel;
                model.Sort = requestParam.Sort;

                int rtn = i_region.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelStore(int storeId)
        {
            MsgModel msgModel = new MsgModel();

            region model = i_region.GetByKey(storeId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_region.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckStoreName(string StoreName, int StoreID)
        {
            //id为0表示新增
            bool result = false;
            if (StoreID > 0)
            {
                int count = i_region.LoadEntities(v => v.ID != StoreID && v.RegionType == 3 && v.RegionName == StoreName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_region.LoadEntities(v => v.RegionName == StoreName && v.RegionType == 3 && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}