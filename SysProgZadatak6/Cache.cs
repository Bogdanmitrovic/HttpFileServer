using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysProgZadatak6
{
    public class FileCache
    {
        struct CacheStruct
        {
            public byte[] content;
            public DateTime CreationTime;
        }
        private static Log _log;
        private Dictionary<string, CacheStruct> cache;
        static ReaderWriterLockSlim cacheLock;
        private int timeToLive; // min

        public FileCache(int timeToLive = 1) 
        {
            cache = new Dictionary<string, CacheStruct>();
            this.timeToLive = timeToLive;
            cacheLock = new ReaderWriterLockSlim();
            _log = Log.Instance;
        }
        
        public byte[] GetFile(string filename)
        {
            cacheLock.EnterReadLock();
            try
            {
                if(cache.TryGetValue(filename,out var value))
                {
                    DateTime expTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(10000)/*TimeSpan.FromMinutes(timeToLive)*/);
                    if (value.CreationTime > expTime)
                    {
                        _log.MessageLog("File " + filename +" found in cache!");
                        return value.content;
                    }
                    
                }
                return null;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

        }

        public byte[] LoadFile(string filename)
        {
            if (!File.Exists(filename))
                return null;
            var buffer = File.ReadAllBytes(filename);
            ThreadPool.QueueUserWorkItem(_ => {

                cacheLock.EnterWriteLock();
                try
                {
                    CacheStruct cacheStruct = new CacheStruct();
                    cacheStruct.content = buffer;
                    cacheStruct.CreationTime = DateTime.Now;
                    cache[filename] = cacheStruct;
                    _log.MessageLog("File "+ filename + " added to cache");
                }
                catch (Exception e)
                {
                    _log.ExceptionLog(e);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            });
            
            return buffer;
            
            
        }
    }
}
