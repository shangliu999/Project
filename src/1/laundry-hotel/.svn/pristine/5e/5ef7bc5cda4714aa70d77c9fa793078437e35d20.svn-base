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

namespace ETexsys.WebApplication.Areas.Customer.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class FloorController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        // GET: Customer/Floor
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetFloorList(ParamModel model)
        {
            int hotelId = 0;
            int.TryParse(model.qCondition1, out hotelId);
            var query = i_region.Entities.Where(v => v.RegionType == 2 && v.IsDelete == false && v.ParentID == hotelId);

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

        public ActionResult GetFloor(int floorId)
        {
            MsgModel msgModel = new MsgModel();

            region model = i_region.GetByKey(floorId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddFloor(region requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            requestParam.RegionType = 2;

            int rtn = i_region.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateFloor(region requestParam)
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

        public JsonResult DelFloor(int floorId)
        {
            MsgModel msgModel = new MsgModel();

            region model = i_region.GetByKey(floorId);

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

        public JsonResult CheckFloorName(string FloorName, int FloorID, int HotelID)
        {
            //id为0表示新增
            bool result = false;
            if (FloorID > 0)
            {
                int count = i_region.LoadEntities(v => v.ID != FloorID && v.RegionType==2 && v.RegionName == FloorName && v.ParentID == HotelID && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_region.LoadEntities(v => v.RegionName == FloorName && v.RegionType == 2 && v.ParentID == HotelID && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}