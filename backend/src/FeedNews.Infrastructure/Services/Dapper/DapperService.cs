using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Dapper;
using FeedNews.Application.Common.Services.Dapper;

namespace FeedNews.Infrastructure.Services.Dapper;

public class DapperService : IDapperService
{
    private readonly Assembly _assembly;
    private readonly IDbConnection _db;

    public DapperService(IDbConnection db)
    {
        _db = db;
        _assembly = GetType().Assembly;
    }

    public Task<T> ExecuteScalarAsync<T>(QueryName queryName, object queryParams)
    {
        throw new NotImplementedException();
    }


    public async Task<dynamic> SelectAsync(string query)
    {
        Debug.WriteLine("DapperSerice: " + query);
        bool isClosed = _db.State == ConnectionState.Closed;
        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            return await _db.QueryAsync(query).ConfigureAwait(false);
        }
        finally
        {
            await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<T>> SelectAsync<T>(QueryName queryName, object queryParams)
    {
        string query = await GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            IEnumerable<T> result = await _db.QueryAsync<T>(query, queryParams).ConfigureAwait(false);
            return result;
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TReturn>(QueryName queryName,
        Func<TFirst, TSecond, TReturn> map, object queryParams, string splitOn)
    {
        string query = await GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            return await _db.QueryAsync(query, map, queryParams, splitOn: splitOn)
                .ConfigureAwait(false);
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TReturn>(QueryName queryName,
        Func<TFirst, TSecond, TThird, TReturn> map, object queryParams, string splitOn)
    {
        string query = await GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            return await _db.QueryAsync(query, map, queryParams, splitOn: splitOn)
                .ConfigureAwait(false);
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(QueryName queryName,
        Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object queryParams, string splitOn)
    {
        string query = await GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            return await _db
                .QueryAsync(query, map, queryParams, splitOn: splitOn)
                .ConfigureAwait(false);
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
        QueryName queryName, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object queryParams,
        string splitOn)
    {
        string query = await GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            return await _db
                .QueryAsync(query, map, queryParams,
                    splitOn: splitOn).ConfigureAwait(false);
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<T> SingleOrDefaultAsync<T>(QueryName queryName, object queryParams)
    {
        string query = await GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) await ((DbConnection)_db).OpenAsync().ConfigureAwait(false);

            List<T> tmpResult = (await _db.QueryAsync<T>(query, queryParams).ConfigureAwait(false)).ToList();
            if (tmpResult.Count() > 1)
                throw new InvalidOperationException($"Query {queryName} have more than 1 record");
            else
                return tmpResult.First();
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)_db).CloseAsync().ConfigureAwait(false);
        }
    }

    public T SingleOrDefault<T>(QueryName queryName, object queryParams)
    {
        string query = GetQueryString(queryName);
        bool isClosed = _db.State == ConnectionState.Closed;

        try
        {
            if (isClosed) _db.Open();
            List<T> tmpResult = _db.Query<T>(query, queryParams).ToList();
            if (tmpResult.Count > 1) throw new InvalidOperationException($"Query {queryName} has more than 1 record");
            T result = tmpResult.First();

            return result;
        }
        finally
        {
            if (isClosed)
                _db.Close();
        }
    }

    private string GetQueryString(QueryName queryName)
    {
        string result;

        string sqlFile = queryName + ".sql";
        Stream? stream = _assembly.GetManifestResourceStream(_assembly.GetName().Name + ".Common.Queries." + sqlFile);
        if (stream == null) throw new ArgumentException("Not exist file sql with name ", sqlFile);
        StreamReader reader = new(stream);
        result = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(result)) throw new ArgumentException("Not exist file sql with name ", sqlFile);

        Debug.WriteLine("Dapper: " + sqlFile);
        return result;
    }

    private async Task<string> GetQueryStringAsync(QueryName queryName)
    {
        string result;

        string sqlFile = queryName + ".sql";
        Stream? stream = _assembly.GetManifestResourceStream(_assembly.GetName().Name + ".Common.Queries." + sqlFile);
        if (stream == null) throw new ArgumentException("Not exist file sql with name ", sqlFile);
        StreamReader reader = new(stream);
        result = await reader.ReadToEndAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(result)) throw new ArgumentException("Not exist file sql with name ", sqlFile);

        Debug.WriteLine("Dapper: " + sqlFile);
        return result;
    }
}