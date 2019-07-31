using PDDeveloper.Plugin.ProductManagement.Data;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PDDeveloper.Plugin.ProductManagement
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class ProductManagementPlugin : BasePlugin, IAdminMenuPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Language> _languageRepository;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ProductManagementObjectContext _objectContext;
        private readonly INopFileProvider _fileProvider;

        public ProductManagementPlugin(ILocalizationService localizationService,
            IRepository<Language> languageRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            ProductManagementObjectContext objectContext,
            INopFileProvider fileProvider)
        {
            this._localizationService = localizationService;
            this._languageRepository = languageRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._objectContext = objectContext;
            this._fileProvider = fileProvider;
        }
        
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //database objects
            _objectContext.Install();

            //locales
            InstallLocaleResources();

            base.Install();
        }
        
        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //database objects
            _objectContext.Uninstall();

            //locales
            DeleteLocalResources();

            base.Uninstall();
            
        }

        /// <summary>
        /// Add menu in admin area base on condition if PDD is available then add chield menu in that or create PDD menu also
        /// </summary>
        /// <param name="rootNode">Root menu of admin area</param>
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "PDDeveloper.ProductManagement",
                Title = _localizationService.GetResource("Plugins.PDD.ProductManagement.Menu.Root.ProductManagement"),
                ControllerName = "ProductSegment",
                ActionName = "List",
                Visible = true,
                IconClass = "fa fa-dot-circle-o",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "PDDeveloper");
            if (pluginNode != null)
            {
                pluginNode.ChildNodes.Add(menuItem);
            }
            else
            {
                var pddRootNode = new SiteMapNode()
                {
                    SystemName = "PDDeveloper",
                    Title = _localizationService.GetResource("Plugins.PDD.ProductManagement.Menu.Root"),
                    Visible = true,
                    IconClass = "fa fa-cube",
                    RouteValues = new RouteValueDictionary() { { "area", "Admin" } }
                };
                pddRootNode.ChildNodes.Add(menuItem);

                rootNode.ChildNodes.Add(pddRootNode);
            }
        }

        /// <summary>
        ///Import Resource string from xml and save
        /// </summary>
        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var language = _languageRepository.Table.FirstOrDefault(l => l.Name == "English");

            //save resources
            foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath("~/Plugins/PDD.Plugin.ProductManagement/Localization/ResourceString"),
                "ResourceString.xml", SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                localizationService.ImportResourcesFromXml(language, localesXml);
            }
        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual void DeleteLocalResources()
        {
            var file = Path.Combine(_fileProvider.MapPath("~/Plugins/PDD.Plugin.ProductManagement/Localization/ResourceString"), "ResourceString.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                _localizationService.DeletePluginLocaleResource(item);
            }
        }

    }
}