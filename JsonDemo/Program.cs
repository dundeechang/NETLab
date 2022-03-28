using JsonDemo;

// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

Result result = new()
{
    Code = ResultCode.OK,
    Message = string.Empty,
    Data = new StockCreditDto()
    {
        BranchId = "8880",
        Account = "1234567",
        StockId = "2330",
        CreditLimit = 90_000_000,
        NonrestrictedUsage = 10_000_000,
        StockLoanUsage = 0,
        RestCredit = 80_000_000
    }
};
string json = System.Text.Json.JsonSerializer.Serialize(result);
System.Console.WriteLine(json);

Result result0 = new()
{
    Code = ResultCode.OK,
    Message = string.Empty,
    Data = null
};
string json0 = System.Text.Json.JsonSerializer.Serialize(result0);
System.Console.WriteLine(json0);

Result resultErr = new()
{
    Code = ResultCode.Error,
    Message = "不合法的客戶端",
    Data = null
};
string jsonErr = System.Text.Json.JsonSerializer.Serialize(resultErr);
System.Console.WriteLine(jsonErr);

Console.Read();