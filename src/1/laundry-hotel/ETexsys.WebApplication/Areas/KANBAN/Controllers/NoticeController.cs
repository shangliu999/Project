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

namespace ETexsys.WebApplication.Areas.KANBAN.Controllers
{
    public class NoticeController : Controller
    {
        [Dependency]
        public IRepository<notice> i_notice { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Authorize(Roles = "Sys_User")]
        [AddressUrl]
        // GET: KANBAN/Notice
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<region> regionlist = i_region.Entities.Where(c => c.RegionType == 3 && c.IsDelete == false).OrderBy(c => c.Sort).ToList();
            ViewData["RegionList"] = regionlist;

            return View();
        }

        [Authorize(Roles = "Sys_User")]
        [AddressUrl]
        public ActionResult GetNoticeList(ParamModel model)
        {
            var query = from n in i_notice.Entities
                        join r in i_region.Entities on n.Position equals r.ID
                        where n.IsDelete == false
                        select new
                        {
                            n.ID,
                            n.IsFull,
                            n.Start,
                            n.End,
                            n.Content,
                            n.UpdateTime,
                            r.RegionName,
                        };

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

        [Authorize(Roles = "Sys_User")]
        [AddressUrl]
        public ActionResult GetNotice(int noticeId)
        {
            MsgModel msgModel = new MsgModel();

            notice model = i_notice.GetByKey(noticeId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        [ValidateInput(false)]
        public JsonResult AddNotice(notice requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.UpdateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_notice.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        [ValidateInput(false)]
        public JsonResult UpdateNotice(notice requestParam)
        {
            MsgModel msgModel = new MsgModel();

            notice model = i_notice.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.IsFull = requestParam.IsFull;
                model.Start = requestParam.Start;
                model.End = requestParam.End;
                model.Content = requestParam.Content;
                model.Position = requestParam.Position;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                int rtn = i_notice.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelNotice(int carId)
        {
            MsgModel msgModel = new MsgModel();

            notice model = i_notice.GetByKey(carId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_notice.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        /// <summary>
        /// 看板获取公告信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNotices()
        {
            DateTime now = DateTime.Now.Date;

            var query = i_notice.Entities.Where(c => c.IsDelete == false && c.Start <= now && c.End >= now);

            JsonResult jr = new JsonResult();
            jr.Data = query.ToList();

            return jr;
        }
    }
}