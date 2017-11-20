﻿using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Customer.Models;
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
    public class RentalPriceController : Controller
    {
        [Dependency]
        public IRepository<pricetemplate> i_priceTemplate { get; set; }

        [Dependency]
        public IRepository<pricedetail> i_pricedetail { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        // GET: Customer/RentalPrice
        public ActionResult Index()
        {
            List<textileclass> ClassList = i_textileclass.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.ClassName).ToList();
            List<size> SizeList = i_size.Entities.Where(v => v.IsDelete == false).ToList();
            List<classsize> ClassSizeList = i_classsize.Entities.ToList();
            List<pricedetail> DetailList = i_pricedetail.Entities.ToList();

            ViewData["ClassList"] = ClassList;
            ViewData["SizeList"] = SizeList;
            ViewData["ClassSizeList"] = ClassSizeList;
            //ViewData["DetailList"] = DetailList;


            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            //var query = from pd in i_pricedetail.Entities
            //            join c in i_textileclass.Entities on pd.ClassID equals c.ID into i_class
            //            from c in i_class.DefaultIfEmpty()
            //            join s in i_size.Entities on pd.SizeID equals s.ID into i_size
            //            from s in i_size.DefaultIfEmpty()
            //            select new
            //            {
            //                ClassName = c.ClassName,
            //                SizeName = s.SizeName == null ? "" : s.SizeName,
            //                price = pd.Price,
            //            };
            //var list = query.ToList();


            return View();
        }

        public ActionResult UpdataRentPriceForm(int ID)
        {
            MsgModel msg = new MsgModel();
            var query = from m in i_pricedetail.Entities
                        join s in i_size.Entities on m.SizeID equals s.ID into i_s
                        from s in i_s.DefaultIfEmpty()
                        join t in i_textileclass.Entities on m.ClassID equals t.ID into i_t
                        from t in i_t.DefaultIfEmpty()
                        join p in i_priceTemplate.Entities on m.TemplateID equals p.ID into i_p
                        from p in i_p.DefaultIfEmpty()
                        where m.TemplateID == ID
                        select new
                        {
                            DataSources = p.DataSources,
                            SettlementMode = p.SettlementMode,
                            TemplateName = p.TemplateName,
                            SubType = p.SubType,
                            SizeName = s.SizeName == null ? "" : s.SizeName,
                            ClassName = t.ClassName,
                            Price = m.Price,
                            ClassId = m.ClassID,
                            SizeId = m.SizeID
                        };
            RentWashiUpdataModel ret;
            List<RentWashiUpdataModel> list = new List<RentWashiUpdataModel>();
            var list1 = query.ToList();
            query.ToList().ForEach(
                q =>
                {
                    ret = new RentWashiUpdataModel();
                    ret.DataSources = q.DataSources;
                    ret.SettlementMode = q.SettlementMode;
                    ret.TemplateName = q.TemplateName;
                    ret.SubType = q.SubType;
                    ret.ClassName = q.ClassName;
                    ret.SizeName = q.SizeName;
                    ret.Price = q.Price;
                    ret.ClassId = q.ClassId;
                    ret.SizeId = q.SizeId;
                    list.Add(ret);
                });
            if (list.Count == 0)
            {
                pricetemplate val = i_priceTemplate.GetByKey(ID);
                return Json(val, JsonRequestBehavior.AllowGet);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRentalPriceList(ParamModel model)
        {
            var query = i_priceTemplate.Entities.Where(v => v.TemplateType == 1);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDetail(int ID)
        {
            MsgModel msgModel = new MsgModel();
            pricedetail pri = new pricedetail();
            List<pricedetail> list = i_pricedetail.Entities.Where(p => p.TemplateID == ID).ToList();
            var query = from p in i_pricedetail.Entities
                        join t in i_textileclass.Entities on p.ClassID equals t.ID into t_join
                        from t in t_join.DefaultIfEmpty()
                        join s in i_size.Entities on p.SizeID equals s.ID into s_join
                        from s in s_join.DefaultIfEmpty()
                        join r in i_priceTemplate.Entities on p.TemplateID equals r.ID into r_join
                        from r in r_join.DefaultIfEmpty()
                        where p.TemplateID == ID
                        select new
                        {
                            ID = p.ID,
                            DataSources = r.DataSources,
                            SettlementMode = r.SettlementMode,
                            TemplateName = r.TemplateName,
                            ClassName = t.ClassName,
                            SizeName = s.SizeName,
                            Price = p.Price
                        };
            List<RentWashPriceModel> listmodel = new List<RentWashPriceModel>();
            RentWashPriceModel model;
            query.ToList().ForEach(q =>
            {
                model = new RentWashPriceModel();
                model.ID = q.ID;
                model.DataSources = q.DataSources == 1 ? "手工录入" : (q.DataSources == 2 ? "下单" : (q.DataSources == 3 ? "污物送洗" : "净物配送"));
                model.SettlementMode = q.SettlementMode == 1 ? "按次" : (q.SettlementMode == 2 ? "按天" : "");
                model.TemplateName = q.TemplateName;
                model.ClassName = q.ClassName;
                model.SizeName = q.SizeName;
                model.Price = q.Price;
                listmodel.Add(model);
            });

            msgModel.OtherResult = listmodel;
            msgModel.Result = query.FirstOrDefault();
            JsonResult js = new JsonResult() { Data = msgModel };
            return Json(msgModel, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CheckName(string name)
        {
            bool result = false;
            int count = i_priceTemplate.Entities.Where(v => v.TemplateName == name && v.TemplateType == 1).Count();
            result = count > 0 ? false : true;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteRentPriceForm(int ID)
        {
            MsgModel msg = new MsgModel();
            int rt2 = 0;
            var list = i_priceTemplate.Entities.Where(p => p.ID == ID).ToList();
            var list1 = i_pricedetail.Entities.Where(p => p.TemplateID == ID).ToList();
            int rt1 = i_priceTemplate.Delete(list);
            if (list1.Count != 0)
            {
                rt2 = i_pricedetail.Delete(list1);
            }
            msg.Result = rt1;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRentalPrice(FormCollection f)
        {
            MsgModel msgModel = new MsgModel();
            pricetemplate priceModel = new pricetemplate();
            pricedetail detailModel;
            int count = Convert.ToInt32(f["DataLength"]);
            var TemplateName = f["TemplateName"];
            var SubType = f["SubType"];
            priceModel.TemplateName = f["TemplateName"];
            priceModel.TemplateType = 1;
            priceModel.SubType = short.Parse(f["SubType"]);
            priceModel.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            priceModel.CreateTime = DateTime.Now;
            priceModel.DataSources = short.Parse(f["DataSources"]);
            priceModel.SettlementMode = short.Parse(f["SettlementMode"]);
            int rtn1 = i_priceTemplate.Insert(priceModel);
            List<pricedetail> priceDetailList = new List<pricedetail>();
            for (int i = 0; i < count; i++)
            {
                string a = Request["PriceArray[{0}][price]"];
                double price = 0;
                double.TryParse(f[string.Format("PriceArray[{0}][price]", i)], out price);
                if (price <= 0)
                {
                    continue;
                }
                detailModel = new pricedetail();
                detailModel.ClassID = Convert.ToInt32(f[string.Format("PriceArray[{0}][classId]", i)]);
                detailModel.TemplateID = priceModel.ID;
                if (priceModel.SubType == 1)
                {
                    detailModel.SizeID = Convert.ToInt32(f[string.Format("PriceArray[{0}][sizeId]", i)]);
                }
                else
                {
                    detailModel.SizeID = 0;
                }
                detailModel.Price = Convert.ToDecimal(price);
                priceDetailList.Add(detailModel);
            }
            i_pricedetail.Insert(priceDetailList);

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult UpdataRentPrice()
        {
            MsgModel msg = new MsgModel();
            int Id = int.Parse(Request["ID"]);
            pricetemplate template = i_priceTemplate.GetByKey(Id);
            //List<pricedetail> detail;
            template.TemplateName = Request["TemplateName"];
            template.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            template.UpdateTime = DateTime.Now;
            template.DataSources = short.Parse(Request["DataSources"]);
            template.SettlementMode = short.Parse(Request["SettlementMode"]);
            msg.Result = i_priceTemplate.Update(template);
            int length = int.Parse(Request["arrlength"]);
            msg.OtherResult = 1;

            //先删除价格明细            
            i_pricedetail.Delete(p => p.TemplateID == Id, false);
            List<pricedetail> priceDetailList = new List<pricedetail>();
            pricedetail detailModel = null;
            for (int i = 0; i < length; i++)
            {
                double price = 0;
                double.TryParse(Request[string.Format("arr[{0}][price]", i)], out price);
                if (price <= 0)
                {
                    continue;
                }
                detailModel = new pricedetail();
                detailModel.ClassID = Convert.ToInt32(Request[string.Format("arr[{0}][classId]", i)]);
                detailModel.TemplateID = template.ID;
                if (template.SubType == 1)
                {
                    detailModel.SizeID = Convert.ToInt32(Request[string.Format("arr[{0}][sizeId]", i)]);
                }
                else
                {
                    detailModel.SizeID = 0;
                }
                detailModel.Price = Convert.ToDecimal(price);
                priceDetailList.Add(detailModel);
            }
            int rst = i_pricedetail.Insert(priceDetailList);
            msg.OtherResult = rst;

            //for (int i = 0; i < length; i++)
            //{
            //    int classId = int.Parse(Request["arr[" + i + "][classId]"]);
            //    string SizeId = Request["arr[" + i + "][sizeId]"] == null ? "0" : Request["arr[" + i + "][sizeId]"];
            //    decimal Price = decimal.Parse(Request["arr[" + i + "][price]"]);
            //    detail = new List<pricedetail>();
            //    detail = i_pricedetail.Entities.Where(p => p.TemplateID == Id).ToList();
            //    detail[i].ClassID = classId;
            //    detail[i].SizeID = int.Parse(SizeId);
            //    detail[i].Price = Price;
            //    int l = i_pricedetail.Update(detail[i]);
            //    msg.OtherResult = int.Parse(msg.OtherResult.ToString()) * l;
            //}
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsDouble()
        {
            int length = int.Parse(Request["TextLength"]);
            for (int i = 0; i < length; i++)
            {
                double d = 0;
                string val = Request["Text[" + i + "][text]"];
                if (double.TryParse(val, out d))
                {

                }
                else
                {
                    return Json("false", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("true", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddSubmitValidate()
        {
            bool result = false;
            string name = Request["name"];
            int length = int.Parse(Request["TextLength"]);
            int count = i_priceTemplate.Entities.Where(v => v.TemplateName == name && v.TemplateType == 1).Count();
            result = count > 0 ? false : true;
            if (!result)
            {
                return Json("false", JsonRequestBehavior.AllowGet);
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    double d = 0;
                    string val = Request["Text[" + i + "][text]"];
                    if (val == "")
                    {
                        continue;
                    }
                    else if (double.TryParse(val, out d))
                    {
                        continue;
                    }
                    else
                    {
                        return Json("false1", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("true", JsonRequestBehavior.AllowGet);
        }


    }

}