using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Dictionary.Controllers
{
    [Authorize(Roles = "Sys_User")]
    public class GoodsController : Controller
    {
        [Dependency]
        public IRepository<goods> i_goods { get; set; }

        [Dependency]
        public IRepository<goodscate> i_goodscate { get; set; }

        // GET: Dictionary/Goods

        [AddressUrl]
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<goodscate> list = i_goodscate.Entities.Where(v => v.Deleted == false).OrderBy(v => v.Sort).ThenBy(v => v.CateName).ToList();
            ViewData["CateList"] = list;
            ViewData["RightID"] = rightId;
            return View();
        }


        [AddressUrl]
        public ActionResult GetList(ParamModel model)
        {

            var query = from g in i_goods.Entities
                        join c in i_goodscate.Entities on g.CateID equals c.ID
                        where g.Deleted == false
                        select new
                        {
                            g.CateID,
                            g.CostPrice,
                            g.CreateTime,
                            g.CreateUserID,
                            g.Deleted,
                            g.GoodsLogo,
                            g.GoodsMoney,
                            g.GoodsName,
                            g.Grounding,
                            g.Hoting,
                            g.ID,
                            g.Remarks,
                            g.Sort,
                            g.UnitName,
                            g.UpdateTime,
                            g.UpdateUserID,
                            c.CateName,
                            CateSort = c.Sort,
                        };

            int cateId = 0;
            int.TryParse(model.qCondition1, out cateId);
            if (cateId > 0)
            {
                query = query.Where(v => v.CateID == cateId);
            }
            int state = -1;
            int.TryParse(model.qCondition2, out state);
            if (state >= 0)
            {
                bool grounding = Convert.ToBoolean(state);
                query = query.Where(v => v.Grounding == grounding);
            }
            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.CateSort).ThenBy(v => v.CateName).ThenBy(v => v.Sort).ThenBy(v => v.GoodsName).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [AddressUrl]
        public ActionResult GetModel(int Id)
        {
            MsgModel msgModel = new MsgModel();

            goods model = i_goods.GetByKey(Id);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddGoods(goods requestParam)
        {
            MsgModel msgModel = new MsgModel();

            if (Request.Files.Count > 0)
            {
                #region Logo

                string savepath = @"upload\\goods";
                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + savepath))
                {
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + savepath);
                }

                HttpPostedFileBase file = Request.Files[0];
                if (file.FileName != "")
                {
                    Random seed = new Random();
                    int j = seed.Next(0, 9);
                    string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + j.ToString() + Path.GetExtension(file.FileName);
                    filename = @"\\" + filename;
                    //检查上传文件是否有相同文件名
                    if (System.IO.File.Exists(savepath + filename))
                    {
                        int i = 1;
                        while (System.IO.File.Exists(savepath + filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."))))
                        {
                            i++;
                        }

                        filename = filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."));
                    }

                    file.SaveAs(AppDomain.CurrentDomain.BaseDirectory + savepath + filename);
                    requestParam.GoodsLogo = savepath + filename;
                }
                #endregion
            }
            else
            {
                requestParam.GoodsLogo = "";
            }

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.Deleted = false;
            int rtn = i_goods.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateGoods(goods requestParam)
        {
            MsgModel msgModel = new MsgModel();

            goods model = i_goods.GetByKey(requestParam.ID);

            if (model != null)
            {
                if (Request.Files.Count > 0)
                {
                    #region Logo

                    string savepath = @"upload\\goods";
                    if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + savepath))
                    {
                        System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + savepath);
                    }

                    HttpPostedFileBase file = Request.Files[0];
                    if (file.FileName != "")
                    {
                        Random seed = new Random();
                        int j = seed.Next(0, 9);
                        string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + j.ToString() + Path.GetExtension(file.FileName);
                        filename = @"\\" + filename;
                        //检查上传文件是否有相同文件名
                        if (System.IO.File.Exists(savepath + filename))
                        {
                            int i = 1;
                            while (System.IO.File.Exists(savepath + filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."))))
                            {
                                i++;
                            }

                            filename = filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."));
                        }

                        file.SaveAs(AppDomain.CurrentDomain.BaseDirectory + savepath + filename);
                        model.GoodsLogo = savepath + filename;
                    }
                    #endregion
                }

                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.GoodsName = requestParam.GoodsName;
                model.CateID = requestParam.CateID;
                model.GoodsMoney = requestParam.GoodsMoney;
                model.CostPrice = requestParam.CostPrice;
                model.UnitName = requestParam.UnitName;
                model.Remarks = requestParam.Remarks;
                model.Grounding = requestParam.Grounding;
                model.Hoting = requestParam.Hoting;
                model.Sort = requestParam.Sort;

                int rtn = i_goods.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        [AddressUrl]
        public JsonResult Delete(int Id)
        {
            MsgModel msgModel = new MsgModel();

            goods model = i_goods.GetByKey(Id);

            if (model != null)
            {
                model.Deleted = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_goods.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        [AddressUrl]
        public JsonResult CheckGoodsName(string GoodsName, int ID)
        {
            //id为0表示新增
            bool result = false;
            if (ID > 0)
            {
                int count = i_goods.LoadEntities(v => v.ID != ID && v.GoodsName == GoodsName && v.Deleted == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_goods.LoadEntities(v => v.GoodsName == GoodsName && v.Deleted == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}