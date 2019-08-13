using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using PDDeveloper.Plugin.ProductManagement.Data;

namespace PDDeveloper.Plugin.ProductManagement
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class ProductManagementPlugin : BasePlugin, IAdminMenuPlugin
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Language> _languageRepository;
        private readonly ProductManagementObjectContext _objectContext;
        private readonly INopFileProvider _fileProvider;
        #endregion

        #region Ctor
        public ProductManagementPlugin(ILocalizationService localizationService,
            IRepository<Language> languageRepository,
            ProductManagementObjectContext objectContext,
            INopFileProvider fileProvider)
        {
            this._localizationService = localizationService;
            this._languageRepository = languageRepository;
            this._objectContext = objectContext;
            this._fileProvider = fileProvider;
        }
        #endregion

        #region Utilities
        /// <summary>
        ///Import Resource string from xml and save
        /// </summary>
        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var language = _languageRepository.Table.Single(l => l.Name == "English");

            //save resources
            foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath(ProductManagementDefaults.ResourceFilePath),
                "ResourceString.xml", SearchOption.TopDirectoryOnly))
            {
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                using (var streamReader = new StreamReader(filePath))
                {
                    localizationService.ImportResourcesFromXml(language, streamReader);
                }
            }
        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual void DeleteLocalResources()
        {
            var file = Path.Combine(_fileProvider.MapPath(ProductManagementDefaults.ResourceFilePath), "ResourceString.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                _localizationService.DeletePluginLocaleResource(item);
            }
        }
        #endregion

        #region Methods
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
        #endregion
    }
}