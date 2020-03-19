﻿using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Routing;
using Foundation.Cms;
using Foundation.Commerce.Catalog.ViewModels;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Extensions;
using Foundation.Commerce.Models.Catalog;
using Foundation.Commerce.Personalization;
using Foundation.Demo.ViewModels;
using Foundation.Social.Services;
using Mediachase.Commerce.Catalog;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Features.CatalogContent.Package
{
    public class PackageController : CatalogContentControllerBase<GenericPackage>
    {
        private readonly bool _isInEditMode;
        private readonly CatalogEntryViewModelFactory _viewModelFactory;

        public PackageController(IsInEditModeAccessor isInEditModeAccessor,
            CatalogEntryViewModelFactory viewModelFactory,
            IReviewService reviewService,
            IReviewActivityService reviewActivityService,
            ICommerceTrackingService recommendationService,
            ReferenceConverter referenceConverter,
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            ILoyaltyService loyaltyService) : base(referenceConverter, contentLoader, urlResolver, reviewService, reviewActivityService, recommendationService, loyaltyService)
        {
            _isInEditMode = isInEditModeAccessor();
            _viewModelFactory = viewModelFactory;
        }

        [HttpGet]
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public async Task<ActionResult> Index(GenericPackage currentContent, bool skipTracking = false)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            var viewModel = _viewModelFactory.CreatePackage<GenericPackage, GenericVariant, DemoGenericPackageViewModel>(currentContent);
            viewModel.BreadCrumb = GetBreadCrumb(currentContent.Code);
            if (_isInEditMode && !viewModel.Entries.Any())
            {
                return View(viewModel);
            }

            if (viewModel.Entries == null || !viewModel.Entries.Any())
            {
                return HttpNotFound();
            }

            await AddInfomationViewModelAsync(viewModel, currentContent.Code, skipTracking);
            currentContent.AddBrowseHistory();

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult QuickView(string productCode)
        {
            var currentContentRef = _referenceConverter.GetContentLink(productCode);
            if (_contentLoader.Get<EntryContentBase>(currentContentRef) is GenericPackage currentContent)
            {
                var viewModel = _viewModelFactory.CreatePackage<GenericPackage, GenericVariant, GenericPackageViewModel>(currentContent);
                return PartialView("_QuickView", viewModel);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Product not found.");
        }
    }
}