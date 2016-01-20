﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Leafing.Core;
using Leafing.Data;
using System.Collections.Generic;
using Leafing.Core.Text;
using Leafing.Data.Definition;
using Leafing.Data.Model;
using Leafing.Data.Model.Member;

namespace Leafing.Web.Common
{
    public static class PageHelper
    {
		public static bool ValidateSave(this ModelContext ctx, Page p, ValidateHandler vh, IDbObject obj,
			NoticeLabelAdapter msg, string noticeText, string cssCommon, string cssWarning, string cssNotice)
        {
			return ValidateSave(ctx, p, vh, obj, msg, noticeText, cssCommon, cssWarning, cssNotice, 
				() => DbEntry.Save(obj));
        }

		public static bool ValidateSave(this ModelContext ctx, Page p, ValidateHandler vh, object obj,
			NoticeLabelAdapter msg, string noticeText, string cssCommon, string cssWarning, string cssNotice,
			Action callback)
        {
            EnumControls(p, ctx.Info, delegate(MemberHandler mh, WebControl c)
            {
				c.CssClass = cssCommon;
            });
            vh.ValidateObject(obj);
            if (vh.IsValid)
            {
                callback();
                if (msg != null)
                {
					msg.AddMessage(noticeText);
					msg.ShowWith(cssNotice);
                }
            }
            else
            {
				foreach (string key in vh.ErrorMessages.Keys)
                {
                    if (msg != null)
                    {
						msg.AddMessage(vh.ErrorMessages[key]);
                    }
                    WebControl c = GetWebControl(p, ctx.Info, key);
                    if (c != null)
                    {
						c.CssClass = cssWarning;
					}
                }
				msg.ShowWith(cssWarning);
            }
            return vh.IsValid;
        }

        private static WebControl GetWebControl(Page p, ObjectInfo oi, string name)
        {
            string cid = string.Format("{0}_{1}", oi.HandleType.Name, name);
            var c = ClassHelper.GetValue(p, cid) as WebControl;
            return c;
        }

        private static void EnumControls(Page p, ObjectInfo oi, Action<MemberHandler, WebControl> callback)
        {
            foreach (MemberHandler h in oi.SimpleMembers)
            {
                if (!h.Is.Key)
                {
                    string cid = string.Format("{0}_{1}", oi.HandleType.Name, h.MemberInfo.Name);
                    var c = ClassHelper.GetValue(p, cid) as WebControl;
                    if (c != null)
                    {
                        callback(h, c);
                    }
                    else
                    {
                        if (!h.Is.AutoSavedValue && !h.Is.AllowNull)
                        {
                            throw new DataException(string.Format("Control {0} not find!", cid));
                        }
                    }
                }
            }
        }

		public static T GetObject<T>(this ModelContext ctx, object key, Page p, string parseErrorText) where T : class, IDbObject
        {
            var obj = DbEntry.GetObject<T>(key);
            return (T)GetObject(obj, ctx, p, parseErrorText);
        }

		public static T GetObject<T>(this ModelContext ctx, Page p, string parseErrorText)
        {
            object obj = ctx.NewObject();
            return (T)GetObject(obj, ctx, p, parseErrorText);
        }

        private static object GetObject(object obj, ModelContext ctx, Page p, string parseErrorText)
        {
            EnumControls(p, ctx.Info, delegate(MemberHandler h, WebControl c)
            {
                string v = GetValue(c);
                if (h.MemberType.IsEnum)
                {
                    var n = (int)Enum.Parse(h.MemberType, v);
                    h.SetValue(obj, n);
                }
                else
                {
                    if (string.IsNullOrEmpty(v))
                    {
                        if (h.Is.AllowNull)
                        {
                            h.SetValue(obj, null);
                        }
                        else
                        {
                            if (h.MemberType == typeof(string))
                            {
                                h.SetValue(obj, "");
                            }
                            else if(!h.Is.CreatedOn && !h.Is.SavedOn)
                            {
                                throw new WebControlException(c, string.Format(parseErrorText, h.ShowString, ""));
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            object iv = ClassHelper.ChangeType(v, h.MemberType);
                            h.SetValue(obj, iv);
                        }
                        catch (Exception ex)
                        {
                            throw new WebControlException(c, string.Format(parseErrorText, h.ShowString, ex.Message));
                        }
                    }
                }
            });
            return obj;
        }

        private static string GetValue(WebControl c)
        {
            if (c is TextBox)
            {
                return ((TextBox)c).Text;
            }
            if (c is CheckBox)
            {
                return ((CheckBox)c).Checked.ToString();
            }
            if (c is DropDownList)
            {
                return ((DropDownList)c).SelectedValue;
            }
            if(c is Label)
            {
                return ((Label) c).Text;
            }
            return GetPropertyInfo(c).GetValue(c, null).ToString();
        }

        private static PropertyInfo GetPropertyInfo(object c)
        {
            var attr = c.GetType().GetAttribute<DefaultPropertyAttribute>(false);
            if (attr != null)
            {
                var pi = c.GetType().GetProperty(attr.Name);
                if (pi != null)
                {
                    return pi;
                }
            }
            throw new NotSupportedException();
        }

		public static void SetObject(this ModelContext ctx, object obj, Page p)
        {
            EnumControls(p, ctx.Info, delegate(MemberHandler h, WebControl c)
            {
                object v = h.GetValue(obj);
                SetValue(c, v);
            });
        }

        private static void SetValue(WebControl c, object v)
        {
            if (c is TextBox)
            {
                ((TextBox)c).Text = (v ?? "").ToString();
            }
            else if (c is CheckBox)
            {
                ((CheckBox)c).Checked = (bool)v;
            }
            else if (c is DropDownList)
            {
                // Type t = v.GetType();
                ((DropDownList)c).SelectedValue = v.ToString();
            }
            else if (c is Label)
            {
                ((Label)c).Text = (v ?? "").ToString();
            }
            else GetPropertyInfo(c).SetValue(c, v, null);
        }

        public static ListItem[] GetItems(Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentOutOfRangeException();

            var ret = new List<ListItem>();
            foreach (string v in Enum.GetNames(enumType))
            {
                string n = StringHelper.EnumToString(enumType, v);
                var li = new ListItem(n, v);
                ret.Add(li);
            }
            return ret.ToArray();
        }
    }
}
