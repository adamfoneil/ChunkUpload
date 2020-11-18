using ChunkUpload.Helpers;
using ChunkUpload.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ChunkUpload.Services
{
    public class UploadService : IUploadService
    {
        private readonly FormOptions _formOptions;
        private readonly IFileStorage _storageService;

        public UploadService(IFileStorage storageService, IOptions<FormOptions> formOptions)
        {
            _formOptions = formOptions.Value;
            _storageService = storageService;
        }

        public async Task<bool> UploadAsync(HttpRequest request, ModelStateDictionary modelState)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                modelState.AddModelError("File", "The request couldn't be processed (Error 1).");
                return false;
            }

            var formAccumulator = new KeyValueAccumulator();

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType),
                _formOptions.MultipartBoundaryLengthLimit);

            var reader = new MultipartReader(boundary, request.Body);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var fileName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
                        await _storageService.UploadAsync(fileName, section.Body);

                        if (!modelState.IsValid)
                        {
                            return false;
                        }
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                        var encoding = MultipartRequestHelper.GetEncoding(section);

                        if (encoding == null)
                        {
                            modelState.AddModelError("File", "The request couldn't be processed (Error 2).");
                            return false;
                        }

                        using var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true);

                        var value = await streamReader.ReadToEndAsync();

                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        formAccumulator.Append(key, value);

                        if (formAccumulator.ValueCount > _formOptions.ValueCountLimit)
                        {
                            modelState.AddModelError("File", "The request couldn't be processed (Error 3).");
                            return false;
                        }
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            /*
            var formData = new FormData();

            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()),
                CultureInfo.CurrentCulture);

            var bindingSuccessful = await TryUpdateModelAsync(formData, "", formValueProvider);
            if (!bindingSuccessful)
            {
                ModelState.AddModelError("File", "The request couldn't be processed (Error 5).");
                return BadRequest(ModelState);
            }
            */

            return true;
        }
    }
}
