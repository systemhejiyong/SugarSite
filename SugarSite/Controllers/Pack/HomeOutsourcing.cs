﻿using Infrastructure.Pub;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SugarSite.Controllers
{
    public class HomeOutsourcing
    {
        /// <summary>
        /// 输出验证码
        /// </summary>
        internal void OutPutVerifyCode()
        {
            VerifyCodeSugar v = new VerifyCodeSugar();
            //是否随机字体颜色
            v.SetIsRandomColor = true;
            //随机码的旋转角度
            v.SetRandomAngle = 4;
            //文字大小
            v.SetFontSize = 15;
            var questionList = new Dictionary<string, string>()
           {
               {"Sugar群号是多少？"," 225982985"},
               {"作者名字叫什么？","孙凯旋"},
               {"作者QQ号码是多少？","610262374" },
               {"ASDF23花木成畦","123ADFA" }
           };
            var questionItem = v.GetQuestion(questionList);//不赋值为随机验证码 例如： 1*2=? 这种
            v.SetVerifyCodeText = questionItem.Key;
            string value = questionItem.Value;
            var sm = SessionManager<string>.GetInstance();
            sm.Add(PubConst.SessionVerifyCode, value);
            //输出图片
            v.OutputImage(System.Web.HttpContext.Current.Response);
        }
    }
}