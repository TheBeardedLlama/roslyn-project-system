﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.ProjectSystem.Imaging;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem
{
    [ProjectSystemTrait]
    public class MyProjectFolderProjectTreePropertiesProviderTests
    {
        [Fact]
        public void Constructor_NullAsImageProvider_ThrowsArgumentNull()
        {
            var projectServices = IUnconfiguredProjectCommonServicesFactory.Create();
            var designerService = IProjectDesignerServiceFactory.Create();

            Assert.Throws<ArgumentNullException>("imageProvider", () => {

                new MyProjectFolderProjectTreePropertiesProvider((IProjectImageProvider)null, projectServices, designerService);
            });
        }

        [Fact]
        public void Constructor_NullAsProjectServices_ThrowsArgumentNull()
        {
            var imageProvider = IProjectImageProviderFactory.Create();
            var designerService = IProjectDesignerServiceFactory.Create();

            Assert.Throws<ArgumentNullException>("projectServices", () => {

                new MyProjectFolderProjectTreePropertiesProvider(imageProvider, (IUnconfiguredProjectCommonServices)null, designerService);
            });
        }

        [Fact]
        public void Constructor_NullAsDesignerService_ThrowsArgumentNull()
        {
            var projectServices = IUnconfiguredProjectCommonServicesFactory.Create();
            var imageProvider = IProjectImageProviderFactory.Create();

            Assert.Throws<ArgumentNullException>("designerService", () => {

                new MyProjectFolderProjectTreePropertiesProvider(imageProvider, projectServices, (IProjectDesignerService)null);
            });
        }

        [Fact]
        public void CalculatePropertyValues_NullAsPropertyContext_ThrowsArgumentNull()
        {
            var propertyValues = IProjectTreeCustomizablePropertyValuesFactory.Create();
            var propertiesProvider = CreateInstance();

            Assert.Throws<ArgumentNullException>("propertyContext", () => {
                propertiesProvider.CalculatePropertyValues((IProjectTreeCustomizablePropertyContext)null, propertyValues);
            });
        }

        [Fact]
        public void CalculatePropertyValues_NullAsPropertyValues_ThrowsArgumentNull()
        {
            var propertyContext = IProjectTreeCustomizablePropertyContextFactory.Create();
            var propertiesProvider = CreateInstance();

            Assert.Throws<ArgumentNullException>("propertyValues", () => {
                propertiesProvider.CalculatePropertyValues(propertyContext, (IProjectTreeCustomizablePropertyValues)null);
            });
        }

        [Fact]
        public void ChangePropertyValues_TreeWithMyProjectCandidateButSupportsProjectDesignerFalse_ReturnsUnmodifiedTree()
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => false);   // Don't support AppDesigner
            var propertiesProvider = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
");

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(tree);

            AssertAreEquivalent(tree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder})
")]
        public void ChangePropertyValues_TreeWithPropertiesFolder_ReturnsUnmodifiedTree(string input)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(input);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(tree);

            AssertAreEquivalent(tree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Folder (flags: {Folder})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Folder (flags: {Folder})
        AssemblyInfo.cs (flags: {})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Folder (flags: {Folder})
        AssemblyInfo.cs (flags: {})
    NotMy Project (flags: {Folder})
")]
        public void ChangePropertyValues_TreeWithoutMyProjectCandidate_ReturnsUnmodifiedTree(string input)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => false);
            var propertiesProvider = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(input);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(tree);

            AssertAreEquivalent(tree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {NotFolder})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Unrecognized NotAFolder})
")]
        public void ChangePropertyValues_TreeWithFileCalledMyProject_ReturnsUnmodifiedTree(string input)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(input);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(tree);

            AssertAreEquivalent(tree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder IncludeInProjectCandidate})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {IncludeInProjectCandidate Folder})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {IncludeInProjectCandidate})
")]        
        public void ChangePropertyValues_TreeWithExcludedMyProjectFolder_ReturnsUnmodifiedTree(string input)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(input);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(tree);

            AssertAreEquivalent(tree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Folder (flags: {Folder})
        My Project (flags: {Folder})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Folder (flags: {Folder})
        Folder (flags: {Folder})
            My Project (flags: {Folder})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    Folder1 (flags: {Folder})
    Folder2 (flags: {Folder})
        My Project (flags: {Folder})
")]        
        public void ChangePropertyValues_TreeWithNestedMyProjectFolder_ReturnsUnmodifiedTree(string input)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(input);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(tree);

            AssertAreEquivalent(tree, result);
        }
        
        [Theory]
        [InlineData(@"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
", @"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder})
", @"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder BubbleUp})
", @"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder Unrecognized AppDesignerFolder})
", @"
Root(flags: {ProjectRoot})
    My Project (flags: {Folder Unrecognized AppDesignerFolder BubbleUp})
