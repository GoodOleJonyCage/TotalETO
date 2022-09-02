using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using TotalETO.Models;
using TotalETO.ViewModels;

namespace TotalETO.Controllers
{
    [DefaultValue(SortValue.Default)]
    [Flags]
    public enum SortValue
    {
        [Description("Ascending")]
        Ascending = 0,
        [Description("Descending")]
        Descending = 1,
        [Description("Default")]
        Default = 2
    }

    [ApiController]
    [Route("[controller]")]
    public class ProductCatalogController : ControllerBase
    {
        [HttpGet(Name = "GetProductCatalog")]
        //[Route("ProductCatalogs/filter/{name?}/{productNum?}")]
        public IActionResult Get(
                                            string? name = "",
                                            string? productNum = "",
                                            decimal? cost = 0,
                                            decimal? weight = 0,
                                            DateTime? modifiedDate = null,
                                            string? productCategory = "",
                                            string? productDescription = "",

                                            //Dictionary<string, int> productCategoryDic = null,
                                            //Dictionary<string, int> productDescriptionDic = null,

                                            SortValue? nameSort = SortValue.Default,
                                            SortValue? productNumSort = SortValue.Default,
                                            SortValue? costSort = SortValue.Default,
                                            SortValue? weightSort = SortValue.Default,
                                            SortValue? modifiedDateSort = SortValue.Default  
                                            
                                        )
        {
            List<ProductCatalog> lst = new List<ProductCatalog>();

            const int NUMBER_OF_ITEMS_ON_PAGE = 10;
            int currentPage = 1;
            int totalCount;
            int totalpages;

            using (var dbcontext = new AdventureWorks2019Context())
            {
                var nonFilteredList = (from p in dbcontext.Products
                                       join ppp in dbcontext.ProductProductPhotos on p.ProductId equals ppp.ProductId
                                       join pp in dbcontext.ProductPhotos on ppp.ProductPhotoId equals pp.ProductPhotoId
                                       join pm in dbcontext.ProductModels on p.ProductModelId equals pm.ProductModelId
                                       join pmx in dbcontext.ProductModelProductDescriptionCultures on pm.ProductModelId equals pmx.ProductModelId
                                       join pd in dbcontext.ProductDescriptions on pmx.ProductDescriptionId equals pd.ProductDescriptionId
                                       join psc in dbcontext.ProductSubcategories on p.ProductSubcategoryId equals psc.ProductSubcategoryId
                                       join pc in dbcontext.ProductCategories on psc.ProductCategoryId equals pc.ProductCategoryId

                                       where (
                                                 p.Name == (string.IsNullOrEmpty(name) ? p.Name : name) &&
                                                 p.ProductNumber == (string.IsNullOrEmpty(productNum) ? p.ProductNumber : productNum) &&
                                                 p.StandardCost == (cost == 0 ? p.StandardCost : cost) &&
                                                 p.Weight == (weight == 0 ? p.Weight : weight) &&
                                                 p.ModifiedDate == (!modifiedDate.HasValue ? p.ModifiedDate : modifiedDate) &&
                                                 pc.Name == (string.IsNullOrEmpty(productCategory) ? pc.Name : productCategory) &&
                                                 pd.Description == (string.IsNullOrEmpty(productDescription) ? pd.Description : productDescription)
                                             )
                                       select new { p, pd, pc, pp }
                        )
                       //.Skip(NUMBER_OF_ITEMS_ON_PAGE * currentPage)
                       //.Take(NUMBER_OF_ITEMS_ON_PAGE)
                       .ToList();

                //pagination  
                totalCount = nonFilteredList.Count;
                totalpages = nonFilteredList.Count/ NUMBER_OF_ITEMS_ON_PAGE;
                nonFilteredList = nonFilteredList
                                    .Skip(NUMBER_OF_ITEMS_ON_PAGE * currentPage)
                                    .Take(NUMBER_OF_ITEMS_ON_PAGE).ToList();


                #region sorting

                //name
                switch (nameSort)
                {
                    case SortValue.Ascending:

                        nonFilteredList = nonFilteredList.OrderBy(x => x.p.Name).ToList();
                        break;

                    case SortValue.Descending:
                        nonFilteredList = nonFilteredList.OrderByDescending(x => x.p.Name).ToList();
                        break;
                }


                //product num
                switch (productNumSort)
                {
                    case SortValue.Ascending:

                        nonFilteredList = nonFilteredList.OrderBy(x => x.p.ProductNumber).ToList();
                        break;

                    case SortValue.Descending:
                        nonFilteredList = nonFilteredList.OrderByDescending(x => x.p.ProductNumber).ToList();
                        break;
                }

                //cost
                switch (costSort)
                {
                    case SortValue.Ascending:

                        nonFilteredList = nonFilteredList.OrderBy(x => x.p.StandardCost).ToList();
                        break;

                    case SortValue.Descending:
                        nonFilteredList = nonFilteredList.OrderByDescending(x => x.p.StandardCost).ToList();
                        break;
                }

                //weightSort
                switch (weightSort)
                {
                    case SortValue.Ascending:

                        nonFilteredList = nonFilteredList.OrderBy(x => x.p.Weight).ToList();
                        break;

                    case SortValue.Descending:
                        nonFilteredList = nonFilteredList.OrderByDescending(x => x.p.Weight).ToList();
                        break;
                }

                //modifiedDateSort
                switch (modifiedDateSort)
                {
                    case SortValue.Ascending:

                        nonFilteredList = nonFilteredList.OrderBy(x => x.p.ModifiedDate).ToList();
                        break;

                    case SortValue.Descending:
                        nonFilteredList = nonFilteredList.OrderByDescending(x => x.p.ModifiedDate).ToList();
                        break;
                }

                #endregion 

                nonFilteredList.ForEach(x =>
                {
                    lst.Add(new ProductCatalog()
                    {
                        Category = x.pc.Name,                           //product category
                        Description = x.pd.Description,                 //product description
                        Photo = x.pp.ThumbnailPhotoFileName ?? "N/A"    //product photo
                    });
                });
            }


            var ProdCatalog = new
            {
                currentPage = currentPage,
                pageSize = NUMBER_OF_ITEMS_ON_PAGE,
                totalCount = totalCount,
                totalPages = totalpages,
                Products = lst
            };

            return Ok(ProdCatalog);
        }

    }
}
