using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Volvo.Common.Services.Helpers;
using Sitecore.Mvc;
using Volvo.Fxm.Web.Markup;

namespace Volvo.Fxm.Web.Helpers
{
    public static class HtmlExtensions
    {
        public static string CurrentLanguage(this HtmlHelper helper, Item item)
        {
            return item.Language.ToString();
        }

        public static HtmlString FXMResponsiveImage<T>(this HtmlHelper<T> helper, string fieldName, Action<FXMResponsiveImage> configuration)
        {
            return helper.FXMResponsiveImage(fieldName, helper.Sitecore().CurrentRendering.Item, configuration);
        }

        public static HtmlString FXMResponsiveImage<T>(this HtmlHelper<T> helper, MediaItem mediaItem, Action<FXMResponsiveImage> configuration)
        {
            FXMResponsiveImage responsiveImage = new FXMResponsiveImage();
            configuration(responsiveImage);
            return new HtmlString(responsiveImage.ToTag(mediaItem));
        }

        public static HtmlString FXMResponsiveImage<T>(this HtmlHelper<T> helper, string fieldName, Item item, Action<FXMResponsiveImage> configuration)
        {
            bool flag = item == null;
            HtmlString result;
            if (flag)
            {
                result = null;
            }
            else
            {
                FXMResponsiveImage responsiveImage = new FXMResponsiveImage();
                configuration(responsiveImage);
                bool flag2 = CommonHtmlExtensions.IsInEditingMode && responsiveImage.Editable;
                if (flag2)
                {
                    result = CommonHtmlExtensions.MakeEditable(fieldName, item);
                }
                else
                {
                    bool flag3 = item.IsFieldSet(fieldName);
                    if (flag3)
                    {
                        ImageField imageField = item.Fields[fieldName];
                        result = new HtmlString(responsiveImage.ToTag(imageField.MediaItem));
                    }
                    else
                    {
                        result = MvcHtmlString.Empty;
                    }
                }
            }
            return result;
        }
    }
}
