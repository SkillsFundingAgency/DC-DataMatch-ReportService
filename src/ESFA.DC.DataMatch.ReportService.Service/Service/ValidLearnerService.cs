using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class ValidLearnerService : IValidLearnersService
    {
        private readonly string _filename;
        private readonly IReportServiceConfiguration _reportServiceConfiguration;
        private readonly IStreamableKeyValuePersistenceService _storage;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly SemaphoreSlim _getDataLock = new SemaphoreSlim(1, 1);
        private bool _loadedDataAlready;
        private List<Learner> _loadedData;
        private readonly ILogger _logger;
        public ValidLearnerService(
            string key,
            ILogger logger,
            IReportServiceConfiguration reportServiceConfiguration,
            IJsonSerializationService jsonSerializationService,
            IStreamableKeyValuePersistenceService storage)
        {
            _filename = key;
            _storage = storage;
            _logger = logger;
            _reportServiceConfiguration = reportServiceConfiguration;
            _jsonSerializationService = jsonSerializationService;
        }

        public async Task<List<Learner>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _loadedData;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                _loadedDataAlready = true;
                int ukPrn = reportServiceContext.Ukprn;

                if (await _storage.ContainsAsync(_filename, cancellationToken))
                {
                    string learnersValidStr = await _storage.GetAsync(_filename, cancellationToken);
                    _loadedData = _jsonSerializationService.Deserialize<List<Learner>>(learnersValidStr);
                }
                else
                {
                    var validLearnersList = new List<Learner>();

                    DbContextOptions<ILR1819_DataStoreEntitiesValid> validContextOptions = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>().UseSqlServer(_reportServiceConfiguration.ILRDataStoreValidConnectionString).Options;
                    using (var ilrValidContext = new ILR1819_DataStoreEntitiesValid(validContextOptions))
                    {
                        validLearnersList = ilrValidContext.Learners.Where(x => x.UKPRN == ukPrn).ToList(); //.Select(x => x.LearnRefNumber).ToList();
                    }

                    _loadedData = validLearnersList;
                }
            }
            catch (Exception ex)
            {
                // Todo: Check behaviour
                _logger.LogError($"Failed to get learners for  {reportServiceContext.Ukprn}", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _loadedData;
        }
    }
}
