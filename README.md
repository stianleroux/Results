# Results

[![Nuget downloads](https://img.shields.io/nuget/v/slr.results.svg)](https://www.nuget.org/packages/SLR.Results/) [![Nuget](https://img.shields.io/nuget/dt/slr.results)](https://www.nuget.org/packages/SLR.Results/) [![Build](https://github.com/stianleroux/Results/actions/workflows/dotnet.yml/badge.svg)](https://github.com/stianleroux/Results/actions/workflows/dotnet.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/stianleroux/Results/blob/master/LICENSE)

**SLR.Results is a .NET library that is designed to tackle a common issue. It presents an object that reflects the success or failure of an operation, as opposed to utilizing or raising exceptions.**

You can install [SLR.Results with NuGet](https://www.nuget.org/packages/SLR.Results/):

```
Install-Package SLR.Results
```

## Key Features

-   Works in most .NET Projects
-   Choose from Result, Result<T>, ListResult<T> to cater for all use cases
-   Store **multiple errors or validation errors** in one Result object
-   Store **Error or Success objects** instead of only error messages in string format
-   Allows uniformity in your code or anyone that needs to consume it
-   ApiResponseHelper to have uniform API Responses in Controllers
-   Result<List<T>> has built-in capability for paging using PagingExtensions
-   Monadic composition (Bind): Chain result-producing operations.
-   Mapping (Map, MapError): Transform values or errors.
-   Pattern matching (Match): Handle success/failure paths cleanly.

## Return a Result

A Result can store multiple Errors, NotFound or ValidationErrors.

```csharp
// return a result which indicates success
return Result.Success();

// return a result of a type which indicates success
return Result<Sample>.Success(new Sample());

//return a list result which indicates success
var entities = this.DatabaseContext.Samples.Select(x => x).AsNoTracking()
var count = await entities.Count();
var paged = await entities.ApplyPaging(model.PagingArgs).ToListAsync(cancellationToken);

return Result<List<Sample>>.Success(paged, count);

// return a result which indicates failure
return Result.Failure("Error");
return Result<List<Sample>>.Failure("Error");
```

## Result on the API

This will handle 400, 404, 500 and 200

```csharp
  return ApiResponseHelper.ResponseOutcome(await this.Mediator.Send(new ExampleQuery(), cancellationToken), this)
```

## Monadic composition Helper Function

FunctionalResult class (like Map, Bind, MapError, and Match) are a great addition and fit perfectly with the Result<T> pattern you're already using.

```cs
Result<UserDto> result = GetUser(userId)
    .Bind(user => GetUser(user.Id))
    .Map(user => new UserDto(user.Name))
    .Match(
        onSuccess: dto => Ok(dto),
        onFailure: err => BadRequest(err)
    )
```

## .NET Targeting

Latest .NET

## Contributors

Thanks to all the contributors and to all the people who gave feedback!

<a href="https://github.com/stianleroux/results/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=stianleroux/results" />
</a>

## Copyright

Copyright (c) Stian Le Roux. See LICENSE for details.
