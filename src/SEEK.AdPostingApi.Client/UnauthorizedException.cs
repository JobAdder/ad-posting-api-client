﻿using System;
using System.Net;
using System.Runtime.Serialization;
using SEEK.AdPostingApi.Client.Models;

namespace SEEK.AdPostingApi.Client
{
    [Serializable]
    public class UnauthorizedException : RequestException
    {
        public UnauthorizedException(string requestId, string message) : base(requestId, (int)HttpStatusCode.Unauthorized, message)
        {
        }

        public UnauthorizedException(string requestId, AdvertisementErrorResponse errorResponse) : base(requestId, (int)HttpStatusCode.Unauthorized, errorResponse?.Message)
        {
            this.Errors = errorResponse?.Errors;
        }

        protected UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Errors = (AdvertisementError[])info.GetValue(nameof(this.Errors), typeof(AdvertisementError[]));
        }

        public AdvertisementError[] Errors { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Errors), this.Errors);

            base.GetObjectData(info, context);
        }
    }
}