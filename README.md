# sawdust

This woodworking CAD app was a side project of mine back around 2007.
I recently dusted off the code and got it running on .NET Core 3.

And just in case it might be of interest to anybody else, I am
posting it here as open source, Apache License v2.

Once you have the prerequisites (.NET Core 3 Preview 3 on Windows),
you should be able to run the app with:

    cd wpfview
    dotnet run

The `sd` directory contains the core solid modeling code as a
.NET Standard 2.0 library with no dependencies.

The `sd_tests` directory has NUnit tests which can be run with:

    cd sd_tests
    dotnet test

Note that I currently have no plans to continue working on
this app.

See [my blog entry](https://ericsink.com/entries/sawdust_dotnetcore3.html) for more information.

