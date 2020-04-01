using System.Net;
using FluentResults;

namespace ThreeChartsAPI.Models.LastFm
{
    public class LastFmResultError : Error
    {
        public int StatusCode { get; }
        public int? LastFmErrorCode { get; }
        public string? LastFmErrorMessage { get; }

        public LastFmResultError(int statusCode, LastFmError? error)
        {
            StatusCode = statusCode;
            LastFmErrorCode = error?.ErrorCode;
            LastFmErrorMessage = error?.Message;
        }
    }
}