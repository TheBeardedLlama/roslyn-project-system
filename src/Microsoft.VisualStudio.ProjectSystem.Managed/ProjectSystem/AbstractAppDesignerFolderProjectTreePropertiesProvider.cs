﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.Imaging;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace Microsoft.VisualStudio.ProjectSystem
{
    /// <summary>
    ///     Provides the base class for tree modifiers that handle the AppDesigner folder, called "Properties" in C# and "My Project" in Visual Basic.
    /// </summary>
    internal abstract class AbstractAppDesignerFolderProjectTreePropertiesProvider : AbstractSpecialFolderProjectTreePropertiesProvider
    {
        private static readonly ProjectTreeFlags DefaultFolderFlags = ProjectTreeFlags.Create(ProjectTreeFlags.Common.AppDesignerFolder | ProjectTreeFlags.Common.BubbleUp);

        private readonly IUnconfiguredProjectCommonServices _projectServices;
        private readonly IProjectDesignerService _designerService;

        protected AbstractAppDesignerFolderProjectTreePropertiesProvider(IProjectImageProvider imageProvider, IUnconfiguredProjectCommonServices projectServices, IProjectDesignerService designerService)
            : base(imageProvider)
        {
            Requires.NotNull(projectServices, nameof(projectServices));
            Requires.NotNull(designerService, nameof(designerService));

            _projectServices = projectServices;
            _designerService = designerService;
        }

        public override bool IsSupported
        {
            get { return _designerService.SupportsProjectDesigner; }
        }

        public override ProjectTreeFlags FolderFlags
        {
            get { return DefaultFolderFlags; }
        }

        public override string FolderImageKey
        {
            get {  return ProjectImageKey.AppDesignerFolder; }
        }

        protected override sealed bool IsCandidateSpecialFolder(IProjectTreeCustomizablePropertyContext propertyContext, ProjectTreeFlags flags)
        {
            if (propertyContext.ParentNodeFlags.IsProjectRoot() && flags.IsFolder() && flags.IsIncludedInProject())
            {
                string folderName = GetAppDesignerFolderName();

                return StringComparers.Paths.Equals(folderName, propertyContext.ItemName);
            }

            return false;
        }

        protected virtual string GetAppDesignerFolderName()
        {
            // Returns the <AppDesignerFolder> from the project file
            return _projectServices.ThreadingService.ExecuteSynchronously(async () => {

                var properties = await _projectServices.ActiveConfiguredProjectProperties.GetAppDesignerPropertiesAsync()
                                                                                         .ConfigureAwait(false);

                return (string)await properties.FolderName.GetValueAsync()
                                                          .ConfigureAwait(false);
            });
        }
    }
}
