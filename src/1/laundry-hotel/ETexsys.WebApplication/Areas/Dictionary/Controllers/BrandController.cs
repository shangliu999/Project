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
    public class BrandController : Controller
    {
        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        // GET: Dictionary/Brand
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetBrandTypeList(ParamModel model)
        {
            var query = i_brandtype.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.Sort).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBrandType(int brandId)
        {
            MsgModel msgModel = new MsgModel();

            brandtype model = i_brandtype.GetByKey(brandId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddBrandType(brandtype requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_brandtype.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateBrandType(brandtype requestParam)
        {
            MsgModel msgModel = new MsgModel();

            brandtype model = i_brandtype.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.BrandName = requestParam.BrandName;
                model.BrandCode = requestParam.BrandCode;
                model.BrandDesc = requestParam.BrandDesc;
                model.Sort = requestParam.Sort;

                int rtn = i_brandtype.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelBrandType(int brandID)
        {
            MsgModel msgModel = new MsgModel();

            brandtype model = i_brandtype.GetByKey(brandID);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_brandtype.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckBrandTypeName(string BrandName, int BrandID)
        {
            //id为0表示新增
            bool result = false;
            if (BrandID > 0)
            {
                int count = i_brandtype.LoadEntities(v => v.ID != BrandID && v.BrandName == BrandName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_brandtype.LoadEntities(v => v.BrandName == BrandName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}