using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Volvo.Web.Helpers.Mvc;

namespace Volvo.Fxm.Web.Markup
{
    public class FXMResponsiveImage : ResponsiveImage
    {
        public string Language { get; set; }

        new public string ToTag(MediaItem item)
        {
            bool flag = item == null;
            string result;
            string sc_lang = string.IsNullOrWhiteSpace(this.Language) ? "" : "&sc_lang=" + this.Language;
            if (flag)
            {
                result = string.Empty;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder("<img");
                stringBuilder.AppendFormat(" alt=\"{0}\"", this.Alt ?? item.Alt);
                bool flag2 = this.SrcSetDeviceRatio != null && this.SrcSetDeviceRatio.ContainsKey("1x");
                if (flag2)
                {
                    stringBuilder.AppendFormat(" src=\"{0}{1}\"", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                    {
                        Width = this.SrcSetDeviceRatio["1x"]
                    }).Replace(" ", "%20"), sc_lang);
                    stringBuilder.AppendFormat(" srcset=\"{0}\"", string.Join(", ", from srcsetItem in this.SrcSetDeviceRatio
                                                                                    select string.Format("{0}{2} {1}", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                                                                                    {
                                                                                        Width = srcsetItem.Value
                                                                                    }).Replace(" ", "%20"), srcsetItem.Key, sc_lang)));
                }
                else
                {
                    bool flag3 = this.SrcSetStaticRatio != null && this.SrcSetStaticRatio.Any<KeyValuePair<string, int>>();
                    if (flag3)
                    {
                        stringBuilder.AppendFormat(" src=\"{0}{1}\"", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                        {
                            Width = this.SrcSetStaticRatio.FirstOrDefault<KeyValuePair<string, int>>().Value
                        }).Replace(" ", "%20"), sc_lang);
                        stringBuilder.AppendFormat(" srcset=\"{0}\"", string.Join(", ", from width in this.SrcSetStaticRatio
                                                                                        select string.Format("{0}{2} {1}w", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                                                                                        {
                                                                                            Width = width.Value
                                                                                        }).Replace(" ", "%20"), width.Key, sc_lang)));
                    }
                    else
                    {
                        stringBuilder.AppendFormat(" src=\"{0}{1}\"", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                        {
                            Width = this.SrcSet.FirstOrDefault<int>()
                        }).Replace(" ", "%20"), sc_lang);
                        stringBuilder.AppendFormat(" srcset=\"{0}\"", string.Join(", ", from width in this.SrcSet
                                                                                        select string.Format("{0}{2} {1}w", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                                                                                        {
                                                                                            Width = width
                                                                                        }).Replace(" ", "%20"), width, sc_lang)));
                    }
                }
                foreach (KeyValuePair<string, string> current in this.Attributes)
                {
                    stringBuilder.AppendFormat(" {0}=\"{1}\"", current.Key, current.Value);
                }
                bool flag4 = !string.IsNullOrWhiteSpace(this.Sizes);
                if (flag4)
                {
                    stringBuilder.AppendFormat(" sizes=\"{0}\"", this.Sizes);
                }
                bool flag5 = !string.IsNullOrWhiteSpace(this.CssClass);
                if (flag5)
                {
                    stringBuilder.AppendFormat(" class=\"{0}\"", this.CssClass);
                }
                stringBuilder.Append("/>");
                result = stringBuilder.ToString();
            }
            return result;
        }

        new public string DataToTag(MediaItem item)
        {
            bool flag = item == null;
            string result;
            string sc_lang = string.IsNullOrWhiteSpace(this.Language) ? "" : "&sc_lang=" + this.Language;
            if (flag)
            {
                result = string.Empty;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder("<img");
                stringBuilder.AppendFormat(" alt=\"{0}\"", this.Alt ?? item.Alt);
                bool flag2 = this.SrcSetDeviceRatio != null && this.SrcSetDeviceRatio.ContainsKey("1x");
                if (flag2)
                {
                    stringBuilder.AppendFormat(" data-src=\"{0}{1}\"", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                    {
                        Width = this.SrcSetDeviceRatio["1x"]
                    }).Replace(" ", "%20"), sc_lang);
                    stringBuilder.AppendFormat(" data-srcset=\"{0}\"", string.Join(", ", from srcsetItem in this.SrcSetDeviceRatio
                                                                                         select string.Format("{0}{2} {1}", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                                                                                         {
                                                                                             Width = srcsetItem.Value
                                                                                         }).Replace(" ", "%20"), srcsetItem.Key, sc_lang)));
                }
                else
                {
                    stringBuilder.AppendFormat(" data-src=\"{0}{1}\"", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                    {
                        Width = this.SrcSet.FirstOrDefault<int>()
                    }).Replace(" ", "%20"), sc_lang);
                    stringBuilder.AppendFormat(" data-srcset=\"{0}\"", string.Join(", ", from width in this.SrcSet
                                                                                         select string.Format("{0}{2} {1}w", MediaManager.GetMediaUrl(item, new MediaUrlOptions
                                                                                         {
                                                                                             Width = width
                                                                                         }).Replace(" ", "%20"), width, sc_lang)));
                }
                foreach (KeyValuePair<string, string> current in this.Attributes)
                {
                    stringBuilder.AppendFormat(" {0}=\"{1}\"", current.Key, current.Value);
                }
                bool flag3 = !string.IsNullOrWhiteSpace(this.Sizes);
                if (flag3)
                {
                    stringBuilder.AppendFormat(" sizes=\"{0}\"", this.Sizes);
                }
                bool flag4 = !string.IsNullOrWhiteSpace(this.CssClass);
                if (flag4)
                {
                    stringBuilder.AppendFormat(" class=\"{0}\"", this.CssClass);
                }
                stringBuilder.Append("/>");
                result = stringBuilder.ToString();
            }
            return result;
        }

    }
}
