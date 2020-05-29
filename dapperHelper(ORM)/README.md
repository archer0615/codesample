## [Dapper官方線上文件](https://dapper-tutorial.net/zh-TW/tutorial/1000167/-----)

## **重要!!! 請用nuget 抓取 Dapper , Dapper.Contrib 兩個套件才可以使用**
---
``` csharp
//修改連線字串
protected string ConnStr = WebConfigurationManager.AppSettings["ROG_ConnStr"];
```
---

####  QueryOne

* 使用 DapperQueryFirstOrDefault
* 查詢第一筆資料 (無結果回傳Null)
``` csharp
public UserInfoViewModel GetUserInfo(string userId)
{
    UserInfoViewModel result = new UserInfoViewModel();
    string sql = @"select * from [dbo].[ROG_Account] where [status] = 1 and userId = @userId";
    result = DapperQueryFirstOrDefault<UserInfoViewModel>(sql, new { userId });
    return result;
}

public class UserInfoViewModel
{
    public int AccountId { get; set; }
    public string UserId { get; set; }
    public int Role { get; set; }
}
```
#### QueryList

* 使用 DapperQuery

``` csharp
public List<UserCountryViewModel> GetUserCountry(int accountId)
{
    string sql = @"
        select aw.WebsiteId, (w.WebsiteName+' / ' + w.[Language] )as WebsiteName
        from ROG_Account_Website as aw 
        inner join ROG_Website as w on aw.WebsiteId= w.WebsiteId and w.[Status] != 0
        where aw.AccountId = @AccountId
    ";
    return DapperQuery<UserCountryViewModel>(sql, new { accountId = accountId }).ToList();
}

public class UserCountryViewModel
{
    public int WebsiteId { get; set; }
    public string WebsiteName { get; set; }
}
```

#### MultiQuery One, List

* 使用 DapperSingleMultiQuery
* 需使用Tuple<>

``` csharp

```

#### MultiQuery One, One, List

* 使用 DapperDoubleSingleMultiQuery
* 需使用Tuple<>
``` csharp

```

#### MultiQuery One, List, List

* 使用 DapperSingleOneDoubleMultiQuery
* 需使用Tuple<>
``` csharp

```

#### MultiQuery List, List

* 使用 DapperMultiQuery
* 需使用Tuple<>
``` csharp

```

#### Excute NonQuery

* 使用 DapperExecuteNonQuery
* Excute Non-Query SQL，允許一次傳入多道SQL指令
* return 影響資料筆數

``` csharp
    string tSQL = @"
            EXECUTE sp_Write_API_Access_Log  
            @Api_key,@Status_Code,@Token,@FuntionName,@Requestor
        ";
   int result = DapperExecuteNonQuery(tSQL, new { Api_key = api_key, Status_Code = statusCode, Token = token, FuntionName = runName, Requestor = requestor});
```

#### Excute Scalar
* 使用 DapperExecuteScalar
* ExecuteScalar，執行查詢並傳回第一個資料列的第一個資料行中查詢所傳回的結果
* return 執行回覆結果
``` csharp

```

