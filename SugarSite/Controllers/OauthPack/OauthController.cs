﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySqlSugar;
using OAuth.Tools;
using SyntacticSugar;
using Infrastructure.Dao;
using Infrastructure.DbModel;
using Infrastructure.Pub;

namespace SugarSite.Controllers
{
    public class OauthController : BaseController
    {
        public OauthController(DbService s) : base(s) { }

        public ActionResult CallBack(string state)
        {
            var current = OAuth2Factory.Current;
            if (current.openID.IsNullOrEmpty())
            {
                return Content("第三方登录失败！");
            }
            _service.Command<OauthOutsourcing>((db, o) =>
            {
                var pwd = new EncryptSugar().MD5(RandomSugar.GetRandomString(10));
                try
                {
                    var userMapping = db.Queryable<UserOAuthMapping>().SingleOrDefault(it => it.AppId == current.openID);
                    if (userMapping == null)//注册
                    {
                        db.BeginTran();
                        UserInfo u = o.GetUser(current, pwd);
                        var id = db.Insert(u).ObjToInt();
                        UserOAuthMapping um = o.GetUserOauthMapping(current, id);
                        db.Insert(um);
                        db.CommitTran();
                        userMapping = um;
                        RemoveNewUserListCache();
                    }
                    var user = db.Queryable<UserInfo>().InSingle(userMapping.UserId);
                    o.SaveAvatar(db,user);
                    var cm = CacheManager<UserInfo>.GetInstance();
                    string uniqueKey = PubGet.GetUserKey;
                    cm.Add(uniqueKey, user, cm.Day * 365);//保存一年
                    LoginHistory lh = new LoginHistory()
                    {
                        CreateDate = DateTime.Now,
                        IsDeleted = false,
                        Uid = user.Id,
                        UniqueKey = uniqueKey
                    };
                    db.Insert(lh);
                }
                catch (Exception ex)
                {
                    PubMethod.WirteExp(ex);
                    db.RollbackTran();
                    throw new Exception("第三方登录注册失败！" + ex.Message);
                }
            });
            return this.Redirect("~/ask");
        }
    }
}