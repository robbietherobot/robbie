using System.Web.Mvc;
using RobbieBehaviour.Models;
using Sitecore.Data.Items;
using Sitecore.Mvc.Presentation;
using Sitecore.Data.Fields;
using Sitecore.Resources.Media;

namespace RobbieBehaviour.Controllers
{
    public class DefaultController : Controller
    {

        public Item RenderingDataSource
        {
            get
            {
                if (RenderingContext.CurrentOrNull != null
                    && RenderingContext.Current.Rendering.Item != null
                    && RenderingContext.Current.Rendering.Item.ID != Sitecore.Context.Item.ID)
                {
                    return RenderingContext.Current.Rendering.Item;
                }

                return Sitecore.Context.Item;
            }
        }

        public LinkImage LinkImageModel
        {
            get
            {
                var viewModel = new LinkImage();

                var imageField = (ImageField)RenderingDataSource?.Fields["Image"];
                if (imageField?.MediaItem != null)
                {
                    viewModel.ImageAlt = imageField.Alt;
                    viewModel.ImageSrc = MediaManager.GetMediaUrl(imageField.MediaItem);
                }

                var linkField = (LinkField) RenderingDataSource?.Fields["Link"];
                if (linkField != null)
                {
                    viewModel.LinkSrc = linkField.GetFriendlyUrl();
                    viewModel.LinkText = linkField.Text;
                    viewModel.LinkTarget = linkField.Target;
                }

                return viewModel;
            }
        }

        public ActionResult Logo()
        {
            var datasource = RenderingDataSource;
            if (datasource == null)
            {
                return View("~/Views/no-datasource.cshtml");
            }

            return View("~/Views/logo.cshtml", LinkImageModel);
        }

        public ActionResult Navigation()
        {
            return View("~/Views/navigation.cshtml");
        }

        public ActionResult HeroBanner()
        {
            return View("~/Views/hero-banner.cshtml", LinkImageModel);
        }

        public ActionResult HeadlessResponse()
        {
            return View("~/Views/headless-response.cshtml");
        }

        public ActionResult ContentStack()
        {
            var datasource = RenderingDataSource;
            if (datasource == null)
            {
                return View("~/Views/no-datasource.cshtml");
            }

            return View("~/Views/content-stack.cshtml", LinkImageModel);
        }

        public ActionResult FooterContent()
        {
            var datasource = RenderingDataSource;
            if (datasource == null)
            {
                return View("~/Views/no-datasource.cshtml");
            }

            return View("~/Views/footer-content.cshtml", LinkImageModel);
        }

        public ActionResult FooterLinks()
        {
            return View("~/Views/footer-links.cshtml");
        }
       
    }
}