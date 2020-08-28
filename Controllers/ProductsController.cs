﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using product_api_netcore.Models;
using System.Linq;

namespace product_api_netcore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {

        private readonly ProductContext _context;

        public ProductsController(ProductContext context)
        {
            _context = context;

            if (_context.Products.Any()) return;

            ProductSeed.InitData(context);
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IQueryable<Product>> GetProducts([FromQuery] ProductRequest request)
        {
            var result = _context.Products as IQueryable<Product>;

            Response.Headers["x-total-count"] = result.Count().ToString();

            return Ok(result
              .OrderBy(p => p.ProductNumber)
              .Skip(request.Offset)
              .Take(request.Limit));
        }

        public class ProductRequest
        {
            [FromQuery(Name = "limit")]
            public int Limit { get; set; } = 100;

            [FromQuery(Name = "offset")]
            public int Offset { get; set; }
        }
    }
}