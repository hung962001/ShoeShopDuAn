using StackExchange.Redis;
using System;

namespace ShoeShopDuAn
{
    public static class RedisCacheManager
    {
        private static IConnectionMultiplexer _connection;

        public static void InitializeConnection(IConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        public static IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }
    }

}