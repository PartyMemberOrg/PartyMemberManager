﻿using PdfSharp.Fonts;
using System;
using System.IO;

namespace AspNetCorePdf.PdfProvider
{
    //This implementation is obviously not very good --> Though it should be enough for everyone to implement their own.
    public class FontResolver : IFontResolver
    {
        private string _resourcesPath = string.Empty;
        public FontResolver(string resourcesPath)
        {
            _resourcesPath = resourcesPath;
        }

        
        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            using (var ms = new MemoryStream())
            {
                using (var fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("OpenSans", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}Tinos-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}Tinos-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}Tinos-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}Tinos-Regular.ttf");
                }
            }
            else if(familyName.Equals("宋体"))
            {
                return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}STSONG.TTF");
            }
            else if (familyName.Equals("黑体"))
            {
                return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}SIMHEI.TTF");
            }
            else if (familyName.Equals("楷体"))
            {
                return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}楷体_GB2312.TTF");
            }
            else if (familyName.Equals("隶书"))
            {
                return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}SIMLI.TTF");
            }
            else
            {
                //如果未设置，则默认用宋体
                return new FontResolverInfo($"{_resourcesPath}{Path.DirectorySeparatorChar}STSONG.TTF");
            }
            return null;
        }
    }
}