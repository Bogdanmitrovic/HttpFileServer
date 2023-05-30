namespace SysProgZadatak6
{
    public class FileCache
    {
        struct CacheStruct
        {
            public byte[] Content;
            public DateTime CreationTime;
        }
        private static readonly Log Log = Log.Instance;
        private readonly Dictionary<string, CacheStruct> _cache;
        private static readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim();
        private int _timeToLive; // min

        public FileCache(int timeToLive = 1)
        {
            _cache = new Dictionary<string, CacheStruct>();
            _timeToLive = timeToLive;
            
        }
        public byte[]? GetFile(string filename)
        {
            CacheLock.EnterReadLock();
            try
            {
                if (!_cache.TryGetValue(filename, out var value)) return null;
                DateTime expTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(_timeToLive));
                if (value.CreationTime > expTime)
                {
                    Log.MessageLog("File " + filename +" found in cache!");
                    return value.Content;
                }
                return null;
            }
            catch (Exception e)
            {
                Log.ExceptionLog(e);
                return null;
            }
            finally
            {
                CacheLock.ExitReadLock();
            }

        }

        public byte[]? LoadFile(string filename)
        {
            if (!File.Exists(filename))
                return null;
            var buffer = File.ReadAllBytes(filename);
            ThreadPool.QueueUserWorkItem(_ => {

                CacheLock.EnterWriteLock();
                try
                {
                    CacheStruct cacheStruct = new CacheStruct
                    {
                        Content = buffer,
                        CreationTime = DateTime.Now
                    };
                    _cache[filename] = cacheStruct;
                    Log.MessageLog("File "+ filename + " added to cache");
                }
                catch (Exception e)
                {
                    Log.ExceptionLog(e);
                }
                finally
                {
                    CacheLock.ExitWriteLock();
                }
            });
            return buffer;
        }
    }
}
