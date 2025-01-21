using Dapper;
using Npgsql;

namespace BlazorSignalRApp.Service
{
    public class BatchService
    {
        // DIするDB接続オブジェクト
        private readonly NpgsqlConnection _connection;

        public BatchService(NpgsqlConnection connection)
        {
            // コンストラクタでDB接続をDIする
            _connection = connection;

            // アンダースコア区切りをパスカルケースに変換してマッピングしてくれる
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public async Task<string> BulkUpdateAsync(int loopCount)
        {
            // DIした接続を利用して、トランザクションを開始する
            using var transaction = await _connection.BeginTransactionAsync();
            try
            {
                // ストアドプロシージャの設定
                using var cmd = new NpgsqlCommand("refresh_pregnancy_report", _connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // INパラメータの設定
                cmd.Parameters.AddWithValue("loop_count", loopCount);

                // OUTパラメータの設定
                var outParam = new NpgsqlParameter("message", NpgsqlTypes.NpgsqlDbType.Text)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                // ストアドプロシージャを実行
                cmd.ExecuteNonQuery();

                // コミット
                await transaction.CommitAsync();

                return $"{outParam.Value}";
            }
            catch (Exception ex)
            {
                // エラー発生時はロールバック
                await transaction.RollbackAsync();

                // エラーハンドリングを追加
                throw new Exception("データ更新に失敗しました", ex);
            }
        }
    }
}