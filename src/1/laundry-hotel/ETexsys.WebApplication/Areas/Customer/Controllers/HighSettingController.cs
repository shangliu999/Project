using ETexsys.Common.SMS;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Customer.Controllers
{
    public class HighSettingController : Controller
    {
        [Dependency]
        public IRepository<pricetemplate> i_pricetemplate { get; set; }

        [Dependency]
        public IRepository<region_setting> i_region_setting { get; set; }

        [Dependency]
        public IRepository<integraldetail> i_integraldetail { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        // GET: Customer/HighSetting
        public ActionResult Index()
        {
            List<pricetemplate> TemplateList = i_pricetemplate.Entities.ToList();
            ViewData["TemplateList"] = TemplateList;
            int cusId = 0;
            int.TryParse(Request.Params["hotelId"], out cusId);
            region_setting model = i_region_setting.GetByKey(cusId);
            return View(model);
        }

        public ActionResult AddHotelSetting(region_setting request)
        {
            MsgModel msgModel = new MsgModel();

            region_setting model = i_region_setting.GetByKey(request.RegionID);
            if (model == null)
            {
                request.CreateTime = DateTime.Now;
                request.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                request.Integral = 0;
                int rtn = i_region_setting.Insert(request);
                msgModel.ResultCode = rtn > 0 ? 0 : 1;

            }
            else
            {
                model.Deposit = request.Deposit;
                model.WashID = request.WashID;
                model.RentID = request.RentID;
                model.LimitNumber = request.LimitNumber;
                model.MatchNumber = request.MatchNumber;
                model.PaymentMode = request.PaymentMode;
                model.RentPaymentMode = request.RentPaymentMode;
                model.WashingTime = request.WashingTime;
                model.RentTime = request.RentTime;
                int rtn = i_region_setting.Update(model);
                msgModel.ResultCode = rtn > 0 ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult UpdateHotelSetting(region_setting request)
        {
            MsgModel msgModel = new MsgModel();
            region_setting model = i_region_setting.GetByKey(request.RegionID);
            if (model != null)
            {
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.WashID = request.WashID;
                model.RentID = request.RentID;
                model.LimitNumber = request.LimitNumber;
                model.MatchNumber = request.MatchNumber;
                model.PaymentMode = request.PaymentMode;
                model.RentPaymentMode = request.RentPaymentMode;
                model.WashingTime = request.WashingTime;
                model.RentTime = request.RentTime;
                model.Deposit = request.Deposit;
                int rtn = i_region_setting.Update(model);
                msgModel.ResultCode = rtn > 0 ? 0 : 1;

            }
            else
            {
                msgModel.ResultCode = 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult Recharge(int hotelId, double point)
        {
            MsgModel msgModel = new MsgModel();

            region_setting model = i_region_setting.GetByKey(hotelId);
            if (model == null)
            {
                model = new region_setting();
                model.RegionID = hotelId;
                model.CreateTime = DateTime.Now;
                model.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.Integral = point;
                i_region_setting.Insert(model, false);
            }
            else
            {
                model.Integral = model.Integral + point;
                i_region_setting.Update(model, false);
            }
            integraldetail integralModel = new integraldetail();
            integralModel.CreateTime = DateTime.Now;
            integralModel.HotelID = hotelId;
            integralModel.SubType = 1;
            integralModel.Points = point;
            int rtn = i_integraldetail.Insert(integralModel);

            var query = from u in i_sys_user.Entities
                        join d in i_sys_user_dataview.Entities on u.UserID equals d.UserID
                        where u.UserType == 2 && d.RegionID == hotelId && d.IsDelete == false && u.IsLevel == false
                        select new { u.Phone };

            string key = ConfigurationManager.AppSettings["MoblieKey"];
            string uid = ConfigurationManager.AppSettings["MoblieUid"];
            string content = string.Format("您的酒店{0}充值{1}，剩余可用{2}。", DateTime.Now.ToString("MM月dd日"), point, model.Integral);
            query.ToList().ForEach(q =>
            {
                if (IsHandset(q.Phone))
                {
                    SMSTools.SendSMS(uid, key, q.Phone, content);
                }
            });

            msgModel.ResultCode = rtn > 0 ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        /// <summary>
        /// 验证手机号码是否合法
        /// </summary>
        /// <param name="str_handset"></param>
        /// <returns></returns>
        private bool IsHandset(string str_handset)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_handset, @"^1[3|4|5|7|8][0-9]\d{8}$");
        }
    }
}