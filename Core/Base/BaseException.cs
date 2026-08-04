using BackendJX3D.Core.Store;

namespace BackendJX3D.Core.Base
{
    public class BaseException : Exception
    {
        public class CoreException : Exception
        {
            public CoreException(string code, string message, int statusCode = (int)StatusCodeHelper.ServerError) : base(message)
            {
                Code = code;
                StatusCode = statusCode;
            }

            public string Code { get; }

            public int StatusCode { get; set; }

            public Dictionary<string, object>? AdditionalData { get; set; }
        }

        public class BadRequestException : ErrorException
        {
            public BadRequestException(string errorCode, string message) : base(400, errorCode, message)
            {
            }

            public BadRequestException(ICollection<KeyValuePair<string, ICollection<string>>> errors) : base(400,
                new ErrorDetail
                {
                    ErrorCode = "bad_request",
                    ErrorMessage = errors
                })
            {
            }
        }
        public class ConflictException : ErrorException
        {
            public ConflictException(string errorCode, string message) : base(409, errorCode, message)
            {
            }

            public ConflictException(ICollection<KeyValuePair<string, ICollection<string>>> errors) : base(409,
                new ErrorDetail
                {
                    ErrorCode = "conflict",
                    ErrorMessage = errors
                })
            {
            }
        }
        public class NotFoundException : ErrorException
        {
            public NotFoundException(string errorCode, string message) : base(404, errorCode, message)
            {
            }

            public NotFoundException(ICollection<KeyValuePair<string, ICollection<string>>> errors) : base(404,
                new ErrorDetail
                {
                    ErrorCode = "not_found",
                    ErrorMessage = errors
                })
            {
            }
        }
        public class ForbiddenException : ErrorException
        {
            public ForbiddenException(string errorCode, string message) : base(403, errorCode, message)
            {
            }

            public ForbiddenException(ICollection<KeyValuePair<string, ICollection<string>>> errors) : base(403,
                new ErrorDetail
                {
                    ErrorCode = "access_denied",
                    ErrorMessage = errors
                })
            {
            }
        }

        public class InternalServerErrorException : ErrorException
        {
            public InternalServerErrorException(string errorCode, string message) : base(500, errorCode, message)
            {
            }

            public InternalServerErrorException(ICollection<KeyValuePair<string, ICollection<string>>> errors) : base(500,
                new ErrorDetail
                {
                    ErrorCode = "internal_server_error",
                    ErrorMessage = errors
                })
            {
            }
        }

        public class ErrorException : Exception
        {
            public int StatusCode { get; }

            public ErrorDetail ErrorDetail { get; }

            public ErrorException(int statusCode, string errorCode, string message)
            {
                StatusCode = statusCode;
                ErrorDetail = new ErrorDetail
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                };
            }

            public ErrorException(int statusCode, ErrorDetail errorDetail)
            {
                StatusCode = statusCode;
                ErrorDetail = errorDetail;
            }
        }
        public class ErrorDetail
        {
            public string? ErrorCode { get; set; }

            public object? ErrorMessage { get; set; }
        }
    }
}
