﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.Win32;

namespace Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT
{
    internal sealed class UriToLocalStreamResolver : IUriToStreamResolver
    {
        private static readonly Lazy<string> BasePath = new Lazy<string>(() =>
#pragma warning disable SA1129 // Do not use default value type constructor
            Path.GetDirectoryName(UnsafeNativeMethods.GetModuleFileName(new HandleRef())));
#pragma warning restore SA1129 // Do not use default value type constructor

        Stream IUriToStreamResolver.UriToStream(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            // The incoming URI will be in the form of
            // ms-local-stream://microsoft.win32webviewhost_xxxxxxxx_yyyyyyyyyyyyyy/content.htm
            // We are only interested in the items after the application identity (x) and guid (y), e.g. "/content.htm"
            // Since we must not read content from web view host
            var path = uri.AbsolutePath;

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(DesignerUI.E_WEBVIEW_INVALID_URI, nameof(uri));
            }

            // Clean up path
            path = path.TrimStart(PathUtilities.AltDirectorySeparatorChar);

            if (string.IsNullOrEmpty(path))
            {
                // After stripping away the separator chars, we have nothing left
                // e.g. uri was "ms-local-stream://microsoft.win32webviewhost_xxxxxxxx_yyyyyyyyyyyyyy/"
                throw new ArgumentException(DesignerUI.E_WEBVIEW_INVALID_URI, nameof(uri));
            }

            // This should never be the case, but guard just in case
            if (PathUtilities.IsAbsolute(path))
            {
                throw new ArgumentException(DesignerUI.E_WEBVIEW_INVALID_URI, nameof(uri));
            }

            // Translate forward slash into backslash for use in file paths
            path = path.Replace(PathUtilities.AltDirectorySeparatorChar, PathUtilities.DirectorySeparatorChar);

            var fullPath = Path.Combine(BasePath.Value, path);

            // TODO: Make this proper async all the way through
            return File.Open(fullPath, FileMode.Open, FileAccess.Read);
        }
    }
}