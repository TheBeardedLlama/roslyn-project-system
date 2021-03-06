### Code

#### Visual Studio
From within [Visual Studio "15" Preview 2](https://www.visualstudio.com/en-us/downloads/visual-studio-next-downloads-vs), simply open _src\ProjectSystem.sln_.

Inside Visual Studio, you can build, deploy and run tests.

#### Command-line

From within a [Visual Studio Developer Prompt](https://msdn.microsoft.com/en-us/library/ms229859(v=vs.150).aspx), from the repo root, run:

```
build.cmd
```

This builds, deploys and run tests.

### Debugging/Deploying


#### Visual Studio
By default when you build, the project system and other binaries gets deployed to the _RoslynDev_ experimental instance of Visual Studio. They are setup so that they _override_ any binaries that come with Visual Studio.

To start debugging:

1. Open __src\ProjectSystem.sln__
2. Right-click on the __ProjectSystemSetup__ project, and choose __Set As Startup Project__
3. Press _F5_

If this is your first launch of the project system, or _RoslynDev_ experimental instance, press _CTRL+F5_ to pre-prime and avoid a _long_ start up time.

#### Command-line

From the command-line, after you've run `build.cmd`, you can launch a Visual Studio instance with your recently built bits by running:

```
devenv /rootsuffix RoslynDev
```

### Testing 

#### Project System
While the long term goal is to have all C#/VB projects use this project system, currently none do. This means that you need to manually create a project to test this:

1. __File__ -> __New__ -> __Project__ -> __C#__ -> __Templates__ -> __Visual C#__ -> __Windows__ -> __Console Application__
2. Right-click on the project and choose __Open in File Explorer__
3. __File__ -> __Close Solution__
4. In __File Explorer__, rename project from _[project].csproj_ -> _[project].msbuildproj_
5. __File__ -> __Open__ -> __Project/Solution__ and browse to the project you just renamed and choose __Open__

#### AppDesigner, Settings, Resource Editors and Property Pages
Unless you you need to test these features within the context of the new project system, the easiest way to test these features are via existing C# and Visual Basic projects.

### Code Coverage

#### Visual Studio

You can collect code coverage within Visual Studio, to do so, do the following:

1. __Test__ -> __Test Settings__ -> __Select Test Settings File__
2. In __Open Settings Files__, browse to and select _src\ProjectSystem.runsettings_. This will exclude files from the coverage run that are not part of the product.
3. Choose __Test__ -> __Analyze Code Coverage__ -> __All Tests__

NOTE: In Visual Studio "15", there is currently a known issue with code coverage turned on that results in tests failing with:

```
'---- System.IO.FileNotFoundException : Could not load file or assembly 'file:///C:\roslyn-project-system\Binaries\Debug\Tests\Microsoft.VisualStudio.CodeCoverage.Shim.dll' or one of its dependencies. The system cannot find the file specified.'
```

This will be fixed in a future release. The workaround is to copy C:\Program Files (x86)\Microsoft Visual Studio 15.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.CodeCoverage.Shim.dll into the tests output directory (bin\Tests).
