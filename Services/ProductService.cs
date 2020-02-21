using System.Collections.Generic;
using Microsoft.Extensions.Options;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAll();
    }

    public class ProductService : IProductService {

        private List<Product> _product = new List<Product>
        { 
            new Product { Id = 1, Name = "Gumboot", Url= "https://picsum.photos/id/122//250/150", Description = "Some quick example text to build on the card title and make up the bulk of the card's content." },
            new Product { Id = 2, Name = "Sweaters", Url= "https://picsum.photos//id/211/250/150", Description = "Some quick example text to build on the card title and make up the bulk of the card's content." },
            new Product { Id = 3, Name = "Gumboot", Url= "https://picsum.photos/id/133//250/150", Description = "Some quick example text to build on the card title and make up the bulk of the card's content." },
        };

        public ProductService()
        {
        }

        public IEnumerable<Product> GetAll()
        {
            return _product;
        }
    }
}