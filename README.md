# ASP.NET Core DI 注入生命週期實戰練習（Controller 篇）

本專案是一個用來深入理解並親手驗證 ASP.NET Core 相依性注入（DI, Dependency Injection）三大生命週期的練習實驗室。透過在同一個 HTTP 請求中注入多次服務，並觀察唯一的實例識別碼（Guid）與計數器數值，能完美釐清 **Transient**、**Scoped** 與 **Singleton** 的行為差異。

---

## 🔬 實驗核心程式碼

### 1. 服務層與計數器定義
我們設計了一個基礎的 `ICounter` 計數器服務，以及一個會依賴計數器的 `ISomeService` 業務服務。

```csharp
// 基礎計數器介面與實作
public interface ICounter {
    Guid Id { get; }
    int Value { get; }
    void Increment();
}

public class Counter : ICounter {
    public Guid Id { get; } = Guid.NewGuid();
    public int Value { get; private set; }
    public void Increment() => Value++;
}

// 另一個會用到計數器的服務介面與實作
public interface ISomeService {
    ICounter Counter { get; }
    void DoSomething();
}

public class SomeService : ISomeService {
    public ICounter Counter { get; }
    
    // 這裡同樣會由 DI 容器注入 ICounter
    public SomeService(ICounter counter) {
        Counter = counter;
    }
    
    public void DoSomething() => Counter.Increment();
}
```

### 2. 測試控制器 (DiTestController)
在 Controller 的建構函式中，我們**同時注入** `ICounter` 和 `ISomeService`。當 API 被呼叫時，兩邊都會對計數器進行累加。

```csharp
using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiTestController : ControllerBase
{
    private readonly ICounter _counterFromController;
    private readonly ISomeService _someService;

    public DiTestController(ICounter counterFromController, ISomeService someService)
    {
        _counterFromController = counterFromController;
        _someService = someService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // 1. 由 Controller 直接操控的計數器加 1
        _counterFromController.Increment();

        // 2. 由 SomeService 內操控的計數器加 1
        _someService.DoSomething();

        // 3. 回傳兩邊看到的 Guid 與數值
        return Ok(new
        {
            ControllerCounter = new { 
                Id = _counterFromController.Id, 
                Value = _counterFromController.Value 
            },
            ServiceCounter = new { 
                Id = _someService.Counter.Id, 
                Value = _someService.Counter.Value 
            }
        });
    }
}
```

---

## 🛠️ 測試操作方式

請依序打開專案中的 `Program.cs`，每次**只開啟其中一組註冊方式**（將另外兩組註解掉），接著啟動專案，並使用瀏覽器或 Postman 持續重整造訪 API 端點：`https://localhost:xxxx/api/ditest`。

---

## 📊 實驗結果與原因分析

### 🧪 測試一：兩邊都註冊為 `AddTransient` (暫時性)

在 `Program.cs` 中設定：
```csharp
builder.Services.AddTransient<ICounter, Counter>();
builder.Services.AddTransient<ISomeService, SomeService>();
```

*   **實驗結果：**
    *   `ControllerCounter` 和 `ServiceCounter` 的 `Id` **完全不同**。
    *   兩者的 `Value` **都是 1**。
*   **原因分析：**
    *   因為註冊為 Transient（暫時性），只要 DI 容器看到有人需要 `ICounter`（Controller 實例化時需要一次、SomeService 實例化時也需要一次），它就會當場隨手 `new` 一個全新型態的實例給對方。這兩個實例各自獨立、互不干涉，過著各自的生活，因此各自呼叫 `Increment()` 後的數值都是 1。

---

### 🧪 測試二：兩邊都註冊為 `AddScoped` (範圍性)

在 `Program.cs` 中設定：
```csharp
builder.Services.AddScoped<ICounter, Counter>();
builder.Services.AddScoped<ISomeService, SomeService>();
```

*   **實驗結果：**
    *   **第一次按重整（第 1 個 HTTP 請求）：** 兩邊的 `Id` **一模一樣**，而且 `Value` **同時變成 2**！
    *   **第二次按重整（第 2 個 HTTP 請求）：** 兩邊的 `Id` 變成了另一組新的（但兩邊依然維持一樣），`Value` **重新變回 2**。
*   **原因分析：**
    *   在同一個 HTTP 請求（Request）的生命週期範圍內，不論有多少個元件需要 `ICounter`，DI 容器都只會建立並給予它們**同一個記憶體實例**。
    *   因此，當 Controller 將其加 1，Service 內部對同一個實例又加 1，總共就累加成了 2。一旦頁面重整（開啟全新的 HTTP 請求），前一個請求的 Scope 就會結束並將實例銷毀，並在新的 Scope 內重新建立一組全新的共用實例。

---

### 🧪 測試三：兩邊都註冊為 `AddSingleton` (單例性)

在 `Program.cs` 中設定：
```csharp
builder.Services.AddSingleton<ICounter, Counter>();
builder.Services.AddSingleton<ISomeService, SomeService>();
```

*   **實驗結果：**
    *   **第一次按重整：** 兩邊 `Id` 一樣，`Value` 是 2。
    *   **第二次按重整：** 兩邊 `Id` 完全沒變，`Value` 變成 4。
    *   **第三次按重整：** 兩邊 `Id` 依然沒變，`Value` 變成 6。
    *   *註：不論換瀏覽器、開無痕視窗、甚至用別台電腦連進來，Id 都完全一樣，且 Value 會無上限累加。*
*   **原因分析：**
    *   Singleton（單例）代表從應用程式（Web Server）啟動的那一刻起，整個伺服器記憶體空間裡就**只會存在唯一一個 `Counter` 實例**。
    *   不論經歷多少次 HTTP 請求、換了多少個不同的使用者連線進來，大家永遠都在共用這同一個實例，所以識別碼永遠不變，而數值則會隨著每次的 API 觸發而無止境地一直累加下去。

---

## 🎯 總結與記憶口訣
1. **Transient**：多子多孫。要一次給一個新的，不與任何人共享。
2. **Scoped**：同舟共濟。在同一個 HTTP 請求內大家都是生命共同體，用同一個；換請求就換新。
3. **Singleton**：天長地久。從頭到尾只有一個，全伺服器、全使用者共享，直到伺服器關機為止。