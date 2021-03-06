[![Nuget](https://img.shields.io/nuget/v/AO.AzureUploader)](https://www.nuget.org/packages/AO.AzureUploader)

This came from a need to offer robust, standardized file uploads in Azure blob storage, with an emphasis on large file support. It's pretty easy to implement blob storage uploads on small files. In my experience, though, things get weird when you need to support large files -- meaning files large enough to timeout on a single request. I use and love [DropzoneJS](https://www.dropzonejs.com/) because, among other things, it supports chunking out of the box. But as a JS/frontend library, it provides no guidance how to implement upload chunking on the backend. This is just the nature of frontend libraries, not a shortcoming. So, in this repo, I worked through the problem of implementing chunked uploads to blob storage. And while I love Azure and blob storage, I think it has a discoverability problem when it comes to advanced functionality. So, if this functionality is obvious to everyone, congratulations! It was not obvious to me.

Here's my approach:

- The heart of my solution is an interface [IBlockUploader](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Interfaces/IBlockUploader.cs). Chunked uploads to blob storage revolve around `staging` and `committing` blob "blocks." Chunks are `staged` over many successive POSTs and then `committed` at the end. These interface methods will ultimately be executed by Azure's [StageBlockAsync](https://docs.microsoft.com/en-us/dotnet/api/azure.storage.blobs.specialized.blockblobclient.stageblockasync?view=azure-dotnet) and [CommitBlockListAsync](https://docs.microsoft.com/en-us/dotnet/api/azure.storage.blobs.specialized.blockblobclient.commitblocklistasync?view=azure-dotnet) `BlockBlobClient` methods.

- Create a service class [BlockBlobUploader](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Services/BlockBlobUploader.cs) that implements `IBlockUploader`. This is what would be added to the service collection [during Startup](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Startup.cs#L59-L63). When you upload blob chunks or "blocks", you have to track the blockIds you've created over all of your `staging` calls. For that purpose, I made an abstract class [BlockTracker](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Abstract/BlockTracker.cs). It's abstract because I could see different ways to track those blockIds -- in a database or in a local file. For this example, I opted for a simple `App_Data` local file approach in [AppDataBlockTracker](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Services/AppDataBlockTracker.cs). Note that `AppDataBlockTracker` didn't work for multiple concurrent uploads, so I created a database solution [DbBlockTracker](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Services/DbBlockTracker.cs) that does. This is not part of the package because I didn't want to add a database dependency, but my implementation here will give you some ideas on how to implement in your app.

- In my application performing the uploads, I have a [FilesController](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Controllers/FilesController.cs). You can see it has a `BlockBlobUploader` dependency along with two actions [Stage](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Controllers/FilesController.cs#L17) and [Commit](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Controllers/FilesController.cs#L24).

- My upload Razor page is [here](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Pages/Index.cshtml), and the important parts are [here](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Pages/Index.cshtml#L16-L28). This page has a lot of junk in it from prior iterations. The part I've highlighted is the most relevant thing to see here. There are two forms and an upload button. One form is `staging` -- the hidden form is for `committing`.

- My Dropzone JS implementation is [here](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/wwwroot/js/dz-upload.js).

## Video Walkthrough
Here's a [walkthrough](https://1drv.ms/v/s!AvguHRnyJtWMmbIk64wgwuoWM-Jzww?e=EtmKQc) on this solution in PowerPoint.

A few things have changed since I made this video, but it's mostly pretty good for understanding what this does. The main change now is that in order to support multiple concurrent uploads, my `Files/Commit` handler doesn't redirect anymore. A redirect durring the `Commit` step would interrupt other downloads in progress. Therefore, I use an ajax [fetch](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/wwwroot/js/dz-upload.js#L25-L29) during `Commit` so that the Upload page is allowed to keep running.

## Credits
This [SO answer](https://stackoverflow.com/a/61484128/2023653) gave me the idea for what became my `BlockBlobUploader` class. The answer here used a local file example. I knew this would have to be heavily reworked to be stateless for web use. This is what led to my `BlockTracker` abstract class.

[Vadim17](https://github.com/vadim17) contributed a [streaming upload](https://github.com/adamfoneil/ChunkUpload/blob/master/ChunkUpload/Services/UploadService.cs) example in this repo. Vadim does amazing work. See his [Upwork profile](https://www.upwork.com/freelancers/~01a778def0bc56bf99). But I ended up not using his implementation because it didn't integrate with DropzoneJS. I like Dropzone's user experience with the animations, drag and drop, etc.

## AppDataTempFileProvider
Another service class offered in this package is [AppDataTempFileProvider](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Services/AppDataTempFileProvider.cs) which implements [ITempFileProvider](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Interfaces/ITempFileProvider.cs). This came up because I needed a way to upload to blob storage from a byte array without using `MemoryStream`, which is problematic for large files. The use is in a closed-source project, so I don't have an example or test built up I can show directly. But to use this, inject `ITempFileProvider` into your controller or page, and use it instead of a `MemoryStream`. This will save the byte array as a temp file in the `App_Data` folder. Then you can [perform any operation](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Services/AppDataTempFileProvider.cs#L48) on it (typically uploading to blob storage), and be assured that the [temp file will be deleted](https://github.com/adamfoneil/ChunkUpload/blob/master/AzureUploader/Services/AppDataTempFileProvider.cs#L54) when the operation completes.

So instead of this:

```csharp
using (var stream = new MemoryStream(byteArray))
{
    await blobClient.UploadAsync(stream);
}
```

Do this:
```csharp
// injected from ctor
private readonly ITempFileProvider _tempFile;

// then for your upload
await _tempFile.ExecuteAsync(byteArray, async (stream) => await blobClient.UploadAsync(stream));
```
