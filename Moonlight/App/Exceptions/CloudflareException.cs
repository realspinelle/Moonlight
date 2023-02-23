﻿using System.Runtime.Serialization;

[Serializable]
public class CloudflareException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public CloudflareException()
    {
    }

    public CloudflareException(string message) : base(message)
    {
    }

    public CloudflareException(string message, Exception inner) : base(message, inner)
    {
    }

    protected CloudflareException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}