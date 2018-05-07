using System;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Volvo.Fxm.Infrastructure.EventHandlers
{
    public class FxmMultilanguageDuplicator
    {
        public Language Language { get; set; }
        public virtual void OnItemSaved(object sender, EventArgs args)
        {
            Language = LanguageManager.GetLanguage("da");

            Item item = Event.ExtractParameter(args, 0) as Item;

            Assert.ArgumentNotNull(item, "item");

            TargetFactory factory = new TargetFactory();

            if (factory.Exist(item.TemplateID.ToString()))
            {
                if (item.Language == Language) return;

                Target target = new TargetFactory().GetTarget(item.TemplateID.ToString());
                DuplicateFieldInLanguage(item, target.FieldName, Language);
            }
        }

        void DuplicateFieldInLanguage(Item item, string fieldName, Language language)
        {
            Item langItem = item.Database.GetItem(item.ID, language);
            var layoutField = item.Fields[fieldName];

            if (langItem.Versions.Count < 1)
            {
                langItem.Versions.AddVersion();
            }

            langItem.Editing.BeginEdit();
            using (new EditContext(langItem))
            {
                langItem.Fields[fieldName].Value = layoutField.Value;
            }
            langItem.Editing.EndEdit();
        }
    }

    public struct Target
    {
        public string Name { get; set; }
        public string TamplateId { get; set; }
        public string FieldName { get; set; }
    }

    public class TargetFactory
    {
        public List<Target> targets;

        public TargetFactory()
        {
            targets = new List<Target>
            {
                new Target { Name = "Page Filter", TamplateId = "{B889DE68-77E0-432C-9786-DE61D129D3DF}", FieldName = "Matcher Rule"},
                new Target { Name = "Element Placeholder", TamplateId = "{10E23679-55DB-4059-B8F2-E417A2F78FCB}", FieldName = "__Final Renderings"},
                new Target { Name = "Domain Matcher ", TamplateId = "{036DB470-1850-4848-A48A-0931F825B867}", FieldName = "Matcher Rule" }
            };
        }

        public bool Exist(string tamplateId)
        {
            return targets.Exists(x => x.TamplateId == tamplateId);
        }

        public Target GetTarget(string tamplateId)
        {
            return targets.First(x => x.TamplateId == tamplateId);
        }
    }
}