")]
        public void ChangePropertyValues_TreeWithMyProjectCandidateAlreadyMarkedAsAppDesignerOrBubbleup_AddsRemainingFlags(string input, string expected)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService);

            var inputTree = ProjectTreeParser.Parse(input);
            var expectedTree = ProjectTreeParser.Parse(expected);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder BubbleUp})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    my project (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    my project (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    MY PROJECT (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    MY PROJECT (flags: {Folder AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder UnrecognizedCapability})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder UnrecognizedCapability AppDesignerFolder BubbleUp})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
")]
[InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        My Project (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        My Project (flags: {Folder VisibleOnlyInShowAllFiles})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        AssemblyInfo.cs (flags: {IncludeInProjectCandidate})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        AssemblyInfo.cs (flags: {IncludeInProjectCandidate})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        AssemblyInfo.cs (flags: {IncludeInProjectCandidate})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        AssemblyInfo.cs (flags: {IncludeInProjectCandidate})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Folder (flags: {IncludeInProjectCandidate})
            Item.cs (flags: {IncludeInProjectCandidate})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        Folder (flags: {IncludeInProjectCandidate})
            Item.cs (flags: {IncludeInProjectCandidate})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Folder1 (flags: {IncludeInProjectCandidate})
            Item.cs (flags: {IncludeInProjectCandidate})
        Folder2 (flags: {Folder})
            Item.cs (flags: {})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        Folder1 (flags: {IncludeInProjectCandidate})
            Item.cs (flags: {IncludeInProjectCandidate})
        Folder2 (flags: {Folder VisibleOnlyInShowAllFiles})
            Item.cs (flags: {})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Resources.resx (flags: {})
            Resources.Designer.cs (flags: {})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        Resources.resx (flags: {VisibleOnlyInShowAllFiles})
            Resources.Designer.cs (flags: {})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        AssemblyInfo.cs (flags: {})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        AssemblyInfo.cs (flags: {VisibleOnlyInShowAllFiles})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Folder (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        Folder (flags: {Folder VisibleOnlyInShowAllFiles})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Folder (flags: {Folder})
            Folder (flags: {Folder})
                File (flags: {})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        Folder (flags: {Folder VisibleOnlyInShowAllFiles})
            Folder (flags: {Folder})
                File (flags: {})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Folder1 (flags: {Folder})
            Folder (flags: {Folder})
                File (flags: {})
        Folder2 (flags: {Folder})
            Folder (flags: {Folder})
                File (flags: {})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
        Folder1 (flags: {Folder VisibleOnlyInShowAllFiles})
            Folder (flags: {Folder})
                File (flags: {})
        Folder2 (flags: {Folder VisibleOnlyInShowAllFiles})
            Folder (flags: {Folder})
                File (flags: {})
")]
        [Theory]
        public void ChangePropertyValues_TreeWithMyProjectCandidate_ReturnsCandidateMarkedWithAppDesignerFolderAndBubbleUp(string input, string expected)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService);

            var inputTree = ProjectTreeParser.Parse(input);
            var expectedTree = ProjectTreeParser.Parse(expected);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
        Folder (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp}), Icon: {AE27A6B0-E345-4288-96DF-5EAF394EE369 1}, ExpandedIcon: {AE27A6B0-E345-4288-96DF-5EAF394EE369 1}
        Folder (flags: {Folder VisibleOnlyInShowAllFiles})
")]
        public void ChangePropertyValues_TreeWithPropertiesCandidate_SetsIconAndExpandedIconToAppDesignerFolder(string input, string expected)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var imageProvider = IProjectImageProviderFactory.ImplementGetProjectImage(ProjectImageKey.AppDesignerFolder, new ProjectImageMoniker(new Guid("AE27A6B0-E345-4288-96DF-5EAF394EE369"), 1));
            var propertiesProvider = CreateInstance(imageProvider, designerService);

            var inputTree = ProjectTreeParser.Parse(input);
            var expectedTree = ProjectTreeParser.Parse(expected);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder}), Icon: {AE27A6B0-E345-4288-96DF-5EAF394EE369 1}, ExpandedIcon: {AE27A6B0-E345-4288-96DF-5EAF394EE369 2}
        Folder (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp}), Icon: {AE27A6B0-E345-4288-96DF-5EAF394EE369 1}, ExpandedIcon: {AE27A6B0-E345-4288-96DF-5EAF394EE369 2}
        Folder (flags: {Folder VisibleOnlyInShowAllFiles})
