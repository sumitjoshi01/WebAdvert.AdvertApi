using AdvertApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace AdvertApi.Services
{
    public class DynamoDbAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;

        public DynamoDbAdvertStorage(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = _mapper.Map<AdvertDbModel>(model);
            dbModel.Id = Guid.NewGuid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);

            await context.SaveAsync(dbModel);

            return dbModel.Id;
        }

        public async Task ConfirmModel(ConfirmAdvertModel model)
        {
            using var client = new AmazonDynamoDBClient();
            var context = new DynamoDBContext(client);
            var record = await context.LoadAsync<AdvertDbModel>(model.Id);
            if (record == null)
            {
                throw new KeyNotFoundException($"Record with ID={model.Id} was not found");
            }

            if (model.Status == AdvertStatus.Active)
            {
                record.Status = AdvertStatus.Active;
                await context.SaveAsync(record);
            }
            else
            {
                await context.DeleteAsync(record);
            }
        }
    }
}
