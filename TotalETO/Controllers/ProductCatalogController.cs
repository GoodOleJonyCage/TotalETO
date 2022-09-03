using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        //[Route("GetProductCatalog/filter/{name?}/{productNum?}/{cost?}/{weight?}/{modifiedDate?}/{productCategory?}/{productDescription?}")]
        public IActionResult Get(
                                            string? name = "",
                                            string? productNum = "",
                                            decimal? cost = 0,
                                            decimal? weight = 0,
                                            DateTime? modifiedDate = null,
                                            string? productCategory = "",
                                            string? productDescription = "",

                                            SortValue? nameSort = SortValue.Default,
                                            SortValue? productNumSort = SortValue.Default,
                                            SortValue? costSort = SortValue.Default,
                                            SortValue? weightSort = SortValue.Default,
                                            SortValue? modifiedDateSort = SortValue.Default  ,
                                            SortValue? productcategorySort = SortValue.Default,

                                            int? pageNumber = 1,
                                            int? pageSize = 10

                                        )
        {
            List<ProductCatalog> productList = new List<ProductCatalog>();

            int NUMBER_OF_ITEMS_ON_PAGE = pageSize.Value;
            int currentPage = pageNumber.HasValue ? pageNumber.Value : 1; 
            int totalCount;
            int totalpages;

            using (var dbcontext = new AdventureWorks2019Context())
            {

                # region check productCategory and productDescription for valid values

                //is valid productCategory
                if ( !string.IsNullOrEmpty(productCategory) && !dbcontext.ProductCategories.Any( x => x.Name.ToLower() == productCategory.ToLower()))
                {
                    return BadRequest("Invalid productCategory value");
                }
                //is valid productDescription
                if (!string.IsNullOrEmpty(productDescription) && !dbcontext.ProductDescriptions.Any(x => x.Description.ToLower() == productDescription.ToLower()))
                {
                    return BadRequest("Invalid productDescription value");
                }

                #endregion 

                var nonPagedList = (from p in dbcontext.Products
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
                       .ToList();

                # region pagination
                totalCount = nonPagedList.Count;
                totalpages = nonPagedList.Count/ NUMBER_OF_ITEMS_ON_PAGE;
                //check for valid page number
                if(NUMBER_OF_ITEMS_ON_PAGE > nonPagedList.Count)
                {
                    return BadRequest("Invalid pageSize value");
                }
                if (currentPage > totalpages)
                {
                    return BadRequest("Invalid pageNumber value");
                }
                
                var pagedList = nonPagedList
                                    .Skip(NUMBER_OF_ITEMS_ON_PAGE * currentPage)
                                    .Take(NUMBER_OF_ITEMS_ON_PAGE).ToList();
                #endregion 

                #region sorting
                var sortedList = pagedList.ToList();
                //name
                switch (nameSort)
                {
                    case SortValue.Ascending:

                        sortedList = pagedList.OrderBy(x => x.p.Name).ToList();
                        break;

                    case SortValue.Descending:
                        sortedList = pagedList.OrderByDescending(x => x.p.Name).ToList();
                        break;
                }


                //product num
                switch (productNumSort)
                {
                    case SortValue.Ascending:

                        sortedList = pagedList.OrderBy(x => x.p.ProductNumber).ToList();
                        break;

                    case SortValue.Descending:
                        sortedList = pagedList.OrderByDescending(x => x.p.ProductNumber).ToList();
                        break;
                }

                //cost
                switch (costSort)
                {
                    case SortValue.Ascending:

                        sortedList = pagedList.OrderBy(x => x.p.StandardCost).ToList();
                        break;

                    case SortValue.Descending:
                        sortedList = pagedList.OrderByDescending(x => x.p.StandardCost).ToList();
                        break;
                }

                //weightSort
                switch (weightSort)
                {
                    case SortValue.Ascending:

                        sortedList = pagedList.OrderBy(x => x.p.Weight).ToList();
                        break;

                    case SortValue.Descending:
                        sortedList = pagedList.OrderByDescending(x => x.p.Weight).ToList();
                        break;
                }

                //modifiedDateSort
                switch (modifiedDateSort)
                {
                    case SortValue.Ascending:

                        sortedList = pagedList.OrderBy(x => x.p.ModifiedDate).ToList();
                        break;

                    case SortValue.Descending:
                        sortedList = pagedList.OrderByDescending(x => x.p.ModifiedDate).ToList();
                        break;
                }

                //productcategory
                switch (productcategorySort)
                {
                    case SortValue.Ascending:

                        sortedList = pagedList.OrderBy(x => x.pc.Name).ToList();
                        break;

                    case SortValue.Descending:
                        sortedList = pagedList.OrderByDescending(x => x.pc.Name).ToList();
                        break;
                }



                #endregion 

                //creating client viewmodel
                sortedList.ForEach(x =>
                {
                    productList.Add(new ProductCatalog()
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
                Products = productList
            };

            return Ok(ProdCatalog);
            
        }

    }
}
