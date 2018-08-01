﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DependencyScanner.Core.Nuspec
{
    public static class NuspecUpdater
    {
        internal static XDocument AddDependency(XDocument document, params string[] ids)
        {
            var dep = document
                    .Element("package")
                    .Element("metadata")
                    .Element("dependencies");

            foreach (var id in ids)
            {
                dep.Add(new XElement("dependency", new XAttribute("id", id)));
            }

            return document;
        }
        internal static XDocument RemoveDependency(XDocument document, params string[] ids)
        {
            var dep = document
                    .Element("package")
                    .Element("metadata")
                    .Element("dependencies")
                    .Elements("dependency")
                    .Where(a => ids.Any(b => b == a.Attribute("id").Value));

            dep.Remove();

            return document;
        }

        internal static IEnumerable<string> GetDependencies(XDocument document)
        {
            var docu = document
                        .Element("package")
                        .Element("metadata");

            if (docu.Descendants("dependencies").Any())
            {
                return docu.Element("dependencies")
                    .Elements("dependency")
                    .Select(a => a.Attribute("id").Value);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
