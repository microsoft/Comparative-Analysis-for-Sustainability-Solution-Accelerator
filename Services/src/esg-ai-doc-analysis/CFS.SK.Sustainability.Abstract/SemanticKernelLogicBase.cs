// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CFS.SK.Abstracts.Model;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using System.Collections.Specialized;

namespace CFS.SK.Abstracts
{
    public class SemanticKernelLogicBase<T>
    {
        protected ApplicationContext _appContext;
        protected ILogger<T> _logger;

        public SemanticKernelLogicBase(ApplicationContext appContext, ILogger<T> logger)
        {
            _appContext = appContext;
            _logger = logger;
        }

        public virtual Task Execute() { throw new NotImplementedException("Not Implemented"); }
        public virtual Task Execute(IParameter serviceRequest) { throw new NotImplementedException("Not Implemented"); }
        public virtual Task<IReturnValue?> ExecuteAndReturnResult(Dictionary<string, object>? param = null) { throw new NotImplementedException("Not Implemented"); }
        public virtual Task<IReturnValue?> ExecuteAndReturnResult(IParameter serviceRequest) { throw new NotImplementedException("Not Implemented"); }
        public virtual Task<string?> ExecuteAndReturnString(IParameter serviceRequest) { throw new NotImplementedException("Not Implemented"); }
        public virtual Task<string?> ExecuteAndReturnString(Dictionary<string, object>? param = null) { throw new NotImplementedException("Not Implemented"); }
        public virtual Task<MemoryAnswer?> ExecuteAndReturnResultAsMemoryAnswer(Dictionary<string, object>? param = null) { throw new NotImplementedException("Not Implemented"); }
        public virtual Task<MemoryAnswer?> ExecuteAndReturnResultAsMemoryAnswer(IParameter serviceRequest) { throw new NotImplementedException("Not Implemented"); }

        protected void Write(object? target = null)
        {
            Console.Out.Write(target ?? string.Empty);
        }

        protected void WriteLine(object? target = null)
        {
            Console.Out.WriteLine(target ?? string.Empty);
        }
    }
}
