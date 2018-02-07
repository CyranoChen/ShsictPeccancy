﻿using System;
using System.Web.Mvc;
using Shsict.Peccacy.Mvc.Models;
using Shsict.Peccacy.Service.DbHelper;
using Shsict.Peccacy.Service.Model;

namespace Shsict.Peccacy.Mvc.Controllers
{
    public class CameraSourceController : Controller
    {
        // GET: CameraSource
        public ActionResult Index()
        {
            var model = new ConsoleModels.CameraSourceListDto();

            using (IRepository repo = new Repository())
            {
                model.CameraSources = repo.All<CameraSource>();
            }

            return View(model);
        }

        // GET: CameraSource/Edit/5
        public ActionResult Edit(int id)
        {
            var model = CameraSource.Cache.Load(id);

            return View(model);
        }

        // POST: CameraSource/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CameraSource model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var cam = CameraSource.Cache.Load(model.ID);
                    if (cam == null) { throw new Exception("当前摄像头数据源无效"); }

                    cam.CamNo = model.CamNo;
                    cam.IsSync = model.IsSync;
                    cam.ConnString = model.ConnString;
                    cam.ViewName = model.ViewName;
                    cam.LastSyncTime = model.LastSyncTime;
                    cam.Remark = model.Remark;

                    using (IRepository repo = new Repository())
                    {
                        repo.Save(cam);
                    }

                    CameraSource.Cache.RefreshCache();

                    return RedirectToAction("Index");

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Warn", ex.Message);
                }
            }

            return View();
        }


        // GET: CameraSource/Sync/5
        public ActionResult Sync(int id)
        {
            using (IRepository repo = new Repository())
            {
                var cam = repo.Single<CameraSource>(id);

                if (cam != null)
                {
                    Service.SyncTruckRecordService.SyncCameraSource(cam);
                }
            }

            return RedirectToAction("Index", "CameraSource");
        }
    }
}