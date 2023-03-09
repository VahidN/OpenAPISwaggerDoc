using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace OpenAPISwaggerDoc.Web.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
{
    private readonly MediaTypeCollection _mediaTypes = new();
    private readonly string _requestHeaderToMatch;

    public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
                                                  string mediaType,
                                                  params string[] otherMediaTypes)
    {
        _requestHeaderToMatch = requestHeaderToMatch
                                ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

        if (mediaType == null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        // check if the inputted media types are valid media types
        // and add them to the _mediaTypes collection                     

        if (MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
        {
            _mediaTypes.Add(parsedMediaType.MediaType);
        }
        else
        {
            throw new ArgumentException("Invalid mediaType", nameof(mediaType));
        }

        if (otherMediaTypes is null)
        {
            return;
        }

        foreach (var otherMediaType in otherMediaTypes)
        {
            if (MediaTypeHeaderValue.TryParse(otherMediaType, out var parsedOtherMediaType))
            {
                _mediaTypes.Add(parsedOtherMediaType.MediaType);
            }
            else
            {
                throw new ArgumentException("Invalid otherMediaTypes", nameof(otherMediaTypes));
            }
        }
    }

    public string RequestHeaderToMatch { get; }
    public string MediaType { get; }
    public string[] OtherMediaTypes { get; }

    public int Order => 0;

    public bool Accept(ActionConstraintContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
        if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
        {
            return false;
        }

        var parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);

        // if one of the media types matches, return true
        foreach (var mediaType in _mediaTypes)
        {
            var parsedMediaType = new MediaType(mediaType);
            if (parsedRequestMediaType.Equals(parsedMediaType))
            {
                return true;
            }
        }

        return false;
    }
}