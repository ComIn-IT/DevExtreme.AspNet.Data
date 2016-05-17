﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.Models;
using DevExtreme.AspNet.Data;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Sample.Controllers {

    [Route("nwind")]
    public class NorthwindController : Controller {
        NorthwindContext _nwind;

        public NorthwindController(NorthwindContext nwind) {
            _nwind = nwind;
        }

#warning TODO why model binder doesn't work on class?

        [HttpGet("orders")]
        public object Orders([ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))] DataSourceLoadOptions loadOptions) {
            return DataSourceLoader.Load(_nwind.Orders, loadOptions);
        }

        [HttpGet("order-details")]
        public object OrderDetails(int orderID, [ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))] DataSourceLoadOptions options) {
            return DataSourceLoader.Load(
                from i in _nwind.Order_Details
                where i.OrderID == orderID
                select new {
                    Product = i.Product.ProductName,
                    Price = i.UnitPrice,
                    Quantity = i.Quantity,
                    Sum = i.UnitPrice * i.Quantity
                },
                options
            );
        }

        [HttpGet("customers-lookup")]
        public object CustomersLookup([ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))] DataSourceLoadOptions options) {
            return DataSourceLoader.Load(
                from c in _nwind.Customers orderby c.CompanyName select new {
                    Value = c.CustomerID,
                    Text = $"{c.CompanyName} ({c.Country})"
                },
                options
            );
        }

        [HttpGet("shippers-lookup")]
        public object ShippersLookup([ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))] DataSourceLoadOptions options) {
            return DataSourceLoader.Load(
                from s in _nwind.Shippers orderby s.CompanyName select new {
                    Value = s.ShipperID,
                    Text = s.CompanyName
                },
                options
            );
        }

        [HttpPut("update-order")]
        public IActionResult UpdateOrder(int key, string values) {
            var order = _nwind.Orders.First(o => o.OrderID == key);
            JsonConvert.PopulateObject(values, order);

            if(!TryValidateModel(order))
                return BadRequest(ModelState.ToFullErrorString());

            _nwind.SaveChanges();

            return Ok();
        }

        [HttpPost("insert-order")]
        public IActionResult InsertOrder(string values) {
            var order = new Order();
            JsonConvert.PopulateObject(values, order);

            if(!TryValidateModel(order))
                return BadRequest(ModelState.ToFullErrorString());

            _nwind.Orders.Add(order);
            _nwind.SaveChanges();

            return Ok();
        }

        [HttpDelete("delete-order")]
        public void DeleteOrder(int key) {
            var order = _nwind.Orders.First(o => o.OrderID == key);
            _nwind.Orders.Remove(order);
            _nwind.SaveChanges();
        }

        [HttpGet("products")]
        public object Products([ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))] DataSourceLoadOptions loadOptions) {
            return DataSourceLoader.Load(
                _nwind.Products.Include(p => p.Category), 
                loadOptions
            );
        }

    }
}
