## [MVC]ASP.NET Web API遇到了JSONP

### 參考網址 => [中文](https://dotblogs.com.tw/rainmaker/2012/09/12/74729)

### 參考網址 => [英文原版](https://weblog.west-wind.com/posts/2012/Apr/02/Creating-a-JSONP-Formatter-for-ASPNET-Web-API)

### 檔案路徑 => [JsonpFormatter.cs](http://172.22.36.2:8080/back_end/codesample/blob/master/JsonpFormatter.cs)

### **使用方法**
```csharp
    // 在global.asax.cs中加入
    protected void Application_Start()
    {
        var config = GlobalConfiguration.Configuration;
        config.Formatters.Insert(0, new JsonpFormatter());
    }

```



