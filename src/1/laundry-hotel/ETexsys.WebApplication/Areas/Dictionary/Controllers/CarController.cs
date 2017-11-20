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
    public class CarController : Controller
    {
        [Dependency]
        public IRepository<car> i_car { get; set; }

        // GET: Dictionary/Car
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetCarList(ParamModel model)
        {
            var query = i_car.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.ID).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCar(int carId)
        {
            MsgModel msgModel = new MsgModel();

            car model = i_car.GetByKey(carId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddCar(car requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_car.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateCar(car requestParam)
        {
            MsgModel msgModel = new MsgModel();

            car model = i_car.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.CarNo = requestParam.CarNo;
                model.OldValue = requestParam.OldValue;
                model.Phone = requestParam.Phone;
                model.UseName = requestParam.UseName;

                int rtn = i_car.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelCar(int carId)
        {
            MsgModel msgModel = new MsgModel();

            car model = i_car.GetByKey(carId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_car.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckCarName(string CarNo, int CarID)
        {
            //id为0表示新增
            bool result = false;
            if (CarID > 0)
            {
                int count = i_car.LoadEntities(v => v.ID != CarID && v.CarNo == CarNo && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_car.LoadEntities(v => v.CarNo == CarNo && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}