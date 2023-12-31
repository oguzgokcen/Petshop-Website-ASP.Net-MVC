﻿using Microsoft.CodeAnalysis.CSharp;
using Proje.Models;

namespace Proje.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts(decimal maxprice,bool stock, string sterm = "");
        Task<Product> Find(int id);
    }
}