")]
        [InlineData(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder}), Icon: {}, ExpandedIcon: {}
        Folder (flags: {Folder})
", @"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp}), Icon: {}, ExpandedIcon: {}
        Folder (flags: {Folder VisibleOnlyInShowAllFiles})
")]
        public void ChangePropertyValues_TreeWithPropertiesCandidateWhenImageProviderReturnsNull_DoesNotSetIconAndExpandedIcon(string input, string expected)
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var imageProvider = IProjectImageProviderFactory.ImplementGetProjectImage(ProjectImageKey.AppDesignerFolder, null);
            var propertiesProvider = CreateInstance(imageProvider, designerService);

            var inputTree = ProjectTreeParser.Parse(input);
            var expectedTree = ProjectTreeParser.Parse(expected);

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        [Fact]
        public void ChangePropertyValues_ProjectWithNullMyProjectFolder_DefaultsToMyProject()
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService, appDesignerFolder: null);

            var inputTree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
");
            var expectedTree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
");

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        [Fact]
        public void ChangePropertyValues_ProjectWithEmptyMyProjectFolder_DefaultsToMyProject()
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService, appDesignerFolder: "");

            var inputTree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder})
");
            var expectedTree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    My Project (flags: {Folder AppDesignerFolder BubbleUp})
");

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        [Fact]
        public void ChangePropertyValues_ProjectWithNonDefaultMyProjectFolder_ReturnsCandidateMarkedWithAppDesignerFolderAndBubbleUp()
        {
            var designerService = IProjectDesignerServiceFactory.ImplementSupportsProjectDesigner(() => true);
            var propertiesProvider = CreateInstance(designerService, appDesignerFolder: "FooBar");

            var inputTree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    FooBar (flags: {Folder})
");
            var expectedTree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    FooBar (flags: {Folder AppDesignerFolder BubbleUp})
");

            var result = propertiesProvider.ChangePropertyValuesForEntireTree(inputTree);

            AssertAreEquivalent(expectedTree, result);
        }

        private void AssertAreEquivalent(IProjectTree expected, IProjectTree actual)
        {
            Assert.NotSame(expected, actual);

            string expectedAsString = ProjectTreeWriter.WriteToString(expected);
            string actualAsString = ProjectTreeWriter.WriteToString(actual);

            Assert.Equal(expectedAsString, actualAsString);
        }

        private MyProjectFolderProjectTreePropertiesProvider CreateInstance()
        {
            return CreateInstance((IProjectImageProvider)null, (IProjectDesignerService)null);
        }

        private MyProjectFolderProjectTreePropertiesProvider CreateInstance(IProjectDesignerService designerService, string appDesignerFolder = "My Project")
        {
            return CreateInstance((IProjectImageProvider)null, designerService, appDesignerFolder);
        }

        private MyProjectFolderProjectTreePropertiesProvider CreateInstance(IProjectImageProvider imageProvider, IProjectDesignerService designerService, string appDesignerFolder = "My Project")
        {
            designerService = designerService ?? IProjectDesignerServiceFactory.Create();
            var threadingService = IProjectThreadingServiceFactory.Create();
            var project = IUnconfiguredProjectFactory.Create();
            var projectProperties = ProjectPropertiesFactory.Create(project, 
                new PropertyPageData() {
                    Category = nameof(AppDesigner),
                    PropertyName = nameof(AppDesigner.FolderName),
                    Value = appDesignerFolder
                });

            var projectServices = IUnconfiguredProjectCommonServicesFactory.Create(project, threadingService, projectProperties.ConfiguredProject, projectProperties);

            return new MyProjectFolderProjectTreePropertiesProvider(imageProvider ?? IProjectImageProviderFactory.Create(), projectServices, designerService);
        }
    }
}