#### Insert
* 使用 DapperInsertReturnBool, DapperInsert   
* 新增單筆或多筆
* 只要丟List進去 就是新增多筆
* 重點! 新增的class 須掛上 [Table("Table名稱")]
* 只會新增設定好且有裝值的欄位
* 此案例 部分欄位會自動補上 所以不宣告Id, Status, Cdt, Udt
* Return The ID(primary key) of the newly inserted record if it is identity using the defined type, otherwise null
``` csharp
 var insertData = new InsertHotProduct()
 {
     LevelTagId = postData.LevelTagId,
     WebsiteId = websiteId,
     ProductId = postData.ProductId,
     MediaId = postData.MediaId,
     Priority = postData.Priority,
     EditAccountId = editAccountId,
 };
 return DapperInsertReturnBool(insertData);

[Table("ROG_HotProduct")]
public class InsertHotProduct
{
    public int LevelTagId { get; set; }
    public int WebsiteId { get; set; }
    public int ProductId { get; set; }
    public int Priority { get; set; }
    public string MediaId { get; set; }
    public int EditAccountId { get; set; }
}

[Table("ROG_HotProduct")]
public class ROG_HotProduct//DB Table示意
{
    public int Id { get; set; }//DB有設定識別規格(自動增加流水號)
    public int LevelTagId { get; set; }
    public int WebsiteId { get; set; }
    public int Priority { get; set; }
    public int ProductId { get; set; }
    public string PartNo { get; set; }
    public string MediaId { get; set; }
    public bool Status { get; set; }//DB有設定預設值false
    public DateTime Cdt { get; set; }//DB有設定預設值getdate()
    public DateTime Udt { get; set; }//DB有設定預設值getdate()
    public int EditAccountId { get; set; }
}
```

#### Update
* 使用 DapperUpdate 
* 修改單筆或多筆
* 重點! 修改的class 須掛上 [Table("Table名稱")]
* 只會修改宣告的欄位
* Pkey 需要 掛上[Key] 讓套件自己尋找從DB找到要修改的資料
* 如果是複合主鍵 則都掛Key 就可以 否則會變成更新多筆
``` csharp
var updateData = new UpdateProductStatus()
{
    ProductId = productId,
    Status = productStatus,
    EditAccountId = accountId,
    Udt = DateTime.Now
};
bool result = DapperUpdate(updateData);

[Table("ROG_Product")]
public class UpdateProductStatus
{
    [Key]
    public int ProductId { get; set; }
    public int Status { get; set; }
    public DateTime Udt { get; set; }
    public int EditAccountId { get; set; }
}
```

#### Update Mutil
* 使用 DapperUpdateMutil 
* 可以更新不同Table資料
* 重點! 修改的class 須掛上 [Table("Table名稱")]
* 只會修改宣告的欄位
* Pkey 需要 掛上[Key] 讓套件自己尋找從DB找到要修改的資料
* 如果是複合主鍵 則都掛Key 就可以 否則會變成更新多筆
``` csharp
var updateHPData = new UpdateHotProduct()
{
    Id = postData.Id,
    WebsiteId = websiteId,
    ProductId = postData.ProductId,
    MediaId = postData.MediaId,
    Udt = DateTime.Now,
    EditAccountId = editAccountId,
};
var updateDataLevelTag = new UpdateHotProductDescription()
{
    Description = postData.Description,
    ProductId = postData.ProductId,
    WebsiteId = websiteId,
    Udt = DateTime.Now,
    EditAccountId = editAccountId,
};
return DapperUpdateMutil(updateHPData, updateDataLevelTag);

[Table("ROG_HotProduct")]
public class UpdateHotProduct
{
    [Key]
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public int ProductId { get; set; }
    public string MediaId { get; set; }
    public DateTime Udt { get; set; }
    public int EditAccountId { get; set; }
}
[Table("ROG_LevelTag")]
public class UpdateHotProductDescription
{
    [Key]
    public int WebsiteId { get; set; }
    [Key]
    public int ProductId { get; set; }
    public string Description { get; set; }
    public DateTime Udt { get; set; }
    public int EditAccountId { get; set; }
}
```

#### Delete one
* 使用 DapperDelete
* 重點! 刪除的class 須掛上 [Table("Table名稱")]
* Pkey 需要 掛上[Key] 讓套件自己尋找從DB找到要刪除的資料
* 如果是複合主鍵 則都掛Key 就可以
``` csharp

```

#### Delete List
* 使用 DapperDeleteList
* 重點! 刪除的class 須掛上 [Table("Table名稱")]
* Pkey 需要 掛上[Key] 讓套件自己尋找從DB找到要刪除的資料
* 如果是複合主鍵 則都掛Key 就可以
``` csharp

```