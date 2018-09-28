[![Build status](https://ci.appveyor.com/api/projects/status/d76ll7iouu121c4t?svg=true)](https://ci.appveyor.com/project/jayoungers/patchmap)
[![NuGet](http://img.shields.io/nuget/v/PatchMap.svg)](https://www.nuget.org/packages/PatchMap/)

# PatchMap
A framework for .Net to handle updating entities from a view model at the field level: Inserts, Updates, and individual field updates can all be handled with one implementation.

```
POST /api/Users
{
    Name: 'Example User',
    IsActive: true
}

PUT /api/Users/1
{
    Name: 'New Name',
    IsActive: false
}

PATCH /api/Users/1
[
    { op: 'replace', path: 'Name', value: 'Patched Name' }
]
```

For more details, visit the [Patchmap Wiki](https://github.com/jayoungers/PatchMap/wiki).