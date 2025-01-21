using BlazorSignalRApp.Service;
using Microsoft.AspNetCore.SignalR;

namespace BlazorSignalRApp.Hubs;

// コンストラクタでサービスをDIする
public class BatchHub(BatchService batchService) : Hub
{
    public async Task BulkUpdateAsync(int loopCount)
    {
        // DIしたサービスのメソッド実行
        string message = await batchService.BulkUpdateAsync(loopCount);

        // 処理完了後、クライアント側の関数をCallし、通知する（全員ではなく、呼び出し元のみ）
        await Clients.Caller.SendAsync("ReceiveMessage", message);
    }
}