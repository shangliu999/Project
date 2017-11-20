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
    public class SizeController : Controller
    {
        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        // GET: Dictionary/Size
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetSizeList(ParamModel model)
        {
            var query = i_size.Entities.Where(v => v.IsDelete == false);

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

        public ActionResult GetSize(int sizeId)
        {
            MsgModel msgModel = new MsgModel();

            size model = i_size.GetByKey(sizeId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddSize(size requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_size.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateSize(size requestParam)
        {
            MsgModel msgModel = new MsgModel();

            size model = i_size.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.SizeName = requestParam.SizeName;
                model.Sort = requestParam.Sort;

                int rtn = i_size.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelSize(int sizeId)
        {
            MsgModel msgModel = new MsgModel();

            size model = i_size.GetByKey(sizeId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_size.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;

                i_classsize.Delete(p => p.SizeID == sizeId);
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckSizeName(string SizeName, int SizeID)
        {
            //id为0表示新增
            bool result = false;
            if (SizeID > 0)
            {
                int count = i_size.LoadEntities(v => v.ID != SizeID && v.SizeName == SizeName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_size.LoadEntities(v => v.SizeName == SizeName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}