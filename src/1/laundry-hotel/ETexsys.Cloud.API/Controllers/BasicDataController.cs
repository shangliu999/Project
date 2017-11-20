﻿using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ETexsys.Cloud.API.Controllers
{
    [SupportFilter]
    public class BasicDataController : ApiController
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<color> i_color { get; set; }

        [Dependency]
        public IRepository<fabric> i_fabric { get; set; }

        [Dependency]
        public IRepository<scrap> i_scrap { get; set; }

        [Dependency]
        public IRepository<bag> i_bag { get; set; }

        [Dependency]
        public IRepository<textilebrandarea> i_textilebrandarea { get; set; }

        public MsgModel HotelList()
        {
            MsgModel msg = new MsgModel();

            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();

            List<ResponseHotelModel> list = new List<ResponseHotelModel>();
            ResponseHotelModel model = null;

            foreach (var item in hotelList)
            {
                model = new ResponseHotelModel();
                model.ID = item.ID;
                model.HotelName = item.RegionName;
                model.BrandID = item.BrandID.Value;
                model.Sort = item.Sort;
                model.RegionMode = item.RegionMode.Value;
                model.DeliveryTime = item.DeliveryTime;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel RegionList()
        {
            MsgModel msg = new MsgModel();

            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 2 && v.IsDelete == false).ToList();

            List<ResponseRegionModel> list = new List<ResponseRegionModel>();
            ResponseRegionModel model = null;

            foreach (var item in hotelList)
            {
                model = new ResponseRegionModel();
                model.ID = item.ID;
                model.RegionName = item.RegionName;
                model.HotelID = item.ParentID.Value;
                model.Sort = item.Sort;
                model.RegionMode = item.RegionMode.Value;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel StoreList()
        {
            MsgModel msg = new MsgModel();

            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 3 && v.IsDelete == false).ToList();

            List<ResponseStoreModel> list = new List<ResponseStoreModel>();
            ResponseStoreModel model = null;

            foreach (var item in hotelList)
            {
                model = new ResponseStoreModel();
                model.ID = item.ID;
                model.StoreName = item.RegionName;
                model.Sort = item.Sort;
                model.StoreType = item.RegionMode.Value;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel TextileClassList()
        {
            MsgModel msg = new MsgModel();

            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseTextileClassModel> list = new List<ResponseTextileClassModel>();
            ResponseTextileClassModel model = null;

            foreach (var item in classList)
            {
                model = new ResponseTextileClassModel();
                model.ID = item.ID;
                model.ClassName = item.ClassName;
                model.ClassLeft = item.ClassLeft.HasValue ? item.ClassLeft.Value : 0;
                model.PackCount = item.PackCount.HasValue ? item.PackCount.Value : 0;
                model.Sort = item.Sort;
                model.IsRFID = item.IsRFID.HasValue ? item.IsRFID.Value : false;

                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel ClassSizeList()
        {
            MsgModel msgModel = new MsgModel();

            var query = from t in i_size.Entities
                        join u in i_classsize.Entities on t.ID equals u.SizeID
                        select new { sizename = t.SizeName, sort = t.Sort, id = t.ID, classid = u.ClassID };

            List<ResponseClassSizeModel> list = new List<ResponseClassSizeModel>();
            ResponseClassSizeModel model = null;

            query.ToList().ForEach(q =>
            {
                model = new ResponseClassSizeModel();
                model.ClassID = q.classid;
                model.SizeID = q.id;
                model.Sort = q.sort;
                model.SizeName = q.sizename;

                list.Add(model);
            });

            msgModel.Result = list;

            return msgModel;
        }

        public MsgModel BrandTypeList()
        {
            MsgModel msg = new MsgModel();

            List<brandtype> brandTypeList = i_brandtype.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseBrandTypeModel> list = new List<ResponseBrandTypeModel>();
            ResponseBrandTypeModel model = null;

            foreach (var item in brandTypeList)
            {
                model = new ResponseBrandTypeModel();
                model.ID = item.ID;
                model.BrandName = item.BrandName;
                model.Sort = item.Sort;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel SizeList()
        {
            MsgModel msg = new MsgModel();

            List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseSizeModel> list = new List<ResponseSizeModel>();

            ResponseSizeModel model = null;

            foreach (var item in sizeList)
            {
                model = new ResponseSizeModel();
                model.ID = item.ID;
                model.SizeName = item.SizeName;
                model.Sort = item.Sort;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel ColorList()
        {
            MsgModel msg = new MsgModel();

            List<color> colorList = i_color.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseColorModel> list = new List<ResponseColorModel>();

            ResponseColorModel model = null;

            foreach (var item in colorList)
            {
                model = new ResponseColorModel();
                model.ID = item.ID;
                model.ColorName = item.ColorName;
                model.Sort = item.Sort;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel FabricList()
        {
            MsgModel msg = new MsgModel();

            List<fabric> fabricList = i_fabric.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseFabricModel> list = new List<ResponseFabricModel>();

            ResponseFabricModel model = null;

            foreach (var item in fabricList)
            {
                model = new ResponseFabricModel();
                model.ID = item.ID;
                model.FabricName = item.FabricName;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel ScrapList()
        {
            MsgModel msg = new MsgModel();

            List<scrap> list = i_scrap.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseScrapModel> scrapList = new List<ResponseScrapModel>();
            ResponseScrapModel model = null;
            foreach (var item in list)
            {
                model = new ResponseScrapModel();
                model.ID = item.ID;
                model.ScrapName = item.ScrapName;
                scrapList.Add(model);
            }
            msg.Result = list;

            return msg;
        }

        public MsgModel BagList()
        {
            MsgModel msg = new MsgModel();

            List<bag> bagList = i_bag.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseBagModel> list = new List<ResponseBagModel>();
            ResponseBagModel model = null;

            foreach (var item in bagList)
            {
                model = new ResponseBagModel();
                model.ID = item.ID;
                model.BagNo = item.BagNo;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }

        public MsgModel TextileBrandList()
        {
            MsgModel msg = new MsgModel();

            List<textilebrandarea> tlist = i_textilebrandarea.Entities.Where(v => v.IsDelete == false).ToList();

            List<ResponseTextileBrandModel> list = new List<ResponseTextileBrandModel>();
            ResponseTextileBrandModel model = null;

            foreach (var item in tlist)
            {
                model = new ResponseTextileBrandModel();
                model.ID = item.ID;
                model.TextileBrandName = item.BrandAreaName;
                list.Add(model);
            }

            msg.Result = list;

            return msg;
        }
    }
}
