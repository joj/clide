﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.ExtensibilityHosting;
using System.ComponentModel.Composition.Primitives;
using Clide;
using Microsoft.VisualStudio.Text.Editor;
using IntegrationPackage;
using System.Dynamic;

namespace Clide
{
    [TestClass]
    public class HostingSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingShellPackage_ThenSucceeds()
        {
            var components = ServiceProvider.GetService<SComponentModel, IComponentModel>();
            var package = components.GetService<IShellPackage>();

            Assert.NotNull(package);

            package = ServiceProvider.GetExportedValue<IShellPackage>();

            Assert.NotNull(package);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenExportingVsAdornmentFactory_ThenCanRetrieveIt()
        {
            var components = ServiceProvider.GetService<SComponentModel, IComponentModel>();
            var factory = components.DefaultExportProvider.GetExports<IWpfTextViewCreationListener, IDictionary<string, object>>()
                .Where(e => e.Metadata.ContainsKey("IsClide"))
                .FirstOrDefault();

            var exports = default(ICollection<Lazy<IWpfTextViewCreationListener, IDictionary<string, object>>>);
            VsExportProviderService.TryGetExports<IWpfTextViewCreationListener, IDictionary<string, object>>(out exports);

            Assert.NotNull(factory);
        }
    }
}