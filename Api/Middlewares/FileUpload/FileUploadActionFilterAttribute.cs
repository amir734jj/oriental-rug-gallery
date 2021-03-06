﻿using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Api.Attributes;
using Dal.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Models.Constants;
using StreamRipper.Extensions;

namespace Api.Middlewares.FileUpload
{
    /// <summary>
    ///     File upload action filter attribute that validates the MIME-Type of uploaded file
    /// </summary>
    public class FileUploadActionFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        ///     On Action executing
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="T:System.Exception"></exception>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var methodInfo = ((ControllerActionDescriptor) context.ActionDescriptor)?.MethodInfo;

            // Get FileUploadAttribute
            var attribute = methodInfo?.GetCustomAttribute<FileUploadAttribute>();

            // If attribute if not null and it's accept parameter is not empty
            if (attribute != null)
            {
                var mimeTypeTable = methodInfo.GetParameters()
                    .Select(x => (x.Name, FileMimeTypeAttribute: x.GetCustomAttribute<FileMimeTypeAttribute>()))
                    .Where(x => x.Name != null && x.FileMimeTypeAttribute != null)
                    .ToDictionary(x => x.Name, x => x.FileMimeTypeAttribute?.MimeType);

                context.ActionArguments.Where(x => x.Value is IFormFile)
                    .Select(x => (x.Key, Value: (IFormFile) x.Value))
                    .ForEach(tuple =>
                {
                    var (key, formFile) = tuple;

                    var regex = new Regex(mimeTypeTable[key]);

                    var result = MimeTypeTable.DefaultMapping.Where(x => regex.IsMatch(x.Value))
                        .Select(x => (Extension: x.Key, MimeType: x.Value))
                        .Any(x => formFile.ContentType == x.MimeType && formFile.FileName.EndsWith(x.Extension));

                    if (!result)
                    {
                        throw new Exception("Invalid File MIME-Type, " +
                                            $"Expected: [{string.Join(',', attribute.Accepts.Select(str => $"`{str}`"))}], " +
                                            $"But received: `{formFile.ContentType}`");
                    }
                });
            }

            // Call base, continue executing the task
            base.OnActionExecuting(context);
        }
    }
}