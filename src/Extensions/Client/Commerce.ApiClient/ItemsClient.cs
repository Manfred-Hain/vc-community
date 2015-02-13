﻿namespace VirtoCommerce.ApiClient
{
    #region

    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using VirtoCommerce.ApiClient.Utilities;
    using VirtoCommerce.Web.Core.DataContracts;

    #endregion

    public class ItemsClient : BaseClient
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the AdminManagementClient class.
        /// </summary>
        /// <param name="adminBaseEndpoint">Admin endpoint</param>
        /// <param name="token">Access token</param>
        public ItemsClient(Uri adminBaseEndpoint, string token)
            : base(adminBaseEndpoint, new TokenMessageProcessingHandler(token))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the AdminManagementClient class.
        /// </summary>
        /// <param name="adminBaseEndpoint">Admin endpoint</param>
        /// <param name="token">Access token</param>
        public ItemsClient(Uri adminBaseEndpoint, MessageProcessingHandler handler)
            : base(adminBaseEndpoint, handler)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     List items matching the given query
        /// </summary>
        public Task AddAsync(string categoryId, Product product)
        {
            var requestUri = this.CreateRequestUri(String.Format(RelativePaths.AddProduct, categoryId));
            return SendAsync(requestUri, HttpMethod.Post, product);
        }

        public Task DeleteAsync(string productId)
        {
            var requestUri = this.CreateRequestUri(String.Format(RelativePaths.DeleteProduct, productId));
            return this.SendAsync(requestUri, HttpMethod.Delete);
        }

        public Task UpdateAsync(string categoryId, Product product)
        {
            var requestUri = this.CreateRequestUri(String.Format(RelativePaths.UpdateProduct, categoryId));
            return SendAsync(requestUri, new HttpMethod("PATCH"), product);
        }

        #endregion

        protected class RelativePaths
        {
            #region Constants

            public const string AddProduct = "products/{0}";

            public const string DeleteProduct = "products/{0}";

            public const string UpdateProduct = "products/{0}";

            #endregion
        }
    }
}