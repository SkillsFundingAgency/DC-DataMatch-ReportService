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
    /// <summary>
    /// valid learners service
    /// </summary>
    public sealed class ValidLearnerService : IValidLearnersService
    {
        private readonly string _filename;
        private readonly IReportServiceConfiguration _reportServiceConfiguration;
        private readonly IStreamableKeyValuePersistenceService _storage;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly SemaphoreSlim _getDataLock = new SemaphoreSlim(1, 1);
        private bool _loadedDataAlready;
        private List<string> _loadedDataLearnRefNumbers;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidLearnerService"/> class.
        /// constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="logger"></param>
        /// <param name="reportServiceConfiguration"></param>
        /// <param name="jsonSerializationService"></param>
        /// <param name="storage"></param>
        public ValidLearnerService(
            ILogger logger,
            IReportServiceConfiguration reportServiceConfiguration,
            IJsonSerializationService jsonSerializationService,
            IStreamableKeyValuePersistenceService storage)
        {
            _storage = storage;
            _logger = logger;
            _reportServiceConfiguration = reportServiceConfiguration;
            _jsonSerializationService = jsonSerializationService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reportServiceContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _loadedDataLearnRefNumbers;
                }

                //if (cancellationToken.IsCancellationRequested)
                //{
                //    return null;
                //}

                _loadedDataAlready = true;
                int ukPrn = reportServiceContext.Ukprn;
                //if (await _storage.ContainsAsync(_filename, cancellationToken))
                //{
                //    string learnersValidStr = await _storage.GetAsync(_filename, cancellationToken);
                //    _loadedDataLearnRefNumbers = _jsonSerializationService.Deserialize<List<Learner>>(learnersValidStr);
                //}
                //else
                //{
                var validLearnersList = new List<string>();
                DbContextOptions<ILR1819_DataStoreEntitiesValid> validContextOptions = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>().UseSqlServer(_reportServiceConfiguration.ILR1819DataStoreValidConnectionString).Options;
                using (var ilrValidContext = new ILR1819_DataStoreEntitiesValid(validContextOptions))
                {
                    validLearnersList = ilrValidContext.Learners.Where(x => x.UKPRN == ukPrn).Select(x => x.LearnRefNumber).ToList();
                }

                _loadedDataLearnRefNumbers = validLearnersList;
                //}
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

            return _loadedDataLearnRefNumbers;
        }
    }
}
