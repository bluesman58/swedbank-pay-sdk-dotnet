﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Moq;

namespace SwedbankPay.Sdk.Tests.TestHelpers
{
    public class MockFileProvider : IFileProvider
    {
        private readonly IEnumerable<IFileInfo> files;
        private readonly Dictionary<string, IChangeToken> changeTokens;

        public MockFileProvider()
        { }

        public MockFileProvider(params IFileInfo[] files)
        {
            this.files = files;
        }

        public MockFileProvider(params KeyValuePair<string, IChangeToken>[] changeTokens)
        {
            this.changeTokens = changeTokens.ToDictionary(
                changeToken => changeToken.Key,
                changeToken => changeToken.Value,
                StringComparer.Ordinal);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var contents = new Mock<IDirectoryContents>();
            contents.Setup(m => m.Exists).Returns(true);

            if (string.IsNullOrEmpty(subpath))
            {
                contents.Setup(m => m.GetEnumerator()).Returns(this.files.GetEnumerator());
                return contents.Object;
            }

            var filesInFolder = this.files.Where(f => f.Name.StartsWith(subpath, StringComparison.Ordinal));
            if (filesInFolder.Any())
            {
                contents.Setup(m => m.GetEnumerator()).Returns(filesInFolder.GetEnumerator());
                return contents.Object;
            }
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var file = this.files.FirstOrDefault(f => f.Name == subpath);
            return file ?? new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            if (this.changeTokens != null && this.changeTokens.ContainsKey(filter))
            {
                return this.changeTokens[filter];
            }
            return NullChangeToken.Singleton;
        }
    }
}