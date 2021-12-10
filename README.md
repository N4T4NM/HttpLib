# HttpLib
C# Http/Https library

### Example

```CSharp
Using HttpLib;

public class Example {
  public static void Main() {
    HttpStream stream = new(); //Create new HttpStream instance
    stream.OpenStream("https://target.url"); //Connect to host
    
    //Sending request
    HttpRequest request = new(stream);
    request.Headers["User-Agent"] = "User agent"; //Set headers
    request.Method = "POST"; //Set Method
    request.BodyStream.Write(Encoding.UTF8.GetBytes("Your data here")); //Write data
    
    HttpResponse response = request.Send(); //Send request and get response
    Console.WriteLine(response.Status); //Get status
    response.HttpStream.BaseStream.Read(...); //Read response body mannually
    byte[] body = response.ReadFullBody(); //Read full body (Parses Transfer-Encoding: chunked)
    
    stream.Url = "..."; //Change url for another request, only valid for same host
  }
}
```
