# TheHandyUnity

This is a wrapper for version 1.0.0 of the web API for The Handy male masturbator. You can learn more about the Handy at [www.thehandy.com](www.thehandy.com)

The focus is on enabling all the functionality through async functions with callbacks in C#. I haven't included anything like synchronizing to animations, following GameObjects, etc. because it's hard to know what people want out of the SDK, and because a lot of that stuff would be super labor intensive for (I imagine) very few users. If you find yourself needing something like that, feel free to submit a pull request!

### Included Functionality

**Machine Commands**
  * Set Mode
  * Toggle Mode
  * Set Speed (Percent and mm/s)
  * Set Stroke Length (Percent and mm)
  * Step Speed
  * Step Stroke
 
**Get Data**
  * Get Version
  * Get Settings
  * Get Mode

**Sync**
  * Get Server Time
  * Sync Prepare (download file to Handy)
  * Sync Play / Pause
  * Set Sync Offset
  * Convert custom pattern to CSV URL
  * Upload Funscript
  * Upload CSV
 
### Basic Operation

The only class you need to know about is `HandyConnection`, which is a static class containing a few public properties, events and a bunch of functions.

All of the functions are asynchronous (since the SDK needs to talk to the Handy API over the internet). Each has an `onSuccess` callback that contains any returned data, and an `onError` callback which contains an error message as a string if anything goes wrong.

Generally speaking, the SDK can be used like this:

```C#
HandyConnection.ConnectionKey = "YOUR KEY HERE";
HandyConnection.SomeFunction(HandleSuccess, HandleError);
```

The class `HandyExampleUsageUI` contains examples on how to do anything you might want to do with the API, including creating generative stroke patterns and uploading them to a Handy, loading a .funscript file to the Handy, and performing all of the direct machine control functions.

### Planned Functionality

There's an undocumented position mode that I'd like to add support for, one of the Handy engineers told me what the endpoint was - secret knowledge! Other than that, there's not much I can think to add, but I'm open to ideas!