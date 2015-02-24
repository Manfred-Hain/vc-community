﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using foundationModel = VirtoCommerce.Foundation.Catalogs.Model;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Foundation.Data.Infrastructure;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Converters;
using System.Data.Entity;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Foundation.Money;

namespace VirtoCommerce.PricingModule.Data.Services
{
	public class PricingServiceImpl : ServiceBase, IPricingService
	{
		private readonly Func<IFoundationPricingRepository> _repositoryFactory;
		public PricingServiceImpl(Func<IFoundationPricingRepository> repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		#region IPricingService Members

		public IEnumerable<coreModel.Price> EvaluateProductPrices(coreModel.PriceEvaluationContext evalContext)
		{
			if(evalContext == null)
			{
				throw new ArgumentNullException("evalContext");
			}
			if(evalContext.ProductId == null)
			{
				throw new MissingFieldException("ProductId");
			}

			var retVal = new List<coreModel.Price>();
			using (var repository = _repositoryFactory())
			{
				var prices = repository.Prices.Include(x => x.Pricelist).Where(x => x.ItemId == evalContext.ProductId)
											  .ToArray()
											  .Select(x => x.ToCoreModel());
											
				foreach (var groupItem in prices.GroupBy(x=>x.Currency))
				{
					var activePice = groupItem.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
					retVal.Add(activePice);
				}
			}
			return retVal;
		}

		public virtual coreModel.Price GetPriceById(string id)
		{
			coreModel.Price retVal = null;
			using (var repository = _repositoryFactory())
			{
				var entity = repository.GetPriceById(id);
				if (entity != null)
				{
					retVal = entity.ToCoreModel();
				}
			}
			return retVal;
		}


		public IEnumerable<coreModel.Pricelist> GetPriceLists()
		{
			List<coreModel.Pricelist> retVal = null;
			using (var repository = _repositoryFactory())
			{
				retVal = repository.Pricelists.ToArray().Select(x => x.ToCoreModel()).ToList();
			}
			return retVal;
		}

		public virtual coreModel.Pricelist GetPricelistById(string id)
		{
			coreModel.Pricelist retVal = null;
			using (var repository = _repositoryFactory())
			{
				var entity = repository.GetPricelistById(id);
				if (entity != null)
				{
					retVal = entity.ToCoreModel();
				}
			}
			return retVal;
		}

		public virtual coreModel.Price CreatePrice(coreModel.Price price)
		{
			var entity = price.ToFoundation();
			coreModel.Price retVal = null;
			using (var repository = _repositoryFactory())
			{
				if (price.PricelistId == null)
				{
					var defaultPriceListId = GetDefaultPriceListName(price.Currency);
					var foundationDefaultPriceList = repository.GetPricelistById(defaultPriceListId);
					if (foundationDefaultPriceList == null)
					{
						var defaultPriceList = new coreModel.Pricelist
						{
							Id = defaultPriceListId,
							Currency = price.Currency,
							Name = defaultPriceListId,
							Description = defaultPriceListId
						};
						foundationDefaultPriceList = defaultPriceList.ToFoundation();
					}
					entity.Pricelist = foundationDefaultPriceList;
				}
				repository.Add(entity);
				CommitChanges(repository);
			}
			retVal = GetPriceById(price.Id);
			return retVal;
		}

		public virtual coreModel.Pricelist CreatePricelist(coreModel.Pricelist priceList)
		{
			var entity = priceList.ToFoundation();
			coreModel.Pricelist retVal = null;
			using (var repository = _repositoryFactory())
			{
				repository.Add(entity);
				CommitChanges(repository);
			}
			retVal = GetPricelistById(priceList.Id);
			return retVal;
		}

		public virtual void UpdatePrices(coreModel.Price[] prices)
		{
			using (var repository = _repositoryFactory())
			using (var changeTracker = base.GetChangeTracker(repository))
			{
				foreach (var price in prices)
				{
					var sourceEntity = price.ToFoundation();
					var targetEntity = repository.GetPriceById(price.Id);
					if (targetEntity == null)
					{
						throw new NullReferenceException("targetEntity");
					}

					changeTracker.Attach(targetEntity);
					sourceEntity.Patch(targetEntity);
				}
				CommitChanges(repository);
			}
		}

		public virtual void UpdatePricelists(coreModel.Pricelist[] priceLists)
		{
			using (var repository = _repositoryFactory())
			using (var changeTracker = base.GetChangeTracker(repository))
			{
				foreach (var priceList in priceLists)
				{
					var sourceEntity = priceList.ToFoundation();
					var targetEntity = repository.GetPricelistById(priceList.Id);
					if (targetEntity == null)
					{
						throw new NullReferenceException("targetEntity");
					}

					changeTracker.Attach(targetEntity);
					sourceEntity.Patch(targetEntity);
				}
				CommitChanges(repository);
			}
		}

		public virtual void DeletePrices(string[] ids)
		{
			GenericDelete(ids, (repository, id) => repository.GetPriceById(id));
		}
		public virtual void DeletePricelists(string[] ids)
		{
			GenericDelete(ids, (repository, id) => repository.GetPricelistById(id));
		}
		#endregion

		private static string GetDefaultPriceListName(CurrencyCodes currency)
		{
			var retVal = "Default" + currency.ToString();
			return retVal;
		}
		private void GenericDelete(string[] ids, Func<IFoundationPricingRepository, string, StorageEntity> getter)
		{
			using (var repository = _repositoryFactory())
			{
				foreach (var id in ids)
				{
					var entity = getter(repository, id);
					repository.Remove(entity);
				}
				CommitChanges(repository);
			}
		}
	}
}