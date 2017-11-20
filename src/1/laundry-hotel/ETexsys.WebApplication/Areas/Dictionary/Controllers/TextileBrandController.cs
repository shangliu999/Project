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
    public class TextileBrandController : Controller
    {
        [Dependency]
        public IRepository<textilebrandarea> i_textilebrandarea { get; set; }

        // GET: Dictionary/TextileBrand
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetList(ParamModel model)
        {
            var query = i_textilebrandarea.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTextileBrand(int brandId)
        {
            MsgModel msgModel = new MsgModel();

            textilebrandarea model = i_textilebrandarea.GetByKey(brandId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult Add(textilebrandarea requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_textilebrandarea.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult Update(textilebrandarea requestParam)
        {
            MsgModel msgModel = new MsgModel();

            textilebrandarea model = i_textilebrandarea.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.BrandAreaName = requestParam.BrandAreaName; 

                int rtn = i_textilebrandarea.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelTextileBrand(int brandID)
        {
            MsgModel msgModel = new MsgModel();

            textilebrandarea model = i_textilebrandarea.GetByKey(brandID);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_textilebrandarea.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckBrandName(string BrandName, int BrandID)
        {
            //id为0表示新增
            bool result = false;
            if (BrandID > 0)
            {
                int count = i_textilebrandarea.LoadEntities(v => v.ID != BrandID && v.BrandAreaName == BrandName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_textilebrandarea.LoadEntities(v => v.BrandAreaName == BrandName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}