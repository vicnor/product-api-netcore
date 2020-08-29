﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using product_api_netcore.Models;
using System;
using System.Linq;

namespace product_api_netcore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {

        private readonly ILogger<ProductsController> _logger;

        private readonly ProductContext _context;

        public ProductsController(ProductContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;

            if (_context.Products.Any()) return;

            ProductSeed.InitData(context);
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IQueryable<Product>> GetProducts([FromQuery] ProductRequest request)
        {
            if (request.Limit >= 100)
                _logger.LogInformation("Requesting more than 100 products.");

            var result = _context.Products as IQueryable<Product>;

            Response.Headers["x-total-count"] = result.Count().ToString();

            return Ok(result
              .OrderBy(p => p.ProductNumber)
              .Skip(request.Offset)
              .Take(request.Limit));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostProduct([FromBody] Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();

                return new CreatedResult($"/products/{product.ProductNumber.ToLower()}", product);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to POST product.");

                return ValidationProblem(e.Message);
            }
        }

        [HttpGet]
        [Route("product-number/{productNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> GetProductByProductNumber([FromRoute] string productNumber)
        {
            var productDb = _context.Products
              .FirstOrDefault(p => p.ProductNumber.Equals(productNumber,
                        StringComparison.InvariantCultureIgnoreCase));

            if (productDb == null) return NotFound();

            return Ok(productDb);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> GetProductById([FromRoute] int id)
        {
            var productDb = _context.Products
              .FirstOrDefault(p => p.Id == id);

            if (productDb == null) return NotFound();

            return Ok(productDb);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PutProduct([FromBody] Product product)
        {
            try
            {
                var productDb = _context.Products
                  .FirstOrDefault(p => p.Id == product.Id);

                if (productDb == null) return NotFound();

                productDb.Name = product.Name;
                productDb.Price = product.Price;
                productDb.Department = product.Department;
                _context.SaveChanges();

                return Ok(product);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to PUT product.");

                return ValidationProblem(e.Message);
            }
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