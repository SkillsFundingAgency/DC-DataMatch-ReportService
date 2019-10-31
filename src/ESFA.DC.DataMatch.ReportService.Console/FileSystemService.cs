using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using Path = System.IO.Path;

namespace ESFA.DC.DataMatch.ReportService.Console
{
    public sealed class FileSystemService : IStreamableKeyValuePersistenceService
    {
        private readonly string _directory;

        public FileSystemService(string directory)
        {
            _directory = directory;
        }

        public async Task SaveAsync(string key, string value, CancellationToken cancellationToken = new CancellationToken())
        {
            File.WriteAllText(Path.Combine(_directory, key), value);
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = new CancellationToken())
        {
            return File.ReadAllText(Path.Combine(_directory, key));
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = new CancellationToken())
        {
            File.Delete(Path.Combine(_directory, key));
        }

        public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = new CancellationToken())
        {
            return File.Exists(Path.Combine(_directory, key));
        }

        public async Task SaveAsync(string key, Stream value, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var fileStream = File.Create(Path.Combine(_directory, key)))
            {
                value.Seek(0, SeekOrigin.Begin);
                value.CopyTo(fileStream);
            }
        }

        public async Task GetAsync(string key, Stream value, CancellationToken cancellationToken = new CancellationToken())
        {
            var stream = new FileStream(Path.Combine(_directory, key), FileMode.Open);
            await stream.CopyToAsync(value);
        }
    }
}
